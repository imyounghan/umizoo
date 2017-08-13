// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    public class ParameterCollection : IParameterCollection
    {
        struct ArgumentInfo
        {
            public readonly int Index;
            public readonly string Name;
            public readonly ParameterInfo ParameterInfo;

            public ArgumentInfo(int index, ParameterInfo parameter)
            {
                this.Index = index;
                this.Name = parameter.Name;
                this.ParameterInfo = parameter;
            }
        }

        private readonly List<ParameterCollection.ArgumentInfo> parameters;
        private readonly object[] arguments;

        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public ParameterCollection(object[] arguments, ParameterInfo[] parameters, Predicate<ParameterInfo> isArgumentPartOfCollection)
        {
            Assertions.NotNull(arguments, "arguments");
            Assertions.NotNull(parameters, "parameters");

            this.arguments = arguments;
            this.parameters = new List<ArgumentInfo>();

            for (int index = 0; index < parameters.Length; ++index) {
                if (isArgumentPartOfCollection(parameters[index])) {
                    this.parameters.Add(new ArgumentInfo(index, parameters[index]));
                }
            }
        }

        private int IndexForInputParameterName(string parameterName)
        {
            var index = parameters.FindIndex(p => p.Name == parameterName);
            if (index == -1)
                throw new ArgumentException("Invalid parameter Name", "paramName");

            return index;
        }


        /// <summary>
        /// 通过参数名称获取该参数的值
        /// </summary>
        public object this[string parameterName]
        {
            get
            {
                return arguments[parameters[IndexForInputParameterName(parameterName)].Index];
            }
        }

        public object this[int index]
        {
            get
            {
                return arguments[index];
            }
        }

        /// <summary>
        /// 检查是否包含该参数名称。
        /// </summary>
        public bool ContainsParameter(string parameterName)
        {
            return parameters.Any(p => p.Name == parameterName);
        }
        /// <summary>
        /// 通过参数位置获取参数信息
        /// </summary>
        public ParameterInfo GetParameterInfo(int index)
        {
            return parameters[index].ParameterInfo;
        }
        /// <summary>
        /// 通过参数名称获取参数信息
        /// </summary>
        public ParameterInfo GetParameterInfo(string parameterName)
        {
            return this.GetParameterInfo(IndexForInputParameterName(parameterName));
        }

        /// <summary>
        /// 从特定的索引处开始，将当前的元素复制到一个 <see cref="Array"/> 中
        /// </summary>
        public void CopyTo(Array array, int index)
        {
            int destIndex = 0;
            parameters.GetRange(index, parameters.Count - index).ForEach(
                delegate (ArgumentInfo info) {
                    array.SetValue(arguments[info.Index], destIndex++);
                });
        }

        /// <summary>
        /// 获取包含的元素数。
        /// </summary>
        public int Count
        {
            get { return parameters.Count; }
        }

        /// <summary>
        /// 非线程安全的集合
        /// </summary>
        public bool IsSynchronized
        {
            get { return false; }
        }
        /// <summary>
        /// 获取可用于同步访问的对象。
        /// </summary>
        public object SyncRoot
        {
            get { return this; }
        }
        
        /// <summary>
        /// 返回一个循环访问集合的枚举数。
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < parameters.Count; ++i) {
                yield return arguments[parameters[i].Index];
            }
        }
    }
}