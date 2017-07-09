
namespace Umizoo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Web.Script.Serialization;

    /// <summary>
    /// <see cref="ITextSerializer" /> 的默认实现。
    /// </summary>
    public class DefaultTextSerializer : ITextSerializer
    {
        public static readonly ITextSerializer Instance = new DefaultTextSerializer();

        #region Fields

        private readonly ITextSerializer serializer;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTextSerializer"/> class.
        /// </summary>
        public DefaultTextSerializer()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            string binPath = string.IsNullOrEmpty(relativeSearchPath)
                                 ? baseDir
                                 : Path.Combine(baseDir, relativeSearchPath);
            string jsonDllPath = string.IsNullOrEmpty(binPath)
                                     ? "Newtonsoft.Json.dll"
                                     : Path.Combine(binPath, "Newtonsoft.Json.dll");

            if (File.Exists(jsonDllPath)
                || AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Newtonsoft.Json"))
            {
                this.serializer = new JsonSerializer();
            }
            else
            {
                this.serializer = new NetFrameworkSerializer();
            }
        }

        #endregion

        #region Methods and Operators

        /// <summary>
        /// 根据 <param name="type" /> 从 <param name="serialized" /> 反序列化一个对象。
        /// </summary>
        public object Deserialize(string serialized, Type type)
        {
            return this.serializer.Deserialize(serialized, type);
        }

        /// <summary>
        /// 序列化一个对象
        /// </summary>
        public string Serialize(object obj)
        {
            return this.serializer.Serialize(obj);
        }

        #endregion

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

            private static Func<string, Type, object> GetDeserializeObjectMethodCall()
            {
                MethodInfo method = JsonConvertType.GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
                ParameterExpression valueParam = Expression.Parameter(typeof(string), "value");
                ParameterExpression typeParam = Expression.Parameter(typeof(Type), "type");
                MethodCallExpression methodCall = Expression.Call(null, method, valueParam, typeParam);

                return Expression.Lambda<Func<string, Type, object>>(methodCall, new[] { valueParam, typeParam }).Compile();
            }

            private static Func<object, string> GetSerializeObjectMethodCall()
            {
                MethodInfo method = JsonConvertType.GetMethod("SerializeObject", new[] { typeof(object) });
                ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");
                MethodCallExpression methodCall = Expression.Call(null, method, valueParam);

                return Expression.Lambda<Func<object, string>>(methodCall, new[] { valueParam }).Compile();
            }

            public object Deserialize(string serialized, Type type)
            {
                return DeserializeObjectDelegate(serialized, type);
            }

            public string Serialize(object obj)
            {
                return SerializeObjectDelegate(obj);
            }
        }
        

        private class NetFrameworkSerializer : ITextSerializer
        {
            private readonly JavaScriptSerializer serializer;

            public NetFrameworkSerializer()
            {
                this.serializer = new JavaScriptSerializer();
                this.serializer.RegisterConverters(new[] { new DateTimeConverter() });
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
                return this.serializer.Serialize(obj);
            }
        }


        class DateTimeConverter : JavaScriptConverter
        {
            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                return serializer.ConvertToType(dictionary, type);
            }
            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                if (!(obj is DateTime))
                {
                    return null;
                }

                return new DateTimeDictionary(((DateTime)obj).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"));
            }
            public override IEnumerable<Type> SupportedTypes
            {
                get { yield return typeof(DateTime); }
            }
        }

        class DateTimeDictionary : Uri, IDictionary<string, object>
        {
            public DateTimeDictionary(string datetime) 
                :base(datetime, UriKind.Relative)
            { }

            #region IDictionary<string,object> 成员

            public void Add(string key, object value)
            {
                throw new NotImplementedException();
            }

            public bool ContainsKey(string key)
            {
                throw new NotImplementedException();
            }

            public ICollection<string> Keys
            {
                get { throw new NotImplementedException(); }
            }

            public bool Remove(string key)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(string key, out object value)
            {
                throw new NotImplementedException();
            }

            public ICollection<object> Values
            {
                get { throw new NotImplementedException(); }
            }

            public object this[string key]
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            #endregion

            #region ICollection<KeyValuePair<string,object>> 成员

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerable<KeyValuePair<string,object>> 成员

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerable 成员

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}