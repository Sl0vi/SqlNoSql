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
    using System.Data;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;
    using System.IO;
    using SqlNoSql.Data;
    using System.Collections.ObjectModel;

    public class DocumentCollection : IDocumentCollection
    {
        private IDbProvider provider { get; set; }

        public string Name { get; private set; }

        public StorageFormat Format { get; private set; }

        public dynamic this[Guid key]
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

        public DocumentCollection(string name, IDbProvider provider)
        {
            this.Name = name;
            this.provider = provider;
        }

        public dynamic Find(Guid id)
        {
            if (this.Format == StorageFormat.BSON)
            {
                var record = provider.GetBsonRecord(id, this.Name);
                if (record != null)
                {
                    return Serializer.DeserializeBson<dynamic>(record.BsonData);
                }
                return null;
            }
            else
            {
                var record = provider.GetJsonRecord(id, this.Name);
                if (record != null)
                {
                    return Serializer.DeserializeJson<dynamic>(record.JsonData);
                }
                return null;
            }
        }

        public KeyValuePair<Guid, dynamic>? FindWithKey(Func<dynamic, bool> filter)
        {
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider.EnumerateBsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeBson<dynamic>(record.BsonData);
                    if (filter != null)
                    {
                        if (filter(document))
                            return new KeyValuePair<Guid, dynamic>(record.Id, document);
                    }
                    else
                    {
                        return new KeyValuePair<Guid, dynamic>(record.Id, document);
                    }
                }
                return null;
            }
            else
            {
                foreach (var record in provider.EnumerateJsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeJson<dynamic>(record.JsonData);
                    if (filter != null)
                    {
                        if (filter(document))
                            return new KeyValuePair<Guid, dynamic>(record.Id, document);
                    }
                    else
                    {
                        return new KeyValuePair<Guid, dynamic>(record.Id, document);
                    }
                }
                return null;
            }
        }

        public dynamic Find(Func<dynamic, bool> filter)
        {
            var keyValuePair = this.FindWithKey(filter);
            if (keyValuePair.HasValue)
                return keyValuePair.Value.Value;
            else
                return null;
        }

        public ICollection<dynamic> Filter(Func<dynamic, bool> filter)
        {
            var result = new Collection<dynamic>();
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider.EnumerateBsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeBson<dynamic>(record.BsonData);
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
                    var document = Serializer.DeserializeJson<dynamic>(record.JsonData);
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

        public ICollection<KeyValuePair<Guid, dynamic>> FilterWithKeys(Func<dynamic, bool> filter = null)
        {
            var result = new Collection<KeyValuePair<Guid, dynamic>();
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider.EnumerateBsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeBson<dynamic>(record.BsonData);
                    if (filter != null)
                    {
                        if (filter(document))
                            result.Add(new KeyValuePair<Guid, dynamic>(record.Id, document));
                    }
                    else
                    {
                        result.Add(new KeyValuePair<Guid, dynamic>(record.Id, document));
                    }
                }
            }
            else
            {
                foreach (var record in provider.EnumerateJsonCollection(this.Name))
                {
                    var document = Serializer.DeserializeJson<dynamic>(record.JsonData);
                    if (filter != null)
                    {
                        if (filter(document))
                            result.Add(new KeyValuePair<Guid, dynamic>(record.Id, document));
                    }
                    else
                    {
                        result.Add(document);
                    }
                }
            }
            return result;
        }

        public void AddOrUpdate(Guid id, dynamic item)
        {
            if (this.Format == StorageFormat.BSON)
            {
                var record = new BsonRecord { Id = id, BsonData = Serializer.SerializeBson<dynamic>(item) };
                provider.AddOrUpdateRecord(record, this.Name);
            }
            else
            {
                var record = new JsonRecord { Id = id, JsonData = Serializer.SerializeJson<dynamic>(item) };
                provider.AddOrUpdateRecord(record, this.Name);
            }
        }

        public void Remove(Guid key)
        {
            provider.RemoveRecord(id, this.Name);
        }

        public IEnumerator GetEnumerator()
        {
            if (this.Format == StorageFormat.BSON)
            {
                foreach (var record in provider.EnumerateBsonCollection(this.Name))
                {
                    yield return new KeyValuePair<Guid, dynamic>(record.Id, Serializer.DeserializeBson<dynamic>(record.BsonData));
                }
                yield break;
            }
            else
            {
                foreach (var record in provider.EnumerateJsonCollection(this.Name))
                {
                    yield return new KeyValuePair<Guid, dynamic>(record.Id, Serializer.DeserializeJson<dynamic>(record.JsonData));
                }
                yield break;
            }
        }
    }
}
