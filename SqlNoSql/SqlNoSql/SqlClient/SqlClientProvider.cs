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
    using Dapper;
    using SqlNoSql.Data;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    public class SqlClientProvider : IDbProvider
    {
        private string connectionString;
        private IEnumerable<string> reservedNames = new[] { "_collections" };

        public SqlClientProvider(string connectionString)
        {
            this.connectionString = connectionString;
            this.CreateCollectionInfoTableIfNotExists();
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public bool CollectionExists(string name)
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                return connection.Query<int>("SELECT CAST(COUNT(*) AS INT) FROM _collections WHERE Name = @Name", new { Name = name }).Single() > 0;
            }
        }

        public IDocumentCollection<T> GetCollection<T>()
        {
            return this.GetCollection<T>(typeof(T).Name);
        }

        public IDocumentCollection<T> GetCollection<T>(string name)
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                var collectionInfo = connection.Query<CollectionInfo>(
                    "SELECT Name, Format FROM _collections WHERE Name = @Name",
                    new { Name = typeof(T).Name }).SingleOrDefault();
                if (collectionInfo != null)
                    return new DocumentCollection<T>(collectionInfo.Name, this, collectionInfo.Format);
                else
                    return null;
            }
        }

        public bool CreateCollection(string name, StorageFormat format)
        {
            if (reservedNames.Any(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new ArgumentException(
                    string.Format("{0} cannot be used, since it is a reserved name", name),
                    name);
            }
            if (this.CollectionExists(name))
            {
                throw new ArgumentException(
                    string.Format("A collection named {0} already exists", name),
                    name);
            }
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        if (format == StorageFormat.BSON)
                            this.CreateBsonTable(name, connection);
                        else
                            this.CreateJsonTable(name, connection);
                        connection.Execute("INSERT INTO _collections (Name, Format) VALUES (@Name, @Format)", new { Name = name, Format = format });
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
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
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        connection.Execute(string.Format("DROP TABLE {0}", name));
                        connection.Execute("DELETE FROM _collections WHERE Name = @Name", new { Name = name });
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool DeleteCollection<T>()
        {
            return this.DeleteCollection(typeof(T).Name);
        }

        public IEnumerable<CollectionInfo> CollectionInfos()
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                return connection.Query<CollectionInfo>("SELECT Name, Format FROM _collections");
            }
        }

        public JsonRecord GetJsonRecord(Guid id, string collectionName)
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                return connection.Query<JsonRecord>(
                    string.Format("SELECT Id, Data FROM {0} WHERE Id = @Id", collectionName),
                    new { Id = id }).SingleOrDefault();
            }
        }

        public BsonRecord GetBsonRecord(Guid id, string collectionName)
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                return connection.Query<BsonRecord>(
                    string.Format("SELECT Id, Data FROM {0} WHERE Id = @Id", collectionName),
                    new { Id = id }).SingleOrDefault();
            }
        }

        public IEnumerable<JsonRecord> EnumerateJsonCollection(string collectionName)
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                return connection.Query<JsonRecord>(
                    string.Format("SELECT Id, Data FROM {0}", collectionName),
                    buffered: false);
            }
        }

        public IEnumerable<BsonRecord> EnumerateBsonCollection(string collectionName)
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                return connection.Query<BsonRecord>(
                    string.Format("SELECT Id, Data FROM {0}", collectionName),
                    buffered: false);
            }
        }

        public bool AddOrUpdateRecord(BsonRecord record, string collectionName)
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                return connection.Execute(
                    string.Format("INSERT INTO {0} (Id, Data) VALUES (@Id, @Data)", collectionName),
                    record) > 0;
            }
        }

        public bool AddOrUpdateRecord(JsonRecord record, string collectionName)
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                return connection.Execute(
                    string.Format("INSERT INTO {0} (Id, Data) VALUES (@Id, @Data)", collectionName),
                    record) > 0;
            }
        }

        public bool RemoveRecord(Guid id, string collectionName)
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                return connection.Execute(
                    string.Format("DELETE FROM {0} WHERE Id = @Id", collectionName),
                    new { Id = id }) > 0;
            }
        }

        private void CreateJsonTable(string name, SqlConnection connection)
        {
            connection.Execute(string.Format(
                "CREATE TABLE {0} ( " +
                "Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL, " +
                "Data NVARCHAR(MAX) NOT NULL)", name));
        }

        private void CreateBsonTable(string name, SqlConnection connection)
        {
            connection.Execute(string.Format(
                "CREATE TABLE {0} ( " +
                "Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL, " +
                "Data VARBINARY(MAX) NOT NULL)", name));
        }

        private void CreateCollectionInfoTableIfNotExists()
        {
            using (var connection = this.GetConnection() as SqlConnection)
            {
                connection.Open();
                connection.Execute(
                    "IF (OBJECT_ID('_collections', 'U') IS NULL) " +
                    "BEGIN " +
                    "CREATE TABLE _collections ( " +
                    "Name NVARCHAR(450) PRIMARY KEY NOT NULL, " +
                    "Format NVARCHAR(MAX) NOT NULL) " +
                    "END");
            }
        }
    }
}
