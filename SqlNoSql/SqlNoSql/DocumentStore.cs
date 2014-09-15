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
    using System.Configuration;
    using System.Reflection;
    using SqlNoSql.Data;

    public class DocumentStore : IDocumentStore
    {
        private IDbProvider provider;

        public DocumentStoreSettings Settings { get; set; }

        /// <summary>
        /// Creates a new DocumentStore instance
        /// </summary>
        /// <param name="name">The name of the connection string in the configuration file</param>
        public DocumentStore(string name)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name];
            if (connectionString == null)
            {
                throw new ArgumentException(
                    string.Format("No connection string named {0} was found in the configuration file", name),
                    name);
            }
            if (string.IsNullOrEmpty(connectionString.ProviderName))
            {
                throw new ConfigurationErrorsException("providerName must be set on the connection string in order to use this constructor");
            }
            var providerType = DbProviderFactory.GetType(connectionString.ProviderName);
            if (providerType == null)
            {
                throw new ConfigurationErrorsException(string.Format("{0} is not a supported provider", connectionString.ProviderName));
            }
            provider = (IDbProvider)Activator.CreateInstance(providerType, new[] { connectionString.ConnectionString });
            this.Init();
        }

        /// <summary>
        /// Creates a new DocumentStore instance
        /// </summary>
        /// <param name="connectionString">The connection string to the database</param>
        /// <param name="providerName">The name of the db provider</param>
        public DocumentStore(string connectionString, string providerName)
        {
            this.Init();
            var providerType = DbProviderFactory.GetType(providerName);
            provider = (IDbProvider)Activator.CreateInstance(providerType, new[] { connectionString });
        }

        private void Init()
        {
            this.Settings = new DocumentStoreSettings();
        }

        public bool CollectionExists(string name)
        {
            return provider.CollectionExists(name);
        }

        public bool CollectionExists<T>()
        {
            return provider.CollectionExists(typeof(T).Name);
        }

        public IEnumerable<CollectionInfo> CollectionInfos()
        {
            return provider.CollectionInfos();
        }

        public IDocumentCollection<dynamic> Collection(string name)
        {
            return provider.GetCollection<dynamic>(name);
        }

        public IDocumentCollection<T> Collection<T>()
        {
            return provider.GetCollection<T>();
        }

        public IDocumentCollection<T> Collection<T>(string name)
        {
            return provider.GetCollection<T>(name);
        }

        public IDocumentCollection<dynamic> CreateCollection(string name)
        {
            if (!provider.CreateCollection(name, this.Settings.DefaultStorageFormat))
                throw new Exception(string.Format("Collection '{0}' could not be created", name));
            return provider.GetCollection<dynamic>(name);
        }

        public IDocumentCollection<dynamic> CreateCollection(string name, StorageFormat format)
        {
            if (!provider.CreateCollection(name, format))
                throw new Exception(string.Format("Collection '{0}' could not be created", name));
            return provider.GetCollection<dynamic>(name);
        }

        public IDocumentCollection<T> CreateCollection<T>()
        {
            if (!provider.CreateCollection<T>(this.Settings.DefaultStorageFormat))
                throw new Exception(string.Format("Collection '{0}' could not be created", typeof(T).Name));
            return provider.GetCollection<T>();
        }

        public IDocumentCollection<T> CreateCollection<T>(StorageFormat format)
        {
            if (!provider.CreateCollection<T>(format))
                throw new Exception(string.Format("Collection '{0}' could not be created", typeof(T).Name));
            return provider.GetCollection<T>();
        }

        public IDocumentCollection<T> CreateCollection<T>(string name)
        {
            if (!provider.CreateCollection(name, this.Settings.DefaultStorageFormat))
                throw new Exception(string.Format("Collection '{0}' could not be created", name));
            return provider.GetCollection<T>(name);
        }

        public IDocumentCollection<T> CreateCollection<T>(string name, StorageFormat format)
        {
            if (!provider.CreateCollection(name, format))
                throw new Exception(string.Format("Collection '{0}' could not be created", name));
            return provider.GetCollection<T>(name);
        }

        public void DeleteCollection<T>()
        {
            provider.DeleteCollection<T>();
        }

        public void DeleteCollection(string name)
        {
            provider.DeleteCollection(name);
        }

        public ITransaction BeginTransaction()
        {
            return provider.BeginTransaction();
        }
    }
}
