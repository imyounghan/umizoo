// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using Umizoo.Configurations;
using Umizoo.Infrastructure.Database.Contexts;

namespace Umizoo.Infrastructure.Database
{
    public class SqlDatabaseFactory : ContextManager, IDatabaseFactory
    {
        public SqlDatabaseFactory()
            : base(ConfigurationSettings.ContextType)
        { }

        public IDatabase Create()
        {
            return this.Create("default");
        }

        public IDatabase Create(string nameOrConnectionString)
        {
            return new SqlDatabase(nameOrConnectionString, this);
        }

        public IDatabase GetCurrent()
        {
            return CurrentContext.CurrentContext() as IDatabase;
        }
    }
}
