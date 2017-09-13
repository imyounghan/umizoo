// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.


namespace Umizoo.Infrastructure.Database.Contexts
{
    /// <summary>
    /// 当前访问的上下文接口
    /// </summary>
    public interface ICurrentContext
    {
        /// <summary>
        /// 获取当前的上下文
        /// </summary>
        IContext CurrentContext();
    }
}
