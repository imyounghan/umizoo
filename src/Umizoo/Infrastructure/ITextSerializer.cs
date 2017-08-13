// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;

namespace Umizoo.Infrastructure
{
    /// <summary>
    ///     表示一个序列化器。用来序列化对象的字符串形式
    /// </summary>
    public interface ITextSerializer
    {
        /// <summary>
        ///     序列化一个对象
        /// </summary>
        string Serialize(object obj);

        /// <summary>
        ///     根据
        ///     <param name="type" />
        ///     从
        ///     <param name="serialized" />
        ///     反序列化一个对象。
        /// </summary>
        object Deserialize(string serialized, Type type);
    }
}