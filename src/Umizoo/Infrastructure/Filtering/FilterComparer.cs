// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System.Collections.Generic;

namespace Umizoo.Infrastructure.Filtering
{
    public class FilterComparer : IComparer<Filter>
    {
        public static readonly FilterComparer Default = new FilterComparer();

        public int Compare(Filter x, Filter y)
        {
            // Nulls always have to be less than non-nulls
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Sort first by order...

            if (x.Order < y.Order) return -1;
            if (x.Order > y.Order) return 1;

            // ...then by scope

            if (x.Scope < y.Scope) return -1;
            if (x.Scope > y.Scope) return 1;

            return 0;
        }
    }
}