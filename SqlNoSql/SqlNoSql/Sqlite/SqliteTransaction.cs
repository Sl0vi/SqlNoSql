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

namespace SqlNoSql.Sqlite
{
    using System.Data;
    
    /// <summary>
    /// A database transaction for SQLite databases
    /// </summary>
    public class SqliteTransaction : ITransaction
    {
        /// <summary>
        /// The provider used in the transaction
        /// </summary>
        public SqliteProvider Provider { get; private set; }
        
        /// <summary>
        /// The database transaction that is wrapped by the SqliteTransaction
        /// </summary>
        public IDbTransaction Transaction { get; private set; }
        
        /// <summary>
        /// The database connection that started the transaction
        /// </summary>
        public IDbConnection Connection { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="T:SqlNoSql.Sqlite.SqliteTransaction"/> class.
        /// </summary>
        /// <param name="provider">The provider used in this transaction</param>
        /// <param name="transaction">
        /// The database transaction that is wrapped by the SqliteTransaction
        /// </param>
        public SqliteTransaction(
            SqliteProvider provider,
            IDbTransaction transaction)
        {
            this.Provider = provider;
            this.Transaction = transaction;
            this.Connection = transaction.Connection;
        }

        /// <summary>
        /// Commits the tranaction to the document store
        /// </summary>
        public void Commit()
        {
            try
            {
                this.Transaction.Commit();
            }
            catch
            {
                this.Transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Rolls back all changes made in this transaction
        /// </summary>
        public void Rollback()
        {
            this.Transaction.Rollback();
        }

        /// <summary>
        /// Releases all resource used by the 
        /// <see cref="T:SqlNoSql.Sqlite.SqliteTransaction"/> object.
        /// </summary>
        /// <remarks>
        /// Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:SqlNoSql.Sqlite.SqliteTransaction"/>. The 
        /// <see cref="Dispose"/> method leaves the
        /// <see cref="T:SqlNoSql.Sqlite.SqliteTransaction"/> in an unusable
        /// state. After calling <see cref="Dispose"/>, you must release all
        /// references to the <see cref="T:SqlNoSql.Sqlite.SqliteTransaction"/>
        /// so the garbage collector can reclaim the memory that the
        /// <see cref="T:SqlNoSql.Sqlite.SqliteTransaction"/> was occupying.
        /// </remarks>
        public void Dispose()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Dispose();
                this.Transaction = null;
            }
            if (this.Connection != null)
            {
                this.Connection.Close();
                this.Connection = null;
            }
            this.Provider.Transaction = null;
        }
    }
}
