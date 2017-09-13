// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 实体验证接口
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// 持久化之前验证
        /// </summary>
        void Validate(IDbContext context);
    }
}
