// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System.Runtime.Serialization;

namespace Umizoo.Communication
{
    /// <summary>
    ///     输出数据
    /// </summary>
    [DataContract]
    public class Response
    {
        /// <summary>
        ///     表示未知类型
        /// </summary>
        public static readonly Response UnknownType = new Response {Message = "Unknown Type.", Status = 404};

        /// <summary>
        ///     成功
        /// </summary>
        public static readonly Response Success = new Response {Status = 200};

        /// <summary>
        ///     表示服务器忙
        /// </summary>
        public static readonly Response ServerTooBusy = new Response(500, "Server too busy.");

        /// <summary>
        ///     反序列化数据失败
        /// </summary>
        public static readonly Response ParsingFailure = new Response(500, "Serialization failure.");

        /// <summary>
        ///     发送到消息总线失败
        /// </summary>
        public static readonly Response SendingFailure = new Response(500, "Send to bus failed.");

        public Response()
        {
        }

        public Response(int status, string message)
        {
            Status = status;
            Message = message;
        }

        #region IResponse 成员

        /// <summary>
        ///     状态码
        /// </summary>
        [DataMember(Name = "status")]
        public int Status { get; set; }

        /// <summary>
        ///     消息
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        #endregion
    }
}