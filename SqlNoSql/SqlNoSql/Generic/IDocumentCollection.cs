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

namespace SqlNoSql.Generic
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A collection of BSON or JSON documents that can be deserialized to the specified type.
    /// </summary>
    /// <typeparam name="T">The type of objects contained in the collection</typeparam>
    public interface IDocumentCollection<T> : IEnumerable<T>
    {
        T this[Guid key] { get; set; }
        string Name { get; }
        StorageFormat StorageFormat { get; }
        T Find(Guid key);
        T Find(Func<T, bool> filter);
        KeyValuePair<Guid, T> FindWithKey(Func<T, bool> filter);
        ICollection<T> Filter(Func<T, bool> filter);
        ICollection<KeyValuePair<Guid, T>> FilterWithKeys(Func<T, bool> filter);
        void AddOrUpdate(Guid key, T item);
        void Remove(Guid key);
    }
}
