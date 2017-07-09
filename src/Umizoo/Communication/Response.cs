

namespace Umizoo.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// 输出数据
    /// </summary>
    [DataContract]
    public class Response
    {
        /// <summary>
        /// 表示未知类型
        /// </summary>
        public static readonly Response UnknownType = new Response { Message = "Unknown Type.", Status = 404 };
        /// <summary>
        /// 成功
        /// </summary>
        public static readonly Response Success = new Response { Status = 200, };
        /// <summary>
        /// 表示服务器忙
        /// </summary>
        public static readonly Response ServerTooBusy = new Response(500, "Server too busy.");
        /// <summary>
        /// 反序列化数据失败
        /// </summary>
        public static readonly Response ParsingFailure = new Response(500, "Serialization failure.");
        /// <summary>
        /// 发送到消息总线失败
        /// </summary>
        public static readonly Response SendingFailure = new Response(500, "Send to bus failed.");

        public Response()
        { }

        public Response(int status, string message)
        {
            this.Status = status;
            this.Message = message;
        }

        #region IResponse 成员
        /// <summary>
        /// 状态码
        /// </summary>
        [DataMember(Name = "status")]
        public int Status { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        #endregion
    }
}
