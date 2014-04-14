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

        public SqlClientProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public bool CollectionExists(string name)
        {
            throw new NotImplementedException();
        }

        public IDocumentCollection GetCollection(string name)
        {
            throw new NotImplementedException();
        }

        public Generic.IDocumentCollection<T> GetCollection<T>()
        {
            throw new NotImplementedException();
        }

        public Generic.IDocumentCollection<T> GetCollection<T>(string name)
        {
            throw new NotImplementedException();
        }

        public bool CreateCollection(string name, StorageFormat format)
        {
            throw new NotImplementedException();
        }

        public bool CreateCollection<T>(StorageFormat format)
        {
            throw new NotImplementedException();
        }

        public bool DeleteCollection(string name)
        {
            throw new NotImplementedException();
        }

        public bool DeleteCollection<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CollectionInfo> CollectionInfos()
        {
            throw new NotImplementedException();
        }

        public JsonRecord GetJsonRecord(Guid id, string collectionName)
        {
            throw new NotImplementedException();
        }

        public BsonRecord GetBsonRecord(Guid id, string collectionName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<JsonRecord> EnumerateJsonCollection(string collectionName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BsonRecord> EnumerateBsonCollection(string collectionName)
        {
            throw new NotImplementedException();
        }

        public bool AddOrUpdateRecord(BsonRecord record, string collectionName)
        {
            throw new NotImplementedException();
        }

        public bool AddOrUpdateRecord(JsonRecord record, string collectionName)
        {
            throw new NotImplementedException();
        }

        public bool RemoveRecord(Guid id, string collectionName)
        {
            throw new NotImplementedException();
        }
    }
}
