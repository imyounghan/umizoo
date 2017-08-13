// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System.Collections;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// 表示参数集合。
    /// </summary>
    public interface IParameterCollection : ICollection
    {
        /// <summary>
        /// 通过参数名称获取该参数的值
        /// </summary>
        object this[string parameterName] { get; }
        /// <summary>
        /// 通过参数位置获取该参数的值
        /// </summary>
        object this[int index] { get; }

        /// <summary>
        /// 检查是否包含该参数名称。
        /// </summary>
        bool ContainsParameter(string parameterName);

        /// <summary>
        /// 通过参数索引获取参数信息
        /// </summary>
        ParameterInfo GetParameterInfo(int index);
        /// <summary>
        /// 通过参数名称获取参数信息
        /// </summary>
        ParameterInfo GetParameterInfo(string parameterName);
    }
}