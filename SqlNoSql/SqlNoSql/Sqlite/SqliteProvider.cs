// The MIT License (MIT)
//
// Copyright (c) 2014-2016 Bernhard Johannessen
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace SqlNoSql.Sqlite
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Dapper;
    using Data;

    /// <summary>
    /// This DbProvider provides support for sqlite databases using
    /// Mono.Data.Sqlite
    /// </summary>
    public abstract class SqliteProvider : IDbProvider
    {
        private string connectionString;
        private string[] reservedNames = new[] { "_collections" };

        internal SqliteTransaction Transaction { get; set; }
        
        protected SqliteProvider(string connectionString)
        {
            this.connectionString = connectionString;
            CreateCollectionInfoTableIfNotExists();
        }

        protected abstract IDbConnection NewConnection(
            string connectionString);

        private IDbConnection GetConnection()
        {
            if (Transaction != null)
                return Transaction.Connection;
            else
            {
                var connection = NewConnection(connectionString);
                connection.Open();
                return connection;
            }
        }

        private void ReleaseConnection(IDbConnection connection)
        {
            if (Transaction == null ||
                !object.ReferenceEquals(
                    Transaction.Connection,
                    connection))
            {
                connection.Close();
            }
        }

        private void CreateCollectionInfoTableIfNotExists()
        {
            var connection = GetConnection();
            try
            {
                connection.Execute(
                    "CREATE TABLE IF NOT EXISTS _collections ( " +
                    "Name TEXT PRIMARY KEY NOT NULL, " +
                    "FORMAT TEXT PRIMARY KEY NOT NULL)");
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public bool CollectionExists(string name)
        {
            var connection = GetConnection();
            try
            {
                return connection
                    .Query<int>(
                        "SELECT CAST(COUNT(*) AS INT) " +
                        "FROM _collections " +
                        "WHERE Name = @Name",
                        new { Name = name })
                    .Single() > 0;
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public IDocumentCollection<T> GetCollection<T>()
        {
            return GetCollection<T>(typeof(T).Name);
        }

        public IDocumentCollection<T> GetCollection<T>(string name)
        {
            var connection = GetConnection();
            try
            {
                var collectionInfo = connection
                    .Query<CollectionInfo>(
                        "SELECT Name, Format " +
                        "FROM _collections " +
                        "WHERE Name = @Name",
                        new { Name = name })
                    .SingleOrDefault();
                if (collectionInfo != null)
                    return new DocumentCollection<T>(
                        collectionInfo.Name,
                        this,
                        collectionInfo.Format);
                return null;
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public bool CreateCollection(string name, StorageFormat format)
        {
            if (reservedNames.Any(x =>
                x.Equals(name, StringComparison.InvariantCulture)))
                throw new ArgumentException(
                    string.Format(
                        "'{0}' cannot be used, since it is a reserved name",
                        name),
                    name);
            if (CollectionExists(name))
                throw new ArgumentException(
                    string.Format(
                        "A collection named '{0}' already exists",
                        name),
                    name);
            var isNewTransaction = Transaction == null;
            var transaction = isNewTransaction ?
                BeginTransaction() as SqliteTransaction :
                Transaction;
            try
            {
                if (format == StorageFormat.JSON)
                    CreateJsonTable(
                        name,
                        transaction.Connection,
                        transaction.Transaction);
                else
                    CreateBsonTable(
                        name,
                        transaction.Connection,
                        transaction.Transaction);
                transaction.Connection.Execute(
                    "INSERT INTO _collections (Name, Format) " +
                    "VALUES (@Name, @Format)",
                    new { Name = name, Format = format.ToString() },
                    transaction: transaction.Transaction);
                if (isNewTransaction)
                    transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (isNewTransaction)
                    transaction.Dispose();
            }
        }

        public bool CreateCollection<T>(StorageFormat format)
        {
            return CreateCollection(typeof(T).Name, format);
        }

        private void CreateJsonTable(
            string name,
            IDbConnection connection,
            IDbTransaction transaction)
        {
            connection.Execute(
                string.Format(
                    "CREATE TABLE {0} ( " +
                    "Id TEXT PRIMARY KEY NOT NULL, " +
                    "Data TEXT NOT NULL",
                    name),
                transaction: transaction);
        }

        private void CreateBsonTable(
            string name,
            IDbConnection connection,
            IDbTransaction transaction)
        {
            connection.Execute(
                string.Format(
                    "CREATE TABLE {0} ( " +
                    "Id TEXT PRIMARY KEY NOT NULL, " +
                    "Data TEXT NOT NULL",
                    name),
                transaction: transaction);
        }

        public bool DeleteCollection(string name)
        {
            if (!CollectionExists(name))
                return true;
            var isNewTransaction = Transaction == null;
            var transaction = isNewTransaction ?
                BeginTransaction() as SqliteTransaction :
                Transaction;
            try
            {
                transaction.Connection.Execute(
                    string.Format(
                        "DROP TABLE {0}",
                        name),
                    transaction: transaction.Transaction);
                transaction.Connection.Execute(
                    "DELETE FROM _collections WHERE Name = @Name",
                    new { Name = name },
                    transaction: transaction.Transaction);
                if (isNewTransaction)
                    transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (isNewTransaction)
                    transaction.Dispose();
            }
        }

        public bool DeleteCollection<T>()
        {
            return DeleteCollection(typeof(T).Name);
        }

        public IEnumerable<CollectionInfo> CollectionInfos()
        {
            var connection = GetConnection();
            try
            {
                return connection.Query<CollectionInfo>(
                    "SELECT Name, Format FROM _collections",
                    transaction: GetOpenTransactionOrNull(connection));
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public JsonRecord GetJsonRecord(Guid id, string collectionName)
        {
            var connection = GetConnection();
            try
            {
                return connection
                    .Query<SqliteJsonRecord>(
                        string.Format(
                            "SELECT Id AS IdString, Data " +
                            "FROM {0} " +
                            "WHERE Id = @Id",
                            collectionName),
                        new { Id = id.ToString() },
                        transaction: GetOpenTransactionOrNull(connection))
                    .SingleOrDefault();
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public BsonRecord GetBsonRecord(Guid id, string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                return connection
                    .Query<SqliteBsonRecord>(
                        string.Format(
                            "SELECT Id AS IdString, Data " +
                            "FROM {0} " +
                            "WHERE Id = @Id",
                            collectionName),
                        new { Id = id.ToString() },
                        transaction: GetOpenTransactionOrNull(connection))
                    .SingleOrDefault();
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public IEnumerable<JsonRecord> EnumerateJsonCollection(
            string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                foreach (var record in connection.Query<SqliteJsonRecord>(
                    string.Format(
                        "SELECT Id AS IdString, Data " +
                        "FROM {0}",
                        collectionName),
                    transaction: GetOpenTransactionOrNull(connection),
                    buffered: false))
                {
                    yield return record;
                }
                yield break;
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public IEnumerable<BsonRecord> EnumerateBsonCollection(
            string collectionName)
        {
            var connection = GetConnection();
            try
            {
                foreach (var record in connection.Query<SqliteBsonRecord>(
                    string.Format(
                        "SELECT Id AS IdString, Data " +
                        "FROM {0}",
                        collectionName),
                    transaction: GetOpenTransactionOrNull(connection),
                    buffered: false))
                {
                    yield return record;
                }
                yield break;
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public bool AddOrUpdateRecord(JsonRecord record, string collectionName)
        {
            var jsonRecord = record as SqliteJsonRecord;
            if (jsonRecord == null)
                jsonRecord = new SqliteJsonRecord
                {
                    Id = record.Id,
                    Data = record.Data
                };
            return AddOrUpdate(
                (IRecord<string>)jsonRecord,
                collectionName);
        }

        public bool AddOrUpdateRecord(BsonRecord record, string collectionName)
        {
            var bsonRecord = record as SqliteBsonRecord;
            if (bsonRecord == null)
                bsonRecord = new SqliteBsonRecord
                {
                    Id = record.Id,
                    Data = record.Data
                };
            return AddOrUpdate(
                (IRecord<byte>)bsonRecord,
                collectionName);
        }

        private bool AddOrUpdate<T>(
            IRecord<T> record,
            string collectionName)
        {
            var connection = GetConnection();
            try
            {
                if (connection
                    .Query<int>(
                        string.Format(
                            "SELECT CAST(COUNT(*) AS INT) " +
                            "FROM {0} " +
                            "WHERE Id = @Id",
                            collectionName),
                        new { Id = record.Id.ToString() },
                        transaction: GetOpenTransactionOrNull(connection))
                    .Single() == 0)
                {
                    return connection.Execute(
                        string.Format(
                            "INSERT INTO {0} (Id, Data) " +
                            "VALUES (@IdString, @Data)",
                            collectionName),
                        record,
                        transaction: GetOpenTransactionOrNull(connection)) > 0;
                }
                else
                {
                    return connection.Execute(
                        string.Format(
                            "UPDATE {0} SET " +
                            "Data = @Data " +
                            "WHERE Id = @IdString",
                            collectionName),
                        record,
                        transaction: GetOpenTransactionOrNull(connection)) > 0;
                }
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public bool RemoveRecord(Guid id, string collectionName)
        {
            var connection = GetConnection();
            try
            {
                return connection.Execute(
                    string.Format(
                        "DELETE FROM {0} WHERE Id = @Id",
                        collectionName),
                    new { Id = id.ToString() },
                    transaction: GetOpenTransactionOrNull(connection)) > 0;
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        public ITransaction BeginTransaction()
        {
            if (Transaction != null)
                throw new InvalidOperationException(
                    "There is already an open transaction");
            var connection = GetConnection();
            var transaction = connection.BeginTransaction(
                IsolationLevel.ReadCommitted);
            return Transaction = new SqliteTransaction(
                this,
                transaction);
        }

        private IDbTransaction GetOpenTransactionOrNull(
            IDbConnection connection)
        {
            return Transaction != null &&
                object.ReferenceEquals(
                    Transaction.Connection,
                    connection) ?
                Transaction.Transaction :
                null;
        }
    }
}
