// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.


using System;
using System.ServiceModel;

namespace Umizoo.Communication
{
    [ServiceContract]
    public interface IChannel
    {
        /// <summary>
        /// 发送一个请求异步返回结果
        /// </summary>
        /// <param name="request">一个请求信息</param>
        /// <param name="callback">回调函数</param>
        /// <param name="state">状态</param>
        /// <returns>异步结果</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginExecute(Request request, AsyncCallback callback, object state);


        Response EndExecute(IAsyncResult asyncResult);
    }
}