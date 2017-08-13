// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umizoo.Messaging
{
    [KnownType("GetKnownTypes")]
    [DataContract]
    public class QueryResult : HandleResult, IQueryResult
    {
        public static readonly QueryResult SentFailed = new QueryResult(HandleStatus.Failed, "Send to bus failed.");
        public static readonly QueryResult Timeout = new QueryResult(HandleStatus.Timeout, "Operation is timeout.");

        private static readonly HashSet<Type> KnownTypes = new HashSet<Type>();

        public QueryResult()
        {
        }

        /// <summary>
        ///     Parameterized Constructor.
        /// </summary>
        public QueryResult(HandleStatus status, string errorMessage = null)
            : base(status, errorMessage)
        {
        }

        public QueryResult(object data)
        {
            Data = data;
        }

        [DataMember]
        public virtual object Data { get; set; }

        private static Type[] GetKnownTypes()
        {
            var array = new Type[KnownTypes.Count];
            KnownTypes.CopyTo(array);

            return array;
        }

        public static void RegisterKnownTypes(params Type[] types)
        {
            foreach (var type in types) KnownTypes.Add(type);
        }
    }


    [DataContract]
    public class QueryResult<T> : HandleResult//, IQueryResult
    {
        public QueryResult()
        {
        }


        public QueryResult(T data)
        {
            Data = data;
        }

        public QueryResult(HandleStatus status, string errorMessage = null)
            : base(status, errorMessage)
        {
        }

        [DataMember]
        public T Data { get; set; }
    }
}