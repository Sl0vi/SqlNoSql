﻿// The MIT License (MIT)
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
        
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="T:SqlNoSql.Sqlite.SqliteProvider"/> class.
        /// </summary>
        protected SqliteProvider(string connectionString)
        {
            this.connectionString = connectionString;
            CreateCollectionInfoTableIfNotExists();
        }
        
        /// <summary>
        /// Creates a new database connection
        /// </summary>
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
                    "FORMAT TEXT KEY NOT NULL)");
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }
        
        /// <summary>
        /// Checks if the collection exists in the database
        /// </summary>
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
        
        /// <summary>
        /// Returns a document collection for the given type
        /// </summary>
        public IDocumentCollection<T> GetCollection<T>()
        {
            return GetCollection<T>(typeof(T).Name);
        }

        /// <summary>
        /// Returns the document for the given type with the specified name
        /// </summary>
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
        
        /// <summary>
        /// Gets the number of items stored in the collection.
        /// </summary>
        public int GetItemCount(string collectionName)
        {
            var connection = GetConnection();
            try
            {
                return connection
                    .Query<int>(
                        string.Format(
                            "SELECT CAST(COUNT(*) AS INT) " +
                            "FROM [{0}]",
                            collectionName))
                    .Single();
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Creates a new collection with the specified name and storage format
        /// </summary>
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
        
        /// <summary>
        /// Creates a new collection with the name of the given type and objects
        /// are stored in
        /// the specified storage format
        /// </summary>
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
                    "CREATE TABLE [{0}] ( " +
                    "Id TEXT PRIMARY KEY NOT NULL, " +
                    "Data TEXT NOT NULL)",
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
                    "CREATE TABLE [{0}] ( " +
                    "Id TEXT PRIMARY KEY NOT NULL, " +
                    "Data TEXT NOT NULL)",
                    name),
                transaction: transaction);
        }

        /// <summary>
        /// Deletes the collection with the specified name
        /// </summary>
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
                        "DROP TABLE [{0}]",
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

        /// <summary>
        /// Deletes the collection with the name of the given type
        /// </summary>
        public bool DeleteCollection<T>()
        {
            return DeleteCollection(typeof(T).Name);
        }

        /// <summary>
        /// Returns a collection with information about all the collections in
        /// the document store
        /// </summary>
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

        /// <summary>
        /// Gets a JSON record from the database
        /// </summary>
        public JsonRecord GetJsonRecord(Guid id, string collectionName)
        {
            var connection = GetConnection();
            try
            {
                return connection
                    .Query<SqliteJsonRecord>(
                        string.Format(
                            "SELECT Id AS IdString, Data " +
                            "FROM [{0}] " +
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

        /// <summary>
        /// Gets a BSON record from the database
        /// </summary>
        public BsonRecord GetBsonRecord(Guid id, string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                return connection
                    .Query<SqliteBsonRecord>(
                        string.Format(
                            "SELECT Id AS IdString, Data " +
                            "FROM [{0}] " +
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

        /// <summary>
        /// Enumerates a collection of JSON documents
        /// </summary>
        public IEnumerable<JsonRecord> EnumerateJsonCollection(
            string collectionName)
        {
            var connection = this.GetConnection();
            try
            {
                foreach (var record in connection.Query<SqliteJsonRecord>(
                    string.Format(
                        "SELECT Id AS IdString, Data " +
                        "FROM [{0}]",
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

        /// <summary>
        /// Enumerates a collection of BSON documents
        /// </summary>
        public IEnumerable<BsonRecord> EnumerateBsonCollection(
            string collectionName)
        {
            var connection = GetConnection();
            try
            {
                foreach (var record in connection.Query<SqliteBsonRecord>(
                    string.Format(
                        "SELECT Id AS IdString, Data " +
                        "FROM [{0}]",
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

        /// <summary>
        /// Adds or updates the passed in BSON document
        /// </summary>
        public bool AddOrUpdateRecord(JsonRecord record, string collectionName)
        {
            return AddOrUpdate(
                record.Id,
                record.Data,
                collectionName);
        }

        /// <summary>
        /// Adds or updates the passed in JSON document
        /// </summary>
        public bool AddOrUpdateRecord(BsonRecord record, string collectionName)
        {
            return AddOrUpdate(
                record.Id,
                record.Data,
                collectionName);
        }

        private bool AddOrUpdate<T>(
            Guid id,
            T data,
            string collectionName)
        {
            var connection = GetConnection();
            try
            {
                if (connection
                    .Query<int>(
                        string.Format(
                            "SELECT CAST(COUNT(*) AS INT) " +
                            "FROM [{0}] " +
                            "WHERE Id = @Id",
                            collectionName),
                        new { Id = id.ToString() },
                        transaction: GetOpenTransactionOrNull(connection))
                    .Single() == 0)
                {
                    return connection.Execute(
                        string.Format(
                            "INSERT INTO [{0}] (Id, Data) " +
                            "VALUES (@Id, @Data)",
                            collectionName),
                        new { Id = id.ToString(), Data = data },
                        transaction: GetOpenTransactionOrNull(connection)) > 0;
                }
                else
                {
                    return connection.Execute(
                        string.Format(
                            "UPDATE [{0}] SET " +
                            "Data = @Data " +
                            "WHERE Id = @Id",
                            collectionName),
                        new { Id = id, Data = data },
                        transaction: GetOpenTransactionOrNull(connection)) > 0;
                }
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Removes the record with the passed in id from the collection
        /// </summary>
        public bool RemoveRecord(Guid id, string collectionName)
        {
            var connection = GetConnection();
            try
            {
                return connection.Execute(
                    string.Format(
                        "DELETE FROM [{0}] WHERE Id = @Id",
                        collectionName),
                    new { Id = id.ToString() },
                    transaction: GetOpenTransactionOrNull(connection)) > 0;
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        public ITransaction BeginTransaction(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (Transaction != null)
                throw new InvalidOperationException(
                    "There is already an open transaction");
            var connection = GetConnection();
            var transaction = connection.BeginTransaction(
                isolationLevel);
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
