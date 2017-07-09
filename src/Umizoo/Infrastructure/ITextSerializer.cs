

namespace Umizoo.Infrastructure
{
    using System;
    using System.Text;

    /// <summary>
    /// 表示一个序列化器。用来序列化对象的字符串形式
    /// </summary>
    public interface ITextSerializer
    {
        /// <summary>
        /// 序列化一个对象
        /// </summary>
        string Serialize(object obj);

        /// <summary>
        /// 根据 <param name="type" /> 从 <param name="serialized" /> 反序列化一个对象。
        /// </summary>
        object Deserialize(string serialized, Type type);
    }


    /// <summary>
    /// 序列化器的扩展类
    /// </summary>
    public static class TextSerializerExtensions
    {
        /// <summary>
        /// 从字符串反序列化一个对象。
        /// </summary>
        public static T Deserialize<T>(this ITextSerializer serializer, string serialized)
        {
            return (T)serializer.Deserialize(serialized, typeof(T));
        }

        /// <summary>
        /// 序列化一个对象
        /// </summary>
        public static byte[] SerializeToBytes(this ITextSerializer serializer, object obj)
        {
            var serialized = serializer.Serialize(obj);
            return Encoding.UTF8.GetBytes(serialized);
        }

        /// <summary>
        /// 从二进制数据反序列化一个对象。
        /// </summary>
        public static T Deserialize<T>(this ITextSerializer serializer, byte[] serialized)
        {
            var text = Encoding.UTF8.GetString(serialized);

            return serializer.Deserialize<T>(text);
        }

        /// <summary>
        /// 从二进制数据反序列化一个对象。
        /// </summary>
        public static object Deserialize(this ITextSerializer serializer, byte[] serialized, Type type)
        {
            var text = Encoding.UTF8.GetString(serialized);

            return serializer.Deserialize(text, type);
        }
    }
}
