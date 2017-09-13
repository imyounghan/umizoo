// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 数据上下文
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// 获取跟踪的对象集合
        /// </summary>
        IEnumerable TrackingObjects { get; }

        /// <summary>
        /// 判断此 <paramref name="entity"/> 是否存在于当前上下文中
        /// </summary>
        bool Contains(object entity);
        /// <summary>
        /// 从当前上下文中分离此 <paramref name="entity"/>
        /// </summary>
        void Detach(object entity);
        /// <summary>
        /// 写入(提交时会触发sql-insert)
        /// </summary>
        void Insert(object entity);
        /// <summary>
        /// 更新(提交时会触发sql-update)
        /// </summary>
        void Update(object entity);
        /// <summary>
        /// 保存。如何存在更新，不存在则新增
        /// </summary>
        void Save(object entity);
        /// <summary>
        /// 删除(提交时会触发sql-delete)
        /// </summary>
        void Delete(object entity);

        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 获取实体信息
        /// </summary>
        object Get(Type type, params object[] keyValues);
        /// <summary>
        /// 从数据中刷新(触发sql-select)
        /// </summary>
        void Refresh(object entity);



        /// <summary>
        /// 获取对数据类型已知的特定数据源的查询进行计算的功能。
        /// </summary>
        IQueryable<TEntity> CreateQuery<TEntity>() where TEntity : class;

        /// <summary>
        /// 获取当前的数据连接
        /// </summary>
        IDbConnection DbConnection { get; }

        /// <summary>
        /// 当数据提交后执行
        /// </summary>
        event EventHandler DataCommitted;
    }
}
