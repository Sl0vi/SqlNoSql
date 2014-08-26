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
    /// <summary>
    /// Specifies various settings for the document store.
    /// </summary>
    public class DocumentStoreSettings
    {
        /// <summary>
        /// Specifies if the document store should use an identity map to store loaded objects in memory.
        /// Enabling the cache means that you will always get the same instance of a document and that
        /// getting an already loaded document by key will not hit the database at all. This will increase
        /// memory and CPU usage since the document store has to make extra lookups into the identity map, and 
        /// it might return stale data if the cached object has been updated in the database after it was loaded
        /// into memory, but it means fewer roundtrips to the database.
        /// 
        /// Default value is false.
        /// </summary>
        public bool EnableCache { get; set; }

        /// <summary>
        /// The default document format that should be used for storing objects in the database.
        /// This only affects newly created collections and can be explicitly set when creating collections.
        /// 
        /// Default value is JSON.
        /// </summary>
        public StorageFormat DefaultStorageFormat { get; set; }

        public DocumentStoreSettings()
        {
            this.EnableCache = false;
            this.DefaultStorageFormat = StorageFormat.JSON;
        }
    }
}
