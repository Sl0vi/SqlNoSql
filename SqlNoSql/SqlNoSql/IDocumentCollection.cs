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

namespace SqlNoSql
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A dynamic document collection that can be used to store any kind of document.
    /// </summary>
    public interface IDocumentCollection : IEnumerable
    {
        /// <summary>
        /// Gets or sets the document with the specified id.
        /// </summary>
        /// <param name="id">The id of the document</param>
        dynamic this[Guid id] { get; set; }

        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the document format used for storing objects in the collection.
        /// </summary>
        StorageFormat Format { get; }

        /// <summary>
        /// Gets the document with the specified id.
        /// </summary>
        /// <param name="id">The id of the document</param>
        dynamic Find(Guid id);

        /// <summary>
        /// Iterates over the collection and returns the first document that passes the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        dynamic Find(Func<dynamic, bool> filter);

        /// <summary>
        /// Iterates over the collection and returns the first document and its id that passes the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        KeyValuePair<Guid, dynamic>? FindWithKey(Func<dynamic, bool> filter);

        /// <summary>
        /// Iterates over the collection and returns all documents that pass the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        ICollection<dynamic> Filter(Func<dynamic, bool> filter);

        /// <summary>
        /// Iterates over the collection and returns all documents and their ids that pass the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        ICollection<KeyValuePair<Guid, dynamic>> FilterWithKeys(Func<dynamic, bool> filter);

        /// <summary>
        /// Adds or updates a document in the collection
        /// </summary>
        /// <param name="id">The key of the record</param>
        /// <param name="item">The object that is being stored in the collection</param>
        void AddOrUpdate(Guid id, dynamic item);

        /// <summary>
        /// Removes the document with the specified id from the collection
        /// </summary>
        /// <param name="id">The key of the document</param>
        void Remove(Guid id);
    }
}
