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
    using System.Linq;
    using System.Reflection;
    using SqlClient;

    /// <summary>
    /// The db provider factory stores information about db providers needed to
    /// load and instantiate them.
    /// </summary>
    public static class DbProviderFactory
    {
        /// <summary>
        /// This dictionary stores all the DbProviders registered with the
        /// factory
        /// </summary>
        private static Dictionary<string, DbProviderInfo> providers = 
            new Dictionary<string, DbProviderInfo>();

        /// <summary>
        /// The contructor registers all the built in providers
        /// </summary>
        static DbProviderFactory()
        {
            providers.Add(
                "System.Data.SqlClient",
                new DbProviderInfo
                {
                    Type = typeof(SqlClientProvider)
                });
            providers.Add(
                "System.Data.SQLite",
                new DbProviderInfo
                {
                    ClassName = "SqlNoSql.Sqlite.Net.NetSqliteProvider",
                    AssemblyName = "SqlNoSql.Sqlite.Net"
                });
            providers.Add(
                "Mono.Data.Sqlite",
                new DbProviderInfo
                {
                    ClassName = "SqlNoSql.Sqlite.Mono.MonoSqliteProvider",
                    AssemblyName = "SqlNoSql.Sqlite.Mono"
                });
        }

        /// <summary>
        /// Registers a DbProvider with the factory. The full name of the class
        /// will be used as the provider name.
        /// </summary>
        /// <typeparam name="T">The DbProvider type</typeparam>
        public static void Register<T>()
            where T : IDbProvider
        {
            providers.Add(
                typeof(T).FullName,
                new DbProviderInfo
                {
                    Type = typeof(T)
                });
        }

        /// <summary>
        /// Registers a DbProvider with the factory.
        /// </summary>
        /// <param name="providerName">
        /// The name of the provider used during construction or in 
        /// configuration file connection strings.
        /// </param>
        /// <typeparam name="T">The DbProvider type</typeparam>
        public static void Register<T>(string providerName)
            where T : IDbProvider
        {
            providers.Add(
                providerName,
                new DbProviderInfo
                {
                    Type = typeof(T)
                });
        }

        /// <summary>
        /// Registers a DbProvider with the factory. The full class name will be
        /// used as the provider name.
        /// </summary>
        /// <param name="className">
        /// The full name of the class that implements the provider.
        /// </param>
        /// <param name="assemblyName">
        /// The name of the assembly that contains the provider class.
        /// </param>
        public static void Register(string className, string assemblyName)
        {
            providers.Add(
                className,
                new DbProviderInfo
                {
                    ClassName = className,
                    AssemblyName = assemblyName
                });
        }

        /// <summary>
        /// Registers a DbProvider with the factory.
        /// </summary>
        /// <param name="providerName"> 
        /// The provider name used during construction and in configuration file
        /// connection strings.
        /// </param>
        /// <param name="className"> 
        /// The full name of the class that implements the provider.
        /// </param>
        /// <param name="assemblyName"> 
        /// The name of the assembly that contains the provider class.
        /// </param>
        public static void Register(
            string providerName,
            string className, 
            string assemblyName)
        {
            providers.Add(
                providerName,
                new DbProviderInfo
                {
                    ClassName = className,
                    AssemblyName = assemblyName
                });
        }

        /// <summary>
        /// Returns the type of the provider with the specified provider name.
        /// </summary>
        public static Type GetType(string name)
        {
            DbProviderInfo providerInfo;
            if (providers.TryGetValue(name, out providerInfo))
            {
                if (providerInfo.Type != null)
                    return providerInfo.Type;
                try
                {
                    var assembly = Assembly.Load(providerInfo.AssemblyName);
                    var type = assembly
                        .GetTypes()
                        .SingleOrDefault(x => 
                            x.FullName == providerInfo.ClassName);
                    if (type == null)
                        throw new TypeLoadException(
                            string.Format(
                                "The class '{0}' was not found in assembly " +
                                "'{1}'",
                                providerInfo.ClassName,
                                providerInfo.AssemblyName));
                    return providerInfo.Type = type;
                }
                catch (Exception error)
                {
                    throw new TypeLoadException(
                        string.Format(
                            "The provider for '{0}' could not be loaded",
                            name),
                        error);
                }
            }
            throw new ConfigurationErrorsException(
                string.Format(
                    "{0} is not registered with the provider factory, " +
                    "make sure you register the provider before " +
                    "initializing the document store",
                    name));
        }
    }
}
