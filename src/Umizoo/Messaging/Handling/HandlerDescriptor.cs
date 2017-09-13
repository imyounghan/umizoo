// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Umizoo.Configurations;
using Umizoo.Infrastructure.Logging;

namespace Umizoo.Messaging.Handling
{
    public enum HandlerStyle
    {
        /// <summary>
        /// 简单方式
        /// </summary>
        Simple,
        /// <summary>
        /// 高级方式
        /// </summary>
        Senior,
        /// <summary>
        /// 特殊方式
        /// </summary>
        Special,
    }

    public class HandlerDescriptor
    {
        private object target;
        private Type handlerType;

        public HandlerDescriptor(object handler, Func<Type, MethodInfo> methodFactory, HandlerStyle handlerStyle)
        {
            this.target = handler;
            this.handlerType = handler.GetType();
            this.Method = methodFactory(handlerType);
            this.HandlerStyle = handlerStyle;
        }

        public MethodInfo Method { get; private set; }

        public object Target { get { return this.target; } }

        public HandlerStyle HandlerStyle { get; private set; }

        public object Invode(params object[] parameters)
        {
            var retryTimes = ConfigurationSettings.HandleRetrytimes;
            var retryInterval = ConfigurationSettings.HandleRetryInterval;

            int count = 0;
            object result = null;
            while (count++ < retryTimes)
                try {
                    result = Method.Invoke(target, parameters);
                    break;
                }
                catch (ApplicationException ex) {
                    LogManager.Default.Error(
                        ex,
                        "ApplicationException raised when handling '{0}' on '{1}', exit retry and throw.",
                        parameters.Last(),
                        handlerType.FullName);
                    throw ex;
                }
                catch (SystemException ex) {
                    LogManager.Default.Error(
                        ex,
                        "SystemException raised when handling '{0}' on '{1}', exit retry and throw.",
                        parameters.Last(),
                        handlerType.FullName);
                    throw ex;
                }
                catch (Exception ex) {
                    if (count == retryTimes) {
                        LogManager.Default.Error(
                            ex,
                            "Exception raised when handling '{0}' on '{1}', the retry count has been reached.",
                            parameters.Last(),
                            handlerType.FullName);
                        throw ex;
                    }

                    Thread.Sleep(retryInterval);
                }

            if (LogManager.Default.IsDebugEnabled)
                LogManager.Default.DebugFormat("Handle '{0}' on '{1}' successfully.",
                    parameters.Last(),
                    handlerType.FullName);

            return result;
        }

    }
}
