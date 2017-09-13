// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 对象生命周期调用
    /// </summary>
    public interface ILifecycle
    {
        /// <summary>
        /// Insert前回调
        /// </summary>
        LifecycleVeto OnInserting(IDbContext context);
        /// <summary>
        /// Update前回调
        /// </summary>
        LifecycleVeto OnUpdating(IDbContext context);
        /// <summary>
        /// Delete前回调
        /// </summary>
        LifecycleVeto OnDeleting(IDbContext context);
        /// <summary>
        /// Load后回调
        /// </summary>
        void OnLoaded(IDbContext context);
    }
}
