// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// <see cref="IDbContext"/> 的扩展查询类
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// 获取实体信息
        /// </summary>
        public static TEntity Get<TEntity>(this IDbContext db, params object[] keyValues) where TEntity : class
        {
            return (TEntity)db.Get(typeof(TEntity), keyValues);
        }

        /// <summary>
        /// 获取符合条件的记录总数
        /// </summary>
        public static int Count<TEntity>(this IDbContext db, ICriteria<TEntity> criteria) where TEntity : class
        {
            var queryable = db.CreateQuery<TEntity>();
            return (criteria ?? Criteria<TEntity>.Empty).Filtered(queryable).Count();
        }
        /// <summary>
        /// 根据查询条件是否存在相关数据
        /// </summary>
        public static bool Exists<TEntity>(this IDbContext db, ICriteria<TEntity> criteria) where TEntity : class
        {
            var queryable = db.CreateQuery<TEntity>();
            return (criteria ?? Criteria<TEntity>.Empty).Filtered(queryable).Any();
        }

        /// <summary>
        /// 获得单个实体
        /// </summary>
        public static TEntity Single<TEntity>(this IDbContext db, ICriteria<TEntity> criteria) where TEntity : class
        {
            return db.Single<TEntity>(criteria, null);
        }
        /// <summary>
        /// 获得单个实体
        /// </summary>
        public static TEntity Single<TEntity>(this IDbContext db, IOrder<TEntity> order) where TEntity : class
        {
            return db.Single<TEntity>(null, order);
        }
        /// <summary>
        /// 获得单个实体
        /// </summary>
        public static TEntity Single<TEntity>(this IDbContext db, ICriteria<TEntity> criteria, IOrder<TEntity> order) where TEntity : class
        {
            return db.FindAll<TEntity>(1, criteria, order).FirstOrDefault();
        }
        /// <summary>
        /// 获得所有实体
        /// </summary>
        public static IEnumerable<TEntity> FindAll<TEntity>(this IDbContext db) where TEntity : class
        {
            return db.FindAll<TEntity>(null, null);
        }
        /// <summary>
        /// 获得符合条件的所有实体
        /// </summary>
        public static IEnumerable<TEntity> FindAll<TEntity>(this IDbContext db, IOrder<TEntity> order) where TEntity : class
        {
            return db.FindAll<TEntity>(null, order);
        }
        /// <summary>
        /// 获得符合条件的所有实体
        /// </summary>
        public static IEnumerable<TEntity> FindAll<TEntity>(this IDbContext db, ICriteria<TEntity> criteria) where TEntity : class
        {
            return db.FindAll<TEntity>(criteria, null);
        }
        /// <summary>
        /// 获得符合条件的所有实体
        /// </summary>
        public static IEnumerable<TEntity> FindAll<TEntity>(this IDbContext db, ICriteria<TEntity> criteria, IOrder<TEntity> order) where TEntity : class
        {
            return db.FindAll<TEntity>(-1, criteria, order);
        }
        /// <summary>
        /// 获得指定数量的所有实体
        /// </summary>
        public static IEnumerable<TEntity> FindAll<TEntity>(this IDbContext db, int limit) where TEntity : class
        {
            return db.FindAll<TEntity>(limit, null, null);
        }
        /// <summary>
        /// 获得符合条件的所有实体
        /// </summary>
        public static IEnumerable<TEntity> FindAll<TEntity>(this IDbContext db, int limit, IOrder<TEntity> order) where TEntity : class
        {
            return db.FindAll<TEntity>(limit, null, order);
        }
        /// <summary>
        /// 获得符合条件的所有实体
        /// </summary>
        public static IEnumerable<TEntity> FindAll<TEntity>(this IDbContext db, int limit, ICriteria<TEntity> criteria) where TEntity : class
        {
            return db.FindAll<TEntity>(limit, criteria, null);
        }
        /// <summary>
        /// 获得符合条件的所有实体
        /// </summary>
        public static IEnumerable<TEntity> FindAll<TEntity>(this IDbContext db, int limit, ICriteria<TEntity> criteria, IOrder<TEntity> order) where TEntity : class
        {
            return Query(db.CreateQuery<TEntity>(), criteria, order, -1, limit).Data;
        }
        /// <summary>
        /// 获得符合条件的所有实体
        /// </summary>
        public static PageResult<TEntity> FindAll<TEntity>(this IDbContext db, IOrder<TEntity> order, int pageIndex, int pageSize) where TEntity : class
        {
            return db.FindAll<TEntity>(null, order, pageIndex, pageSize);
        }
        /// <summary>
        /// 获得符合条件的所有实体
        /// </summary>
        public static PageResult<TEntity> FindAll<TEntity>(this IDbContext db, ICriteria<TEntity> criteria, IOrder<TEntity> order, int pageIndex, int pageSize) where TEntity : class
        {
            if (order == null || !order.OrderItems.Any()) {
                throw new InvalidOperationException("无效的排序。");
            }
            if (pageIndex < 0) {
                throw new ArgumentOutOfRangeException("pageIndex", "页索引数不能小于零");
            }
            if (pageSize <= 0) {
                throw new ArgumentOutOfRangeException("pageSize", "页显示数必须大于零");
            }


            return Query(db.CreateQuery<TEntity>(), criteria, order, pageIndex, pageSize);
        }

        private static PageResult<TEntity> Query<TEntity>(IQueryable<TEntity> query, ICriteria<TEntity> criteria, IOrder<TEntity> order, int pageIndex, int pageSize)
             where TEntity : class
        {
            //IQueryable<TEntity> query = db.CreateQuery<TEntity>();

            query = (criteria ?? Criteria<TEntity>.Empty).Filtered(query);

            int total = 0;
            if (pageIndex >= 0 && pageSize > 0) {
                total = query.Count();
            }

            query = (order ?? Order<TEntity>.Empty).Arranged(query);

            if (pageSize > 0) {
                if (pageIndex > 0) {
                    query = query.Skip(pageIndex * pageSize);
                }
                query = query.Take(pageSize);
            }


            IEnumerable<TEntity> result = query.ToList();

            if (pageSize <= 0)
                pageSize = 10;

            return new PageResult<TEntity>(total, pageSize, pageIndex, result);
        }
    }
}
