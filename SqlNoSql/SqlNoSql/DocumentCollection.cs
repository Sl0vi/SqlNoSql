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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using SqlNoSql.Data;

    public class DocumentCollection<T> : IDocumentCollection<T>
    {
        private IDbProvider provider;

        /// <summary>
        /// Gets or sets the document with the provided id.
        /// </summary>
        /// <param name="id">The id of the document</param>
        public T this[Guid key]
        {
            get
            {
                return this.Find(key);
            }
            set
            {
                this.AddOrUpdate(key, value);
            }
        }

        /// <summary>
        /// The name of the collection.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The document format that data is saved in.
        /// </summary>
        public StorageFormat Format { get; private set; }

        public DocumentCollection(IDbProvider provider, StorageFormat format)
        {
            this.Name = typeof(T).Name;
            this.provider = provider;
            this.Format = format;
        }

        public DocumentCollection(
            string name, 
            IDbProvider provider, 
            StorageFormat format)
        {
            this.Name = name;
            this.provider = provider;
            this.Format = format;
        }

        /// <summary>
        /// Gets the document with the provided id
        /// </summary>
        /// <param name="id">The id of the document</param>
        public T Find(Guid id)
        {
            if (this.Format == StorageFormat.BSON)
            {
                var record = provider.GetBsonRecord(id, this.Name);
                if (record != null)
                {
                    return Serializer.DeserializeBson<T>(record.Data);
                }
                return default(T);
            }
            else
            {
                var record = provider.GetJsonRecord(id, this.Name);
                if (record != null)
                {
                    return Serializer.DeserializeJson<T>(record.Data);
                }
                return default(T);
            }
        }

        /// <summary>
        /// Iterates over the collection and returns the first document that
        /// passes the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        public T Find(Func<T, bool> filter)
        {
            var keyValuePair = this.FindWithId(filter);
            if (keyValuePair.Key != Guid.Empty)
                return keyValuePair.Value;
            else
                return default(T);
        }

        /// <summary>
        /// Iterates over the collection and returns the first document and its
        /// id that passes the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        public KeyValuePair<Guid, T> FindWithId(Func<T, bool> filter)
        {
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider
                    .EnumerateBsonCollection(
                        this.Name))
                {
                    var document = Serializer.DeserializeBson<dynamic>(
                        record.Data);
                    if (filter != null)
                    {
                        if (filter(document))
                            return new KeyValuePair<Guid, T>(
                                record.Id,
                                document);
                    }
                    else
                    {
                        return new KeyValuePair<Guid, T>(record.Id, document);
                    }
                }
                return new KeyValuePair<Guid,T>(Guid.Empty, default(T));
            }
            else
            {
                foreach (var record in provider
                    .EnumerateJsonCollection(
                        this.Name))
                {
                    var document = Serializer.DeserializeJson<dynamic>(
                        record.Data);
                    if (filter != null)
                    {
                        if (filter(document))
                            return new KeyValuePair<Guid, T>(
                                record.Id, 
                                document);
                    }
                    else
                    {
                        return new KeyValuePair<Guid, T>(record.Id, document);
                    }
                }
                return new KeyValuePair<Guid, T>(Guid.Empty, default(T));
            }
        }

        /// <summary>
        /// Iterates over the collection and returns all documents that pass the
        /// filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        public ICollection<T> Filter(Func<T, bool> filter)
        {
            var result = new Collection<T>();
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider
                    .EnumerateBsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeBson<T>(record.Data);
                    if (filter != null)
                    {
                        if (filter(document))
                            result.Add(document);
                    }
                    else
                    {
                        result.Add(document);
                    }
                }
            }
            else
            {
                foreach (var record in provider
                    .EnumerateJsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeJson<T>(record.Data);
                    if (filter != null)
                    {
                        if (filter(document))
                            result.Add(document);
                    }
                    else
                    {
                        result.Add(document);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Iterates over the collection and returns all documents and their ids
        /// that pass the filter.
        /// </summary>
        /// <param name="filter">The filter action</param>
        public ICollection<KeyValuePair<Guid, T>> FilterWithIds(
            Func<T, bool> filter)
        {
            var result = new Collection<KeyValuePair<Guid, T>>();
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider
                    .EnumerateBsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeBson<T>(record.Data);
                    if (filter != null)
                    {
                        if (filter(document))
                            result.Add(new KeyValuePair<Guid, T>(
                                record.Id, 
                                document));
                    }
                    else
                    {
                        result.Add(
                            new KeyValuePair<Guid, T>(
                                record.Id, 
                                document));
                    }
                }
            }
            else
            {
                foreach (var record in provider
                    .EnumerateJsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeJson<T>(record.Data);
                    if (filter != null)
                    {
                        if (filter(document))
                            result.Add(
                                new KeyValuePair<Guid, T>(
                                    record.Id, 
                                    document));
                    }
                    else
                    {
                        result.Add(
                            new KeyValuePair<Guid, T>(
                                record.Id, 
                                document));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Adds or updates a document in the collection
        /// </summary>
        /// <param name="id">The id of the document</param>
        /// <param name="item">
        /// The object that is being stored in the collection
        /// </param>
        public void AddOrUpdate(Guid id, T item)
        {
            if (this.Format == StorageFormat.BSON)
            {
                var record = new BsonRecord 
                { 
                    Id = id, 
                    Data = Serializer.SerializeBson<T>(item) 
                };
                provider.AddOrUpdateRecord(record, this.Name);
            }
            else
            {
                var record = new JsonRecord 
                { 
                    Id = id, 
                    Data = Serializer.SerializeJson<T>(item) 
                };
                provider.AddOrUpdateRecord(record, this.Name);
            }
        }

        /// <summary>
        /// Removes the document with the specified id from the collection
        /// </summary>
        /// <param name="id">The id of the document</param>
        public void Remove(Guid id)
        {
            provider.RemoveRecord(id, this.Name);
        }

        /// <summary>
        /// Gets the enumerator for this collection
        /// </summary>
        public IEnumerator<KeyValuePair<Guid, T>> GetEnumerator()
        {
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider
                    .EnumerateBsonCollection(this.Name))
                {
                    yield return new KeyValuePair<Guid, T>(
                        record.Id, 
                        Serializer.DeserializeBson<T>(record.Data));
                }
                yield break;
            }
            else
            {
                foreach (var record in provider
                    .EnumerateJsonCollection(this.Name))
                {
                    yield return new KeyValuePair<Guid, T>(
                        record.Id,
                        Serializer.DeserializeJson<T>(record.Data));
                }
                yield break;
            }
        }

        /// <summary>
        /// Gets the enumerator for this collection
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator() as IEnumerator;
        }
    }
}
