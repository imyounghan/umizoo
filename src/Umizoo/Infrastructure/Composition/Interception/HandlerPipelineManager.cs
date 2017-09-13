// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-09.

using System.Collections.Generic;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    public class HandlerPipelineManager
    {
        private readonly Dictionary<HandlerPipelineKey, HandlerPipeline> pipelines =
            new Dictionary<HandlerPipelineKey, HandlerPipeline>();

        private static readonly HandlerPipeline EmptyPipeline = new HandlerPipeline();

        public HandlerPipeline GetPipeline(MethodBase method)
        {
            HandlerPipelineKey key = HandlerPipelineKey.ForMethod(method);
            HandlerPipeline pipeline = EmptyPipeline;
            if (pipelines.ContainsKey(key))
            {
                pipeline = pipelines[key];
            }
            return pipeline;
        }

        public void SetPipeline(MethodBase method, HandlerPipeline pipeline)
        {
            HandlerPipelineKey key = HandlerPipelineKey.ForMethod(method);
            pipelines[key] = pipeline;
        }

        public bool InitializePipeline(MethodInfo method, IEnumerable<ICallHandler> handlers)
        {
            Assertions.NotNull(method, "method");

            var pipeline = CreatePipeline(method, handlers);
            //if (method.InterfaceMethodInfo != null)
            //{
            //    pipelines[HandlerPipelineKey.ForMethod(method.InterfaceMethodInfo)] = pipeline;
            //}

            return pipeline.Count > 0;
        }

        private HandlerPipeline CreatePipeline(MethodInfo method, IEnumerable<ICallHandler> handlers)
        {
            HandlerPipelineKey key = HandlerPipelineKey.ForMethod(method);
            if (pipelines.ContainsKey(key))
            {
                return pipelines[key];
            }

            if (method.GetBaseDefinition() == method)
            {
                pipelines[key] = new HandlerPipeline(handlers);
                return pipelines[key];
            }

            var basePipeline = CreatePipeline(method.GetBaseDefinition(), handlers);
            pipelines[key] = basePipeline;
            return basePipeline;
        }
    }
}
