// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Script.Serialization;

namespace Umizoo.Infrastructure
{
    /// <summary>
    ///     <see cref="ITextSerializer" /> 的默认实现。
    /// </summary>
    public class TextSerializer : ITextSerializer
    {
        public static readonly ITextSerializer Instance = new TextSerializer();


        private readonly ITextSerializer serializer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextSerializer" /> class.
        /// </summary>
        public TextSerializer()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            var binPath = string.IsNullOrEmpty(relativeSearchPath)
                ? baseDir
                : Path.Combine(baseDir, relativeSearchPath);
            var jsonDllPath = string.IsNullOrEmpty(binPath)
                ? "Newtonsoft.Json.dll"
                : Path.Combine(binPath, "Newtonsoft.Json.dll");

            if (File.Exists(jsonDllPath)
                || AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Newtonsoft.Json"))
                serializer = new JsonSerializer();
            else
                serializer = new NetFrameworkSerializer();
        }

        private class JsonSerializer : ITextSerializer
        {
            private static readonly Func<string, Type, object> DeserializeObjectDelegate;
            private static readonly Func<object, string> SerializeObjectDelegate;
            private static readonly Type JsonConvertType;

            static JsonSerializer()
            {
                JsonConvertType = Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
                DeserializeObjectDelegate = GetDeserializeObjectMethodCall();
                SerializeObjectDelegate = GetSerializeObjectMethodCall();
            }

            public object Deserialize(string serialized, Type type)
            {
                return DeserializeObjectDelegate(serialized, type);
            }

            public string Serialize(object obj)
            {
                return SerializeObjectDelegate(obj);
            }

            private static Func<string, Type, object> GetDeserializeObjectMethodCall()
            {
                var method = JsonConvertType.GetMethod("DeserializeObject", new[] {typeof(string), typeof(Type)});
                var valueParam = Expression.Parameter(typeof(string), "value");
                var typeParam = Expression.Parameter(typeof(Type), "type");
                var methodCall = Expression.Call(null, method, valueParam, typeParam);

                return Expression.Lambda<Func<string, Type, object>>(methodCall, valueParam, typeParam).Compile();
            }

            private static Func<object, string> GetSerializeObjectMethodCall()
            {
                var method = JsonConvertType.GetMethod("SerializeObject", new[] {typeof(object)});
                var valueParam = Expression.Parameter(typeof(object), "value");
                var methodCall = Expression.Call(null, method, valueParam);

                return Expression.Lambda<Func<object, string>>(methodCall, valueParam).Compile();
            }
        }


        private class NetFrameworkSerializer : ITextSerializer
        {
            private readonly JavaScriptSerializer serializer;

            public NetFrameworkSerializer()
            {
                serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] {new DateTimeConverter()});
            }

            public object Deserialize(string serialized, Type type)
            {
                //var serializer = new DataContractJsonSerializer(type);
                //byte[] buffer = Encoding.UTF8.GetBytes(serialized);
                //using (var stream = new MemoryStream(buffer)) {
                //    return serializer.ReadObject(stream);
                //}
                return serializer.Deserialize(serialized, type);
            }

            public string Serialize(object obj)
            {
                //Type type = obj.GetType();
                //var serializer = new DataContractJsonSerializer(type);
                //using (var stream = new MemoryStream()) {
                //    serializer.WriteObject(stream, obj);
                //    return Encoding.UTF8.GetString(stream.ToArray());
                //}
                return serializer.Serialize(obj);
            }
        }


        private class DateTimeConverter : JavaScriptConverter
        {
            public override IEnumerable<Type> SupportedTypes
            {
                get { yield return typeof(DateTime); }
            }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type,
                JavaScriptSerializer serializer)
            {
                return serializer.ConvertToType(dictionary, type);
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                if (!(obj is DateTime))
                    return null;

                return new DateTimeDictionary(((DateTime) obj).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"));
            }
        }

        private class DateTimeDictionary : Uri, IDictionary<string, object>
        {
            public DateTimeDictionary(string datetime)
                : base(datetime, UriKind.Relative)
            {
            }


            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(string key, object value)
            {
            }

            public bool ContainsKey(string key)
            {
                return false;
            }

            public ICollection<string> Keys => throw new NotImplementedException();

            public bool Remove(string key)
            {
                return false;
            }

            public bool TryGetValue(string key, out object value)
            {
                value = null;
                return false;
            }

            public ICollection<object> Values
            {
                get { return null; }
            }

            public object this[string key]
            {
                get { return null; }
                set { }
            }

            public void Add(KeyValuePair<string, object> item)
            {
            }

            public void Clear()
            {
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                return false;
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                return false;
            }
        }

        #region Methods and Operators

        /// <summary>
        ///     根据
        ///     <param name="type" />
        ///     从
        ///     <param name="serialized" />
        ///     反序列化一个对象。
        /// </summary>
        public object Deserialize(string serialized, Type type)
        {
            return serializer.Deserialize(serialized, type);
        }

        /// <summary>
        ///     序列化一个对象
        /// </summary>
        public string Serialize(object obj)
        {
            return serializer.Serialize(obj);
        }

        #endregion
    }
}