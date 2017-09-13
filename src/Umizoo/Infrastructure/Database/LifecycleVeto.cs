// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 生命周期状态
    /// </summary>
    public enum LifecycleVeto
    {
        /// <summary>
        /// 接受
        /// </summary>
        Accept,
        /// <summary>
        /// 否决
        /// </summary>
        Veto
    }
}
