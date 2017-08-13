// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;

namespace Umizoo.Seeds
{
    public class AggregateStorageInMemory : IAggregateStorage
    {
        public void Delete(IAggregateRoot aggregateRoot)
        {
        }

        public IAggregateRoot Find(Type aggregateRootType, object aggregateRootId)
        {
            return null;
        }

        public void Save(IAggregateRoot aggregateRoot)
        {
        }
    }
}