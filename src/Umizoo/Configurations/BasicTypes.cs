// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Collections.Generic;

namespace Umizoo.Configurations
{
    public static class BasicTypes
    {
        public static IDictionary<string, Type> CommandTypes { get; internal set; }

        public static IDictionary<string, Type> EventTypes { get; internal set; }

        public static IDictionary<string, Type> AggregateTypes { get; internal set; }

        public static IDictionary<string, Type> PublishableExceptionTypes { get; internal set; }

        public static IDictionary<string, Type> QueryTypes { get; internal set; }

        //public static IDictionary<string, Type> ResultTypes { get; internal set; }
    }
}
