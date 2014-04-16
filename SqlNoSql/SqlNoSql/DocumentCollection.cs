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
    using SqlNoSql.Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class DocumentCollection<T> : IDocumentCollection<T>
    {
        private IDbProvider provider;

        public string Name { get; private set; }

        public StorageFormat Format { get; private set; }

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

        public DocumentCollection(IDbProvider provider, StorageFormat format)
        {
            this.Name = typeof(T).Name;
            this.provider = provider;
            this.Format = format;
        }

        public DocumentCollection(string name, IDbProvider provider, StorageFormat format)
        {
            this.Name = name;
            this.provider = provider;
            this.Format = format;
        }

        public T Find(Guid id)
        {
            if (this.Format == StorageFormat.BSON)
            {
                var record = provider.GetBsonRecord(id, this.Name);
                if (record != null)
                {
                    return Serializer.DeserializeBson<T>(record.BsonData);
                }
                return default(T);
            }
            else
            {
                var record = provider.GetJsonRecord(id, this.Name);
                if (record != null)
                {
                    return Serializer.DeserializeJson<T>(record.JsonData);
                }
                return default(T);
            }
        }

        public T Find(Func<T, bool> filter)
        {
            var keyValuePair = this.FindWithId(filter);
            if (keyValuePair.Key != Guid.Empty)
                return keyValuePair.Value;
            else
                return default(T);
        }

        public KeyValuePair<Guid, T> FindWithId(Func<T, bool> filter)
        {
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider.EnumerateBsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeBson<dynamic>(record.BsonData);
                    if (filter != null)
                    {
                        if (filter(document))
                            return new KeyValuePair<Guid, T>(record.Id, document);
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
                foreach (var record in provider.EnumerateJsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeJson<dynamic>(record.JsonData);
                    if (filter != null)
                    {
                        if (filter(document))
                            return new KeyValuePair<Guid, T>(record.Id, document);
                    }
                    else
                    {
                        return new KeyValuePair<Guid, T>(record.Id, document);
                    }
                }
                return new KeyValuePair<Guid, T>(Guid.Empty, default(T));
            }
        }

        public ICollection<T> Filter(Func<T, bool> filter)
        {
            var result = new Collection<T>();
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider.EnumerateBsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeBson<T>(record.BsonData);
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
                foreach (var record in provider.EnumerateJsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeJson<T>(record.JsonData);
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

        public ICollection<KeyValuePair<Guid, T>> FilterWithIds(Func<T, bool> filter)
        {
            var result = new Collection<KeyValuePair<Guid, T>>();
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider.EnumerateBsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeBson<T>(record.BsonData);
                    if (filter != null)
                    {
                        if (filter(document))
                            result.Add(new KeyValuePair<Guid, T>(record.Id, document));
                    }
                    else
                    {
                        result.Add(new KeyValuePair<Guid, T>(record.Id, document));
                    }
                }
            }
            else
            {
                foreach (var record in provider.EnumerateJsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeJson<T>(record.JsonData);
                    if (filter != null)
                    {
                        if (filter(document))
                            result.Add(new KeyValuePair<Guid, T>(record.Id, document));
                    }
                    else
                    {
                        result.Add(new KeyValuePair<Guid, T>(record.Id, document));
                    }
                }
            }
            return result;
        }

        public void AddOrUpdate(Guid id, T item)
        {
            if (this.Format == StorageFormat.BSON)
            {
                var record = new BsonRecord { Id = id, BsonData = Serializer.SerializeBson<T>(item) };
                provider.AddOrUpdateRecord(record, this.Name);
            }
            else
            {
                var record = new JsonRecord { Id = id, JsonData = Serializer.SerializeJson<T>(item) };
                provider.AddOrUpdateRecord(record, this.Name);
            }
        }

        public void Remove(Guid id)
        {
            provider.RemoveRecord(id, this.Name);
        }

        public IEnumerator<KeyValuePair<Guid, T>> GetEnumerator()
        {
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider.EnumerateBsonCollection(this.Name))
                {
                    yield return new KeyValuePair<Guid, T>(record.Id, Serializer.DeserializeBson<T>(record.BsonData));
                }
                yield break;
            }
            else
            {
                foreach (var record in provider.EnumerateJsonCollection(this.Name))
                {
                    yield return new KeyValuePair<Guid, T>(record.Id, Serializer.DeserializeJson<T>(record.JsonData));
                }
                yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator() as IEnumerator;
        }
    }
}
