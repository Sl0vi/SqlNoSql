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

namespace SqlNoSql.MonoSqlite
{
    using System.Data;

    internal class MonoSqliteTransaction : ITransaction
    {
        public MonoSqliteProvider Provider { get; private set; }
        public IDbTransaction Transaction { get; private set; }
        public IDbConnection Connection { get; private set; }

        public MonoSqliteTransaction(
            MonoSqliteProvider provider,
            IDbTransaction transaction)
        {
            this.Provider = provider;
            this.Transaction = transaction;
            this.Connection = transaction.Connection;
        }

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

        public void Rollback()
        {
            this.Transaction.Rollback();
        }

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
