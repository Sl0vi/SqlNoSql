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
    using System.Reflection;
    using System.Configuration;
    using SqlNoSql.Generic;

    public class DocumentStore : IDocumentStore
    {
        private IDbProvider provider;

        public DocumentStoreSettings Settings { get; set; }

        public DocumentStore()
        {
        }

        public DocumentStore(string name)
        {
        }

        public DocumentStore(string connectionString, string providerName)
        {
        }

        private void Init()
        {
            this.Settings = new DocumentStoreSettings();
        }


        public IDocumentCollection Collection(string name)
        {
            throw new NotImplementedException();
        }

        public Generic.IDocumentCollection<T> Collection<T>()
        {
            throw new NotImplementedException();
        }

        public Generic.IDocumentCollection<T> Collection<T>(string name)
        {
            throw new NotImplementedException();
        }

        public IDocumentCollection CreateCollection(string name)
        {
            throw new NotImplementedException();
        }

        public Generic.IDocumentCollection<T> CreateCollection<T>()
        {
            throw new NotImplementedException();
        }

        public Generic.IDocumentCollection<T> CreateCollection<T>(string name)
        {
            throw new NotImplementedException();
        }

        public void DeleteCollection<T>()
        {
            throw new NotImplementedException();
        }

        public void DeleteCollection(string name)
        {
            throw new NotImplementedException();
        }

        

        private void LoadSettings()
        {
        }

        private void GetProvider()
        {
        }

        public IDocumentCollection CreateCollection(string name, StorageFormat format)
        {
            throw new NotImplementedException();
        }

        public Generic.IDocumentCollection<T> CreateCollection<T>(StorageFormat format)
        {
            throw new NotImplementedException();
        }

        public IDocumentCollection<T> CreateCollection<T>(string name, StorageFormat format)
        {
            throw new NotImplementedException();
        }
    }
}
