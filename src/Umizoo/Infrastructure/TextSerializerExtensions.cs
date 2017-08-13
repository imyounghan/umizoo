// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Text;

namespace Umizoo.Infrastructure
{
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