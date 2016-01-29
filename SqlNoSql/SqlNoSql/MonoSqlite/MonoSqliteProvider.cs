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

namespace SqlNoSql.MonoSqlite
{
    public class MonoSqliteProvider : IDbProvider
    {
        internal MonoSqliteTransaction Transaction { get; set; }
        
        public MonoSqliteProvider()
        {
        }

        #region IDbProvider implementation

        public bool CollectionExists(string name)
        {
            throw new System.NotImplementedException();
        }

        public IDocumentCollection<T> GetCollection<T>()
        {
            throw new System.NotImplementedException();
        }

        public IDocumentCollection<T> GetCollection<T>(string name)
        {
            throw new System.NotImplementedException();
        }

        public bool CreateCollection(string name, StorageFormat format)
        {
            throw new System.NotImplementedException();
        }

        public bool CreateCollection<T>(StorageFormat format)
        {
            throw new System.NotImplementedException();
        }

        public bool DeleteCollection(string name)
        {
            throw new System.NotImplementedException();
        }

        public bool DeleteCollection<T>()
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<SqlNoSql.Data.CollectionInfo> CollectionInfos()
        {
            throw new System.NotImplementedException();
        }

        public SqlNoSql.Data.JsonRecord GetJsonRecord(System.Guid id, string collectionName)
        {
            throw new System.NotImplementedException();
        }

        public SqlNoSql.Data.BsonRecord GetBsonRecord(System.Guid id, string collectionName)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<SqlNoSql.Data.JsonRecord> EnumerateJsonCollection(string collectionName)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<SqlNoSql.Data.BsonRecord> EnumerateBsonCollection(string collectionName)
        {
            throw new System.NotImplementedException();
        }

        public bool AddOrUpdateRecord(SqlNoSql.Data.BsonRecord record, string collectionName)
        {
            throw new System.NotImplementedException();
        }

        public bool AddOrUpdateRecord(SqlNoSql.Data.JsonRecord record, string collectionName)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveRecord(System.Guid id, string collectionName)
        {
            throw new System.NotImplementedException();
        }

        public ITransaction BeginTransaction()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}

