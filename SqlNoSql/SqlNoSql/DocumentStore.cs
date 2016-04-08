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
    using System.Configuration;
    using Data;

    /// <summary>
    /// The document store represents the database used for storing JSON or BSON
    /// documents.
    /// </summary>
    public class DocumentStore : IDocumentStore
    {
        private IDbProvider provider;
        
        /// <summary>
        /// Settings for the document store
        /// </summary>
        public DocumentStoreSettings Settings { get; set; }

        /// <summary>
        /// Creates a new DocumentStore instance
        /// </summary>
        /// <param name="name">
        /// The name of the connection string in the configuration file
        /// </param>
        public DocumentStore(string name)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name];
            if (connectionString == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "No connection string named {0} was found in the " +
                        "configuration file", 
                        name),
                    name);
            }
            if (string.IsNullOrEmpty(connectionString.ProviderName))
            {
                throw new ConfigurationErrorsException(
                 "providerName must be set on the connection string in order " +
                 "to use this constructor");
            }
            var providerType = DbProviderFactory.GetType(
                connectionString.ProviderName);
            provider = (IDbProvider)Activator.CreateInstance(
                providerType, 
                new[] { connectionString.ConnectionString });
            this.Init();
        }

        /// <summary>
        /// Creates a new DocumentStore instance
        /// </summary>
        /// <param name="connectionString">
        /// The connection string to the database
        /// </param>
        /// <param name="providerName">The name of the db provider</param>
        public DocumentStore(string connectionString, string providerName)
        {
            this.Init();
            var providerType = DbProviderFactory.GetType(providerName);
            provider = (IDbProvider)Activator.CreateInstance(
                providerType, 
                new[] { connectionString });
        }

        /// <summary>
        /// Creates a new DocumentStore instance
        /// </summary>
        /// <param name="dbProvider"> 
        /// An instance of the DbProvider you want to use with the document
        /// store
        /// </param>
        public DocumentStore(IDbProvider dbProvider)
        {
            this.Init();
            provider = dbProvider;
        }

        private void Init()
        {
            this.Settings = new DocumentStoreSettings();
        }

        /// <summary>
        /// Checks if a collection exists in the document store.
        /// </summary>
        /// <param name="name">The name of the collection to check for</param>
        public bool CollectionExists(string name)
        {
            return provider.CollectionExists(name);
        }

        /// <summary>
        /// Checks if a collection with the same name as the passed in type
        /// exists in the document store.
        /// </summary>
        /// <typeparam name="T">The type to check for</typeparam>
        public bool CollectionExists<T>()
        {
            return provider.CollectionExists(typeof(T).Name);
        }

        /// <summary>
        /// Gets information about all collections in the document store.
        /// </summary>
        public IEnumerable<CollectionInfo> CollectionInfos()
        {
            return provider.CollectionInfos();
        }

        /// <summary>
        /// Gets a dynamic document collection by its name.
        /// </summary>
        /// <param name="name">The name of the collection</param>
        public IDocumentCollection<dynamic> Collection(string name)
        {
            return provider.GetCollection<dynamic>(name);
        }

        /// <summary>
        /// Gets a document collection for the specified type. The name of the
        /// collection is assumed to be the same as the name of the type.
        /// </summary>
        /// <typeparam name="T">
        /// The type that is stored in the collection
        /// </typeparam>
        public IDocumentCollection<T> Collection<T>()
        {
            return provider.GetCollection<T>();
        }

        /// <summary>
        /// Gets a document collection for the specified type with the specified
        /// name.
        /// </summary>
        /// <typeparam name="T">The type stored in the collection</typeparam>
        /// <param name="name">The name of the collection</param>
        public IDocumentCollection<T> Collection<T>(string name)
        {
            return provider.GetCollection<T>(name);
        }

        /// <summary>
        /// Creates a new dynamic collection and returns an instance of it.
        /// </summary>
        /// <param name="name">The name of the collection to create</param>
        public IDocumentCollection<dynamic> CreateCollection(string name)
        {
            if (!provider.CreateCollection(
                name, 
                this.Settings.DefaultStorageFormat))
                throw new Exception(
                    string.Format(
                        "Collection '{0}' could not be created",
                        name));
            return provider.GetCollection<dynamic>(name);
        }

        /// <summary>
        /// Creates a new dynamic collection and returns an instance of it.
        /// </summary>
        /// <param name="name">The name of the collection to create</param>
        /// <param name="format">
        /// The document format that is used for storing objects in the database
        /// </param>
        public IDocumentCollection<dynamic> CreateCollection(
            string name,
            StorageFormat format)
        {
            if (!provider.CreateCollection(name, format))
                throw new Exception(
                    string.Format(
                        "Collection '{0}' could not be created",
                        name));
            return provider.GetCollection<dynamic>(name);
        }

        /// <summary>
        /// Creates a new collection for the specified type. The name of the
        /// collection that is created is the same as the name of the type.
        /// </summary>
        /// <typeparam name="T">
        /// The type that is stored in the collection
        /// </typeparam>
        public IDocumentCollection<T> CreateCollection<T>()
        {
            if (!provider.CreateCollection<T>(
                this.Settings.DefaultStorageFormat))
                throw new Exception(
                    string.Format("Collection '{0}' could not be created",
                    typeof(T).Name));
            return provider.GetCollection<T>();
        }

        /// <summary>
        /// Creates a new collection for the specified type. The name of the
        /// collection that is created is the same as the name of the type.
        /// </summary>
        /// <typeparam name="T">
        /// The type that is stored in the collection
        /// </typeparam>
        /// <param name="format">
        /// The document format that is used for storing objects in the database
        /// </param>
        public IDocumentCollection<T> CreateCollection<T>(StorageFormat format)
        {
            if (!provider.CreateCollection<T>(format))
                throw new Exception(
                    string.Format("Collection '{0}' could not be created",
                    typeof(T).Name));
            return provider.GetCollection<T>();
        }

        /// <summary>
        /// Creates a new collection for the specified type with the specified
        /// name and returns an instance of it.
        /// </summary>
        /// <typeparam name="T">
        /// The type that is stored in the collection
        /// </typeparam>
        /// <param name="name">The name of the collection</param>
        public IDocumentCollection<T> CreateCollection<T>(string name)
        {
            if (!provider.CreateCollection(
                name, 
                this.Settings.DefaultStorageFormat))
                throw new Exception(
                    string.Format("Collection '{0}' could not be created",
                    name));
            return provider.GetCollection<T>(name);
        }

        /// <summary>
        /// Creates a new collection for the specified type with the specified
        /// name and returns an instance of it.
        /// </summary>
        /// <typeparam name="T">
        /// The type that is stored in the collection
        /// </typeparam>
        /// <param name="name">The name of the collection</param>
        /// <param name="format">
        /// The document format that is used for storing objects in the database
        /// </param>
        public IDocumentCollection<T> CreateCollection<T>(
            string name,
            StorageFormat format)
        {
            if (!provider.CreateCollection(name, format))
                throw new Exception(
                    string.Format("Collection '{0}' could not be created",
                    name));
            return provider.GetCollection<T>(name);
        }

        /// <summary>
        /// Deletes the collection that has the same name as the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type that is stored in the collection
        /// </typeparam>
        public void DeleteCollection<T>()
        {
            provider.DeleteCollection<T>();
        }

        /// <summary>
        /// Deletes the collection with the specified name.
        /// </summary>
        /// <param name="name">The name of the collection to delete</param>
        public void DeleteCollection(string name)
        {
            provider.DeleteCollection(name);
        }

        /// <summary>
        /// Begins a new transaction
        /// </summary>
        public ITransaction BeginTransaction()
        {
            return provider.BeginTransaction();
        }
    }
}
