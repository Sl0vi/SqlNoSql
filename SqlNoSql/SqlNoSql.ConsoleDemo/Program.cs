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

/*
 * SqlNoSql Console Demo
 * =====================
 * 
 * This application is a small demo
 * that reads and writes some data
 * from a Miscrosoft SQL Server database
 * 
 */

namespace SqlNoSql.ConsoleDemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SqlNoSql;
    using SqlNoSql.ConsoleDemo.Entities;

    class Program
    {
        const string characters = "abcdefghijklmnopqrstuvwxyz";

        static void Main(string[] args)
        {
            var documentStore = new DocumentStore("SqlNoSqlDemo");
            Console.WriteLine("Setting up the database...");

            /*
             * Create collections to hold our entities
             */
            var begin = DateTime.Now;

            if (!documentStore.CollectionExists<Product>())
                documentStore.CreateCollection<Product>();
            if (!documentStore.CollectionExists<Customer>())
                documentStore.CreateCollection<Customer>();
            if (!documentStore.CollectionExists<Vendor>())
                documentStore.CreateCollection<Vendor>();
            if (!documentStore.CollectionExists<Order>())
                documentStore.CreateCollection<Order>();

            var end = DateTime.Now;

            Console.WriteLine("Created 4 collections...\nElapsed time: {0:c}", (end - begin));
            Console.WriteLine("Inserting data...");

            var vendors = new[] {
                new Vendor { Id = Guid.NewGuid(), Name = "ACME Inc." },
                new Vendor { Id = Guid.NewGuid(), Name = "NorthWind" }
            };

            begin = DateTime.Now;

            for (var i = 0; i < vendors.Length; i++)
                documentStore.Collection<Vendor>().AddOrUpdate(vendors[i].Id, vendors[i]);

            end = DateTime.Now;

            Console.WriteLine("Inserted {0} records into {1}\nElapsed time: {2:c}", vendors.Length, typeof(Vendor).Name, (end - begin));

            var customers = new[] {
                new Customer { Id = Guid.NewGuid(), Name = "John Doe" },
                new Customer { Id = Guid.NewGuid(), Name = "John Smith" },
                new Customer { Id = Guid.NewGuid(), Name = "Elfriede Pulnik" },
                new Customer { Id = Guid.NewGuid(), Name = "Ivan Albrechtsson" },
                new Customer { Id = Guid.NewGuid(), Name = "Regina Devine" },
                new Customer { Id = Guid.NewGuid(), Name = "Timotheos Norup" }
            };

            begin = DateTime.Now;

            for (var i = 0; i < customers.Length; i++)
                documentStore.Collection<Customer>().AddOrUpdate(customers[i].Id, customers[i]);

            end = DateTime.Now;

            Console.WriteLine("Inserted {0} records into {1}\nElapsed time: {2:c}", customers.Length, typeof(Customer).Name, (end - begin));

            var products = new[] {
                new Product { Id = Guid.NewGuid(), Name = "Hammer", Price = 15M, VendorId = vendors.First(x => x.Name == "ACME Inc.").Id },
                new Product { Id = Guid.NewGuid(), Name = "Saw", Price = 15M, VendorId = vendors.First(x => x.Name == "ACME Inc.").Id },
                new Product { Id = Guid.NewGuid(), Name = "Dynamite", Price = 15M, VendorId = vendors.First(x => x.Name == "ACME Inc.").Id },
                new Product { Id = Guid.NewGuid(), Name = "Ladder", Price = 15M, VendorId = vendors.First(x => x.Name == "ACME Inc.").Id },
                new Product { Id = Guid.NewGuid(), Name = "Bowling ball", Price = 15M, VendorId = vendors.First(x => x.Name == "ACME Inc.").Id },
                new Product { Id = Guid.NewGuid(), Name = "Wrecking ball", Price = 15M, VendorId = vendors.First(x => x.Name == "ACME Inc.").Id },
                new Product { Id = Guid.NewGuid(), Name = "Paint", Price = 15M, VendorId = vendors.First(x => x.Name == "ACME Inc.").Id },
                new Product { Id = Guid.NewGuid(), Name = "Rocket", Price = 15M, VendorId = vendors.First(x => x.Name == "NorthWind").Id },
                new Product { Id = Guid.NewGuid(), Name = "Bowl", Price = 15M, VendorId = vendors.First(x => x.Name == "NorthWind").Id },
                new Product { Id = Guid.NewGuid(), Name = "Nail", Price = 15M, VendorId = vendors.First(x => x.Name == "NorthWind").Id },
                new Product { Id = Guid.NewGuid(), Name = "Spork", Price = 15M, VendorId = vendors.First(x => x.Name == "NorthWind").Id },
            };

            Console.WriteLine("Inserted {0} records into {1}\nElapsed time: {2:c}", products.Length, typeof(Product).Name, (end - begin));

            begin = DateTime.Now;

            for (var i = 0; i < products.Length; i++)
                documentStore.Collection<Product>().AddOrUpdate(products[i].Id, products[i]);

            end = DateTime.Now;

            Console.WriteLine("Get all vendors...");

            begin = DateTime.Now;

            var productVendors = documentStore.Collection<Vendor>().ToList();

            end = DateTime.Now;

            Console.WriteLine("Got {0} vendors\nElapsed time: {1:c}", productVendors.Count, (end - begin));

            Console.WriteLine("Creating 10.000 random products with random vendor");
            
            var random = new Random();

            begin = DateTime.Now;

            for (var i = 0; i < 10000; i++)
            {
                var product = new Product 
                { 
                    Id = Guid.NewGuid(), 
                    Name = string.Concat(Enumerable.Range(0, 10).Select(x => characters[random.Next(0, characters.Length)])), 
                    Price = Math.Round((decimal)random.NextDouble() * 100, 2),
                    VendorId = productVendors[random.Next(0, productVendors.Count)].Key
                };
                documentStore.Collection<Product>().AddOrUpdate(product.Id, product);
            }

            end = DateTime.Now;

            Console.WriteLine("Inserted 10.000 products\nElapsed time: {0:c}", (end - begin));

            Console.WriteLine("Counting products for ACME inc.");

            begin = DateTime.Now;

            var acmeProductCount = documentStore.Collection<Product>().Where(x => x.Value.VendorId == productVendors.First(z => z.Value.Name == "ACME Inc.").Value.Id).Count();

            end = DateTime.Now;

            Console.WriteLine("ACME Inc. has {0} products\nElapsed time: {1:c}", acmeProductCount, (end - begin));

            Console.WriteLine("Counting products for Northwind");

            begin = DateTime.Now;

            var northWindProductCount = documentStore.Collection<Product>().Where(x => x.Value.VendorId == productVendors.First(z => z.Value.Name == "NorthWind").Value.Id).Count();

            end = DateTime.Now;

            Console.WriteLine("NorthWind has {0} products\nElapsed time: {1:c}", northWindProductCount, (end - begin));
            
            Console.WriteLine("Deleting collections...");

            begin = DateTime.Now;

            if (documentStore.CollectionExists<Product>())
                documentStore.DeleteCollection<Product>();
            if (documentStore.CollectionExists<Customer>())
                documentStore.DeleteCollection<Customer>();
            if (documentStore.CollectionExists<Vendor>())
                documentStore.DeleteCollection<Vendor>();
            if (documentStore.CollectionExists<Order>())
                documentStore.DeleteCollection<Order>();

            end = DateTime.Now;

            Console.WriteLine("Deleted 4 collections\nElapsed time: {0:c}", (end - begin));

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
