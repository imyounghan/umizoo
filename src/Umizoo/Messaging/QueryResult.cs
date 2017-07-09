namespace Umizoo.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;


    [KnownType("GetKnownTypes")]
    [DataContract]
    public class QueryResult : HandleResult, IQueryResult
    {
        public readonly static QueryResult SentFailed = new QueryResult(HandleStatus.Failed, "Send to bus failed.");
        public readonly static QueryResult Timeout = new QueryResult(HandleStatus.Timeout, "Operation is timeout.");

        public QueryResult()
        { }

        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public QueryResult(HandleStatus status, string errorMessage = null)
            : base(status, errorMessage)
        { }

        public QueryResult(object data)
        {
            this.Data = data;
        }

        [DataMember]
        public virtual object Data { get; set; }

        static readonly HashSet<Type> KnownTypes = new HashSet<Type>();

        static Type[] GetKnownTypes()
        {
            Type[] array = new Type[KnownTypes.Count];
            KnownTypes.CopyTo(array);

            return array;
        }

        public static void RegisterKnownTypes(params Type[] types)
        {
            foreach (var type in types) {
                KnownTypes.Add(type);
            }
        }

        //public static QueryResult Create<T>(T data)
        //{
        //    return new QueryResult<T>(data);
        //}
    }

    
    [DataContract]
    public class QueryResult<T> : HandleResult, IQueryResult
    {
        public QueryResult()
        { }


        public QueryResult(T data)
        {
            this.Data = data;
        }

        public QueryResult(HandleStatus status, string errorMessage = null)
            : base(status, errorMessage)
        { }

        [DataMember]
        public T Data { get; set; }

        //[DataMember]
        //public T Metadata { get; set; }

        #region IQueryResult 成员

        object IQueryResult.Data
        {
            get
            {
                return this.Data;
            }
        }

        #endregion
    }
}
