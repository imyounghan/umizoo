// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-09.

using System.Collections.Generic;

namespace Umizoo.Infrastructure.Composition.Interception
{
    public class HandlerPipeline
    {
        private readonly List<ICallHandler> _handlers;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public HandlerPipeline()
        {
            _handlers = new List<ICallHandler>();
        }

        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        public HandlerPipeline(IEnumerable<ICallHandler> handlers)
        {
            _handlers = new List<ICallHandler>(handlers);
        }

        /// <summary>
        ///     拦截器数据
        /// </summary>
        public int Count
        {
            get { return _handlers.Count; }
        }

        /// <summary>
        ///     调用结果
        /// </summary>
        public IMethodReturn Invoke(IMethodInvocation input, InvokeHandlerDelegate target)
        {
            if (Count == 0)
                return target(input, null);

            int handlerIndex = 0;

            var result = _handlers[0].Invoke(input, delegate
            {
                ++handlerIndex;
                if (handlerIndex < _handlers.Count)
                {
                    return _handlers[handlerIndex].Invoke;
                }
                else
                {
                    return target;
                }
            });

            return result;
        }
    }
}
