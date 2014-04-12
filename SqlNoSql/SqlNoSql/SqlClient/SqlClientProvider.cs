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

namespace SqlNoSql.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data;
    using System.Data.SqlClient;
    using Dapper;
    using SqlNoSql.Generic;

    public class SqlClientProvider : IDbProvider
    {
        private string connectionString;

        public SqlClientProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public bool TableExists(string name)
        {
            using (var connection = this.GetConnection())
            {
                connection.Open();
                return connection.Query<int>(
                    "SELECT CAST(COUNT(*) AS INT) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @Name",
                    new { Schema = "dbo", Name = name }).Single() > 0;
            }
        }


        public void CreateCollectionTable(string name)
        {
            using (var connection = this.GetConnection())
            {
                connection.Open();
                connection.Execute(string.Format("CREATE TABLE {0}", name));
            }
        }

        public void DeleteTable(string name)
        {
        }


        public void CreateTable(string name)
        {
            throw new NotImplementedException();
        }
    }
}
