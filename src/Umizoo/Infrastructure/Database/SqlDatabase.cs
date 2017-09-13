// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System.Data.Common;
using System.Data.SqlClient;
using Umizoo.Infrastructure.Database.Contexts;

namespace Umizoo.Infrastructure.Database
{
    public class SqlDatabase : Database
    {
        public SqlDatabase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        { }

        public SqlDatabase(string nameOrConnectionString, IContextManager contextManager)
            : base(nameOrConnectionString, contextManager)
        { }

        public SqlDatabase(DbConnection connection)
            : base(connection)
        { }

        protected override string BuildParameterName(string name)
        {
            return string.Concat("@", name);
        }

        protected override DbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        protected override DbConnection CreateConnection()
        {
            return new SqlConnection();
        }

        protected override DbParameter CreateParameter()
        {
            return new SqlParameter();
        }

        protected override DbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }
    }
}
