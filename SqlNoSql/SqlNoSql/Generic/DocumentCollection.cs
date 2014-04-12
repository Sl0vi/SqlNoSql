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
    using System.Collections;
    using System.Collections.Generic;

    public class DocumentCollection<T> : IDocumentCollection<T>
    {
        private IDbProvider provider;

        public string Name { get; private set; }

        public StorageFormat StorageFormat { get; private set; }

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

        public DocumentCollection(IDbProvider provider)
        {
            this.Name = typeof(T).Name;
        }

        public DocumentCollection(string name, IDbProvider provider)
        {
            this.Name = name;
        }

        public T Find(Guid key)
        {
            using (var connection = provider.GetConnection())
            {
                connection.Open();
                throw new NotImplementedException();
            }
        }

        public T Find(Func<T, bool> filter)
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<Guid, T> FindWithKey(Func<T, bool> filter)
        {
            throw new NotImplementedException();
        }

        public ICollection<T> Filter(Func<T, bool> filter)
        {
            throw new NotImplementedException();
        }

        public ICollection<KeyValuePair<Guid, T>> FilterWithKeys(Func<T, bool> filter)
        {
            throw new NotImplementedException();
        }

        public void AddOrUpdate(Guid key, T item)
        {
            throw new NotImplementedException();
        }

        public void Remove(Guid key)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }


        
    }
}
