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
    using System.Collections.Generic;

    /// <summary>
    /// A collection of BSON or JSON documents that can be deserialized to the specified type.
    /// </summary>
    /// <typeparam name="T">The type of objects contained in the collection</typeparam>
    public interface IDocumentCollection<T> : IEnumerable<KeyValuePair<Guid, T>>
    {
        /// <summary>
        /// Gets or sets the document with the provided id.
        /// </summary>
        /// <param name="id">The id of the document</param>
        T this[Guid id] { get; set; }

        /// <summary>
        /// The name of the collection.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The document format that data is saved in.
        /// </summary>
        StorageFormat Format { get; }

        /// <summary>
        /// Gets the document with the provided id
        /// </summary>
        /// <param name="id">The id of the document</param>
        T Find(Guid id);

        /// <summary>
        /// Iterates over the collection and returns the first document that passes the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        T Find(Func<T, bool> filter);

        /// <summary>
        /// Iterates over the collection and returns the first document and its id that passes the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        KeyValuePair<Guid, T> FindWithId(Func<T, bool> filter);

        /// <summary>
        /// Iterates over the collection and returns all documents that pass the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        ICollection<T> Filter(Func<T, bool> filter);

        /// <summary>
        /// Iterates over the collection and returns all documents and their ids that pass the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        ICollection<KeyValuePair<Guid, T>> FilterWithIds(Func<T, bool> filter);

        /// <summary>
        /// Adds or updates a document in the collection
        /// </summary>
        /// <param name="id">The id of the document</param>
        /// <param name="item">The object that is being stored in the collection</param>
        void AddOrUpdate(Guid id, T item);

        /// <summary>
        /// Removes the document with the specified id from the collection
        /// </summary>
        /// <param name="key">The id of the document</param>
        void Remove(Guid id);
    }
}
