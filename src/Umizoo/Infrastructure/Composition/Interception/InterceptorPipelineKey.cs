// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    ///     拦截器管理的标识
    /// </summary>
    public struct InterceptorPipelineKey : IEquatable<InterceptorPipelineKey>
    {
        private readonly Module module;
        private readonly int methodMetadataToken;

        private InterceptorPipelineKey(Module module, int methodMetadataToken)
        {
            this.module = module;
            this.methodMetadataToken = methodMetadataToken;
        }

        /// <summary>
        ///     比较该实例是否与当前实例相同
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is InterceptorPipelineKey))
                return false;

            return this == (InterceptorPipelineKey) obj;
        }

        /// <summary>
        ///     获取此实例的哈希代码
        /// </summary>
        public override int GetHashCode()
        {
            return module.GetHashCode() ^ methodMetadataToken;
        }

        /// <summary>
        ///     比较两个实例相等
        /// </summary>
        public static bool operator ==(InterceptorPipelineKey left, InterceptorPipelineKey right)
        {
            return left.module == right.module && left.methodMetadataToken == right.methodMetadataToken;
        }

        /// <summary>
        ///     比较两个实例不相等
        /// </summary>
        public static bool operator !=(InterceptorPipelineKey left, InterceptorPipelineKey right)
        {
            return !(left == right);
        }

        #region IEquatable<InterceptorPipelineKey> 成员

        bool IEquatable<InterceptorPipelineKey>.Equals(InterceptorPipelineKey other)
        {
            return this == other;
        }

        #endregion

        /// <summary>
        ///     获取该方法的标识
        /// </summary>
        public static InterceptorPipelineKey ForMethod(MethodBase method)
        {
            Assertions.NotNull(method, "method");

            return new InterceptorPipelineKey(method.DeclaringType.Module, method.MetadataToken);
        }
    }
}