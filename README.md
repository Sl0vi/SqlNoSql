SqlNoSql
================================================================================

**Use your SQL database as a NoSql JSON or BSON document store**

A data access layer that uses SQL databases as NoSQL JSON or BSON document
stores.

It works by serializing objects as either JSON or BSON to simple two column
tables. One column for the id and one for the serialized data.

Each of these tables is called a collection and can be queried like any normal
.Net collection. Collections also expose an indexer that allows you to retrieve
and set documents the same way as with a dictionary.

All queries are done by loading each record into memory one at a time and then
applying the filter. Therefore, the closer your data access layer is to the
database the better. Preferably you should run your data access code on the same
machine as the SQL Server.

**Currently supported database providers:**

- System.Data.SqlClient
- System.Data.SQLite
- Mono.Data.Sqlite

*Currently only GUIDs are supported as keys*

How to use
--------------------------------------------------------------------------------

Create a new instance of the DocumentStore class and either pass in a connection
string and provider name, or pass in the name of the connection string in your
configuration file.

**Example connection string in App.config:**

    <connectionStrings>
      <add name="SqlNoSqlDemo" 
        connectionString="Data Source=localhost;
                          Initial Catalog=SqlNoSqlConsoleDemo;
                          Integrated Security=True" 
        providerName="System.Data.SqlClient" />
    </connectionStrings>

**Instantiate the document store:**

    var documentStore = new DocumentStore("SqlNoSqlDemo");

### Creating and deleting collections ###

You can use the document store to create and delete collections. The document
store creates a table named _collections that contains some metadata about each
collection you create and each collection is represented as a table in the
database. Since the table name "_collections" is used for internal metadata you
cannot create a collection with that name.

    var documentStore = new DocumentStore("SqlNoSqlDemo");
    
    // Create a collection named 'Product'
    var productsCollection = documentStore.CreateCollection<Product>();
    
    // Delete the collection
    documentStore.DeleteCollection<Product>();

While the example above returns a collection object for the class Product, all
collections are actually schemaless and can store any kind of object. Using a
strongly typed collection is just a bit easier to work with since it gives us 
intellisense in queries.

It is also possible to create dynamic collections by not specifying any type and
just passing in a name.

    // Create a dynamic collection
    documentStore.CreateCollection("dynamicCollection");

You can also specify if you want objects to be serialized as either JSON or BSON
when creating a collection. *The default is JSON*

    // Create a collection that serializes objects to BSON
    documentStore.CreateCollection<Product>(StorageFormat.BSON);

### Working with collections ###

The collection tables consist of two columns, an Id column that contains a
unique identifier for each record and a data column that contains the record
serialized as either BSON or JSON.

You can use the AddOrUpdate method to add an object or update an existing object
in the collection.

    // Add a product
    var product = new Product 
    { 
        Id = Guid.NewGuid(), 
        Name = "Hammer", 
        Price = 10M 
    };
    documentStore.Collection<Product>().AddOrUpdate(product.Id, product);

You can use the Find method to retrieve a document by its Id. This is the
fastest way to get a document since it uses the primary key column on the table.

    // Find a product by id
    var product = documentStore
        .Collection<Product>()
        .Find(new Guid("0283793a-b45a-4896-a5b2-f4efe5ec163c"));

You can also use a filter action to find your object. The find method returns
the first object that passes the filter and stops enumerating.

    // Find the product named 'Hammer'
    var product = documentStore
        .Collection<Product>()
        .Find(x => x.Name == "Hammer");

The filter method also allows you to apply an action filter, however it returns
all objects that pass the filter.

    // Find all products where price is bigger than 5
    var products = documentStore
        .Collection<Product>()
        .Filter(x => x.Price > 5M);

Collections also expose an enumerator. This means that you can use collections
in the same way you can use any IEnumerable. For example in a foreach statement
or with Linq.

The collection keeps a connection open to the database during the enumeration,
since it only loads one record at a time. Linq and foreach statements will
properly dispose the enumerator, however if you access the enumerator directly
you must make sure to call dispose yourself or the connection to the database
won't be properly closed.

    // Enumerate over a collection with foreach
    foreach (var product in documentStore.Collection<Product>())
    {
        Console.WriteLine(
            "Name: {0}, Price: {0:c}", 
            product.Name, 
            product.Price);
    }
    
    // Use Linq to query a collection
    var products = documentStore
        .Collection<Product>()
        .Where(x => x.Price > 5M);

You can use the indexer to retrieve an object by its id and to add or update an
object. The indexer uses the find method internally, so it should have similar
performance.

    // Retrieve the a product from the collection
    var id = new Guid("0283793a-b45a-4896-a5b2-f4efe5ec163c");
    var product = documentStore
        .Collection<Product>()[id];
    
    // Update the product
    documentStore.Collection<Product>()[product.Id] = product;
