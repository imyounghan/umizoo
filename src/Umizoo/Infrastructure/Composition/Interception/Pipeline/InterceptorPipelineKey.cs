using System;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception.Pipeline
{
    /// <summary>
    /// 拦截器管理的标识
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
        /// 比较该实例是否与当前实例相同
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is InterceptorPipelineKey))
                return false;

            return this == (InterceptorPipelineKey)obj;
        }

        /// <summary>
        /// 获取此实例的哈希代码
        /// </summary>
        public override int GetHashCode()
        {
            return this.module.GetHashCode() ^ methodMetadataToken;
        }

        /// <summary>
        /// 比较两个实例相等
        /// </summary>
        public static bool operator ==(InterceptorPipelineKey left, InterceptorPipelineKey right)
        {
            return left.module == right.module && left.methodMetadataToken == right.methodMetadataToken;
        }

        /// <summary>
        /// 比较两个实例不相等
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
        /// 获取该方法的标识
        /// </summary>
        public static InterceptorPipelineKey ForMethod(MethodBase method)
        {
            method.NotNull("method");

            return new InterceptorPipelineKey(method.DeclaringType.Module, method.MetadataToken);
        }
    }
}
