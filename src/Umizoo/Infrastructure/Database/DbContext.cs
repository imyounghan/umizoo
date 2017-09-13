// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Collections;
using System.Data;
using System.Linq;
using Umizoo.Infrastructure.Database.Contexts;

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 实现 <see cref="IDbContext"/> 的抽象类
    /// </summary>
    public abstract class DbContext : DisposableObject, IDbContext, IContext
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected DbContext()
        { }

        private readonly IContextManager _contextManager;
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected DbContext(IContextManager contextManager)
        {
            this._contextManager = contextManager;
        }

        /// <summary>
        /// 当前的数据连接
        /// </summary>
        public abstract IDbConnection DbConnection { get; }

        private void Validate(object entity)
        {
            if (entity is IValidatable) {
                (entity as IValidatable).Validate(this);
            }
        }
        private LifecycleVeto Callback(object entity, Func<ILifecycle, IDbContext, LifecycleVeto> action)
        {
            if (entity is ILifecycle) {
                return action(entity as ILifecycle, this);
            }
            return LifecycleVeto.Accept;
        }
        private static LifecycleVeto OnInserting(ILifecycle entity, IDbContext context)
        {
            return (entity as ILifecycle).OnInserting(context);
        }
        private static LifecycleVeto OnUpdating(ILifecycle entity, IDbContext context)
        {
            return (entity as ILifecycle).OnUpdating(context);
        }
        private static LifecycleVeto OnDeleting(ILifecycle entity, IDbContext context)
        {
            return (entity as ILifecycle).OnDeleting(context);
        }
        private static string GetKey(object entity)
        {
            return string.Concat(entity.GetType().FullName, "@", entity.GetHashCode());
        }

        /// <summary>
        /// 预处理事务。
        /// </summary>
        protected abstract void DoCommit();


        /// <summary>
        /// 获取跟踪的对象集合
        /// </summary>
        public abstract IEnumerable TrackingObjects { get; }


        /// <summary>
        /// 提交事务。
        /// </summary>
        public void Commit()
        {
            this.DoCommit();

            this.DataCommitted(this, EventArgs.Empty);
        }

        /// <summary>
        /// 新增一个新对象到当前上下文
        /// </summary>
        public abstract void Insert(object entity);
        void IDbContext.Insert(object entity)
        {
            this.Validate(entity);
            if (this.Callback(entity, OnInserting) == LifecycleVeto.Veto)
                return;

            this.Insert(entity);
        }

        /// <summary>
        /// 修改一个对象到当前上下文
        /// </summary>
        public abstract void Update(object entity);
        void IDbContext.Update(object entity)
        {
            this.Validate(entity);
            if (this.Callback(entity, OnUpdating) == LifecycleVeto.Veto)
                return;

            this.Update(entity);
        }

        /// <summary>
        /// 保存。如何存在更新，不存在则新增
        /// </summary>
        public virtual void Save(object entity)
        {
            if (Contains(entity)) {
                ((IDbContext)this).Insert(entity);
            }
            else {
                ((IDbContext)this).Update(entity);
            }
        }

        /// <summary>
        /// 删除一个对象到当前上下文
        /// </summary>
        public abstract void Delete(object entity);
        void IDbContext.Delete(object entity)
        {
            this.Validate(entity);
            if (this.Callback(entity, OnDeleting) == LifecycleVeto.Veto)
                return;

            this.Delete(entity);
        }

        /// <summary>
        /// 当前工作单元是否包含此实体
        /// </summary>
        public abstract bool Contains(object entity);
        /// <summary>
        /// 从当前工作分离此实体
        /// </summary>
        public abstract void Detach(object entity);

        /// <summary>
        /// 从数据库刷新最新状态的实体
        /// </summary>
        public abstract void Refresh(object entity);
        void IDbContext.Refresh(object entity)
        {
            this.Refresh(entity);

            if (entity != null && entity is ILifecycle) {
                (entity as ILifecycle).OnLoaded(this);
            }
        }
        /// <summary>
        /// 获取实体信息
        /// </summary>
        public abstract object Get(Type type, params object[] keyValues);
        object IDbContext.Get(Type type, params object[] keyValues)
        {
            var entity = this.Get(type, keyValues);
            if (entity != null && entity is ILifecycle) {
                (entity as ILifecycle).OnLoaded(this);
            }

            return entity;
        }

        /// <summary>
        /// 获取对数据类型已知的特定数据源的查询进行计算的功能。
        /// </summary>
        public abstract IQueryable<TEntity> CreateQuery<TEntity>() where TEntity : class;


        /// <summary>
        /// 在数据提交成功后执行
        /// </summary>
        public event EventHandler DataCommitted = (sender, args) => { };

        IContextManager IContext.ContextManager
        {
            get { return this._contextManager; }
        }
    }
}
