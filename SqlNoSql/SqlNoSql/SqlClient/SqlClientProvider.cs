// The MIT License (MIT)
//
// Copyright (c) 2014 Bernhard Johannessen
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace SqlNoSql.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using Dapper;
    using SqlNoSql.Data;

    /// <summary>
    /// This DbProvides provides support for Microsoft SQL Server databases.
    /// </summary>
    internal class SqlClientProvider : IDbProvider
    {
        private string connectionString;
        private IEnumerable<string> reservedNames = new[] { "_collections" };

        internal SqlClientTransaction Transaction { get; set; }

        public SqlClientProvider(string connectionString)
        {
            this.connectionString = connectionString;
            this.Transaction = null;
            this.CreateCollectionInfoTableIfNotExists();
        }

        private IDbConnection GetConnection()
        {
            if (this.Transaction != null)
                return this.Transaction.Connection;
            else
            {
                var connection = new SqlConnection(connectionString);
                connection.Open();
                return connection;
            }
        }

        private void ReleaseConnection(IDbConnection connection)
        {
            if (this.Transaction == null && !object.ReferenceEquals(this.Transaction.Connection, connection))
                connection.Close();
        }

        public bool CollectionExists(string name)
        {
            var connection = this.GetConnection();
            try
            {
                return connection.Query<int>("SELECT CAST(COUNT(*) AS INT) FROM _collections WHERE Name = @Name", 
                    new { Name = name },
                    transaction: this.GetOpenTransactionOrNull(connection)).Single() > 0;
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        public IDocumentCollection<T> GetCollection<T>()
        {
            return this.GetCollection<T>(typeof(T).Name);
        }

        public IDocumentCollection<T> GetCollection<T>(string name)
        {
            var connection = this.GetConnection();
            try
            {
                var collectionInfo = connection.Query<CollectionInfo>(
                    "SELECT Name, Format FROM _collections WHERE Name = @Name",
                    new { Name = typeof(T).Name },
                    transaction: this.GetOpenTransactionOrNull(connection))
                    .SingleOrDefault();
                if (collectionInfo != null)
                    return new DocumentCollection<T>(collectionInfo.Name, this, collectionInfo.Format);
                else
                    return null;
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        public bool CreateCollection(string name, StorageFormat format)
        {
            if (reservedNames.Any(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new ArgumentException(
                    string.Format("'{0}' cannot be used, since it is a reserved name", name),
                    name);
            }
            if (this.CollectionExists(name))
            {
                throw new ArgumentException(
                    string.Format("A collection named '{0}' already exists", name),
                    name);
            }
            var isNewTransaction = this.Transaction == null;
            var transaction = isNewTransaction ? this.BeginTransaction() as SqlClientTransaction : this.Transaction;
            try
            {
                if (format == StorageFormat.BSON)
                    this.CreateBsonTable(name, transaction.Connection, transaction.Transaction);
                else
                    this.CreateJsonTable(name, transaction.Connection, transaction.Transaction);
                transaction.Connection.Execute("INSERT INTO _collections (Name, Format) VALUES (@Name, @Format)", 
                    new { Name = name, Format = format.ToString() },
                    transaction: transaction.Transaction);
                if (isNewTransaction)
                    transaction.Commit();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (isNewTransaction)
                    transaction.Dispose();
            }
        }

        public bool CreateCollection<T>(StorageFormat format)
        {
            return this.CreateCollection(typeof(T).Name, format);
        }

        public bool DeleteCollection(string name)
        {
            if (!this.CollectionExists(name))
                return true;
            var isNewTransaction = this.Transaction == null;
            var transaction = isNewTransaction ? this.BeginTransaction() as SqlClientTransaction : this.Transaction;
            try
            {
                transaction.Connection.Execute(string.Format("DROP TABLE [{0}]", name), transaction: transaction.Transaction);
                transaction.Connection.Execute("DELETE FROM _collections WHERE Name = @Name", 
                    new { Name = name },
                    transaction: transaction.Transaction);
                if (isNewTransaction)
                    transaction.Commit();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (isNewTransaction)
                    transaction.Dispose();
            }
        }

        public bool DeleteCollection<T>()
        {
            return this.DeleteCollection(typeof(T).Name);
        }

        public IEnumerable<CollectionInfo> CollectionInfos()
        {
            var connection = this.GetConnection();
            try
            {
                return connection.Query<CollectionInfo>(
                    "SELECT Name, Format FROM _collections",
                    transaction: this.GetOpenTransactionOrNull(connection));
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        public JsonRecord GetJsonRecord(Guid id, string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                return connection.Query<JsonRecord>(
                    string.Format("SELECT Id, Data FROM [{0}] WHERE Id = @Id", collectionName),
                    new { Id = id },
                    transaction: this.GetOpenTransactionOrNull(connection)).SingleOrDefault();
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        public BsonRecord GetBsonRecord(Guid id, string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                return connection.Query<BsonRecord>(
                    string.Format("SELECT Id, Data FROM [{0}] WHERE Id = @Id", collectionName),
                    new { Id = id },
                    transaction: this.GetOpenTransactionOrNull(connection)).SingleOrDefault();
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        public IEnumerable<JsonRecord> EnumerateJsonCollection(string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                foreach (var record in connection.Query<JsonRecord>(
                    string.Format("SELECT Id, Data FROM [{0}]", collectionName),
                    transaction: this.GetOpenTransactionOrNull(connection),
                    buffered: false))
                {
                    yield return record;
                }
                yield break;
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        public IEnumerable<BsonRecord> EnumerateBsonCollection(string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                foreach (var record in connection.Query<BsonRecord>(
                    string.Format("SELECT Id, Data FROM [{0}]", collectionName),
                    transaction: this.GetOpenTransactionOrNull(connection),
                    buffered: false))
                {
                    yield return record;
                }
                yield break;
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        public bool AddOrUpdateRecord(BsonRecord record, string collectionName)
        {
            return this.AddOrUpdateRecord((IRecord<byte[]>)record, collectionName);
        }

        public bool AddOrUpdateRecord(JsonRecord record, string collectionName)
        {
            return this.AddOrUpdateRecord((IRecord<string>)record, collectionName);
        }

        private bool AddOrUpdateRecord<T>(IRecord<T> record, string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                if (connection.Query<int>(string.Format("SELECT CAST(COUNT(*) AS INT) FROM [{0}] WHERE Id = @Id", collectionName), record).Single() == 0)
                {
                    return connection.Execute(
                        string.Format("INSERT INTO [{0}] (Id, Data) VALUES (@Id, @Data)", collectionName),
                        record,
                        transaction: this.GetOpenTransactionOrNull(connection)) > 0;
                }
                else
                {
                    return connection.Execute(
                        string.Format("Update [{0}] SET Data = @Data WHERE Id = @Id", collectionName),
                        record,
                        transaction: this.GetOpenTransactionOrNull(connection)) > 0;
                }
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        public bool RemoveRecord(Guid id, string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                return connection.Execute(
                    string.Format("DELETE FROM [{0}] WHERE Id = @Id", collectionName),
                    new { Id = id },
                    transaction: this.GetOpenTransactionOrNull(connection)) > 0;
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        private void CreateJsonTable(string name, IDbConnection connection, IDbTransaction transaction)
        {
            connection.Execute(string.Format(
                "CREATE TABLE [{0}] ( " +
                "Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL, " +
                "Data NVARCHAR(MAX) NOT NULL)", name),
                transaction: transaction);
        }

        private void CreateBsonTable(string name, IDbConnection connection, IDbTransaction transaction)
        {
            connection.Execute(string.Format(
                "CREATE TABLE [{0}] ( " +
                "Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL, " +
                "Data VARBINARY(MAX) NOT NULL)", name),
                transaction: transaction);
        }

        private void CreateCollectionInfoTableIfNotExists()
        {
            var connection = this.GetConnection();
            try
            {
                connection.Execute(
                    "IF (OBJECT_ID('_collections', 'U') IS NULL) " +
                    "BEGIN " +
                    "CREATE TABLE [_collections] ( " +
                    "Name NVARCHAR(450) PRIMARY KEY NOT NULL, " +
                    "Format NVARCHAR(MAX) NOT NULL) " +
                    "END",
                    transaction: this.GetOpenTransactionOrNull(connection));
            }
            finally
            {
                this.ReleaseConnection(connection);
            }
        }

        public ITransaction BeginTransaction()
        {
            if (this.Transaction != null)
                throw new Exception("There is already an open transaction");
            var connection = this.GetConnection();
            var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
            return this.Transaction = new SqlClientTransaction(this, transaction);
        }

        private IDbTransaction GetOpenTransactionOrNull(IDbConnection connection)
        {
            return this.Transaction != null && object.ReferenceEquals(this.Transaction.Connection, connection) ? this.Transaction.Transaction : null;
        }
    }
}
