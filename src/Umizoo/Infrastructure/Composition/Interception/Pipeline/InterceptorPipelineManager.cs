using System;
using System.Collections.Generic;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception.Pipeline
{
    /// <summary>
    /// 拦截器管理的管理器
    /// </summary>
    public class InterceptorPipelineManager
    {
        //private static readonly InterceptorPipeline EmptyPipeline = new InterceptorPipeline();

        /// <summary>
        /// 一个 <see cref="InterceptorPipelineManager"/> 的实例
        /// </summary>
        public static readonly InterceptorPipelineManager Instance = new InterceptorPipelineManager();

        private readonly Dictionary<InterceptorPipelineKey, InterceptorPipeline> pipelines = new Dictionary<InterceptorPipelineKey, InterceptorPipeline>();

        /// <summary>
        /// 获取当前方法的拦截器管道
        /// </summary>
        public InterceptorPipeline GetPipeline(MethodBase method)
        {
            var key = InterceptorPipelineKey.ForMethod(method);
            if (pipelines.ContainsKey(key))
                return pipelines[key];

            return InterceptorPipeline.Empty;
        }

        /// <summary>
        /// 设置当前方法的拦截器管道
        /// </summary>
        public void SetPipeline(MethodBase method, InterceptorPipeline pipeline)
        {
            var key = InterceptorPipelineKey.ForMethod(method);
            pipelines[key] = pipeline;
        }

        //public bool InitializePipeline(methodi)
        //{
        //}

        /// <summary>
        /// 创建并返回当前方法的拦截器管道
        /// </summary>
        public InterceptorPipeline CreatePipeline(MethodInfo method, IEnumerable<IInterceptor> interceptors)
        {
            var key = InterceptorPipelineKey.ForMethod(method);

            if (pipelines.ContainsKey(key))
                return pipelines[key];

            if (method.GetBaseDefinition() == method)
                return pipelines[key] = new InterceptorPipeline(interceptors);

            return pipelines[key] = CreatePipeline(method.GetBaseDefinition(), interceptors);
        }

        ///// <summary>
        ///// 创建并返回当前方法的拦截器管道
        ///// </summary>
        //public InterceptorPipeline CreatePipeline(MethodInfo method, Func<IEnumerable<IInterceptor>> getInterceptors)
        //{
        //    var key = InterceptorPipelineKey.ForMethod(method);

        //    if (pipelines.ContainsKey(key))
        //        return pipelines[key];

        //    if (method.GetBaseDefinition() == method)
        //        return pipelines[key] = new InterceptorPipeline(getInterceptors());

        //    return pipelines[key] = CreatePipeline(method.GetBaseDefinition(), getInterceptors);
        //}

        /// <summary>
        /// 创建并返回当前方法的拦截器管道
        /// </summary>
        public InterceptorPipeline CreatePipeline(MethodInfo method, Func<MethodInfo, IEnumerable<IInterceptor>> getInterceptors)
        {
            var key = InterceptorPipelineKey.ForMethod(method);

            if(pipelines.ContainsKey(key))
                return pipelines[key];

            if(method.GetBaseDefinition() == method)
                return pipelines[key] = new InterceptorPipeline(getInterceptors(method));

            return pipelines[key] = CreatePipeline(method.GetBaseDefinition(), getInterceptors);
        }
    }
}
