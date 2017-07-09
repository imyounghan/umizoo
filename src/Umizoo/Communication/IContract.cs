

namespace Umizoo.Communication
{
    using System;
    using System.ServiceModel;

    [ServiceContract]
    public interface IContract
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
