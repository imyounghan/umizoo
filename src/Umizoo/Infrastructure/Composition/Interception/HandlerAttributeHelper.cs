// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-09.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    public class HandlerAttributeHelper
    {
        public static IEnumerable<ICallHandler> GetHandlersFor(MethodBase method, IObjectContainer container)
        {
            var ordered = new List<HandlerAttribute>();
            var nonOrdered = new List<HandlerAttribute>();

            var allAttributes = method.ReflectedType.GetAllAttributes<HandlerAttribute>(false).Concat(method.GetAllAttributes<HandlerAttribute>(false));
            foreach (var attr in allAttributes)
            {
                if (attr.Order != 0)
                {
                    bool inserted = false;
                    // add in order to ordered
                    for (int i = ordered.Count - 1; i >= 0; i--)
                    {
                        if (ordered[i].Order <= attr.Order)
                        {
                            ordered.Insert(i + 1, attr);
                            inserted = true;
                            break;
                        }
                    }
                    if (!inserted)
                    {
                        ordered.Insert(0, attr);
                    }
                }
                else
                {
                    nonOrdered.Add(attr);
                }
            }

            ordered.AddRange(nonOrdered);
            ordered.Reverse();


            return RemoveDuplicates(ordered).Reverse().Select(attr => attr.CreateHandler(container));
        }

        private static IEnumerable<HandlerAttribute> RemoveDuplicates(IEnumerable<HandlerAttribute> filters)
        {
            var visitedTypes = new HashSet<Type>();

            foreach (var filter in filters)
            {
                var attributeType = filter.GetType();
                if (!visitedTypes.Contains(attributeType) || filter.AllowMultiple)
                {
                    yield return filter;
                    visitedTypes.Add(attributeType);
                }
            }
        }
    }
}
