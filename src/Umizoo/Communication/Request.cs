// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umizoo.Communication
{
    /// <summary>
    ///     请求数据
    /// </summary>
    [DataContract]
    public class Request
    {
        /// <summary>
        ///     包括的头信息
        /// </summary>
        [DataMember(Name = "header", EmitDefaultValue = false)]
        public IDictionary<string, string> Header { get; set; }

        /// <summary>
        ///     正文
        /// </summary>
        [DataMember(Name = "body")]
        public string Body { get; set; }
    }
}