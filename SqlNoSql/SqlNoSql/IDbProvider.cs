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

namespace SqlNoSql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using SqlNoSql.Data;

    /// <summary>
    /// The interface used by providers for different databases.
    /// If you want to use the document store with an unsupported database
    /// then implement this interface.
    /// </summary>
    public interface IDbProvider
    {
        /// <summary>
        /// Checks if the collection exists in the database
        /// </summary>
        bool CollectionExists(string name);

        /// <summary>
        /// Returns a document collection for the given type
        /// </summary>
        IDocumentCollection<T> GetCollection<T>();

        /// <summary>
        /// Returns the document for the given type with the specified name
        /// </summary>
        IDocumentCollection<T> GetCollection<T>(string name);

        /// <summary>
        /// Gets the number of items stored in the collection.
        /// </summary>
        int GetItemCount(string collectionName);

        /// <summary>
        /// Creates a new collection with the specified name and storage format
        /// </summary>
        bool CreateCollection(string name, StorageFormat format);

        /// <summary>
        /// Creates a new collection with the name of the given type and objects
        /// are stored in
        /// the specified storage format
        /// </summary>
        bool CreateCollection<T>(StorageFormat format);

        /// <summary>
        /// Deletes the collection with the specified name
        /// </summary>
        bool DeleteCollection(string name);

        /// <summary>
        /// Deletes the collection with the name of the given type
        /// </summary>
        bool DeleteCollection<T>();

        /// <summary>
        /// Returns a collection with information about all the collections in
        /// the document store
        /// </summary>
        IEnumerable<CollectionInfo> CollectionInfos();

        /// <summary>
        /// Gets a JSON record from the database
        /// </summary>
        JsonRecord GetJsonRecord(Guid id, string collectionName);

        /// <summary>
        /// Gets a BSON record from the database
        /// </summary>
        BsonRecord GetBsonRecord(Guid id, string collectionName);

        /// <summary>
        /// Enumerates a collection of JSON documents
        /// </summary>
        IEnumerable<JsonRecord> EnumerateJsonCollection(string collectionName);

        /// <summary>
        /// Enumerates a collection of BSON documents
        /// </summary>
        IEnumerable<BsonRecord> EnumerateBsonCollection(string collectionName);

        /// <summary>
        /// Adds or updates the passed in BSON document
        /// </summary>
        bool AddOrUpdateRecord(BsonRecord record, string collectionName);

        /// <summary>
        /// Adds or updates the passed in JSON document
        /// </summary>
        bool AddOrUpdateRecord(JsonRecord record, string collectionName);

        /// <summary>
        /// Removes the record with the passed in id from the collection
        /// </summary>
        bool RemoveRecord(Guid id, string collectionName);

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        ITransaction BeginTransaction(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}
