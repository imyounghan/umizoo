// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 排序表达式实现
    /// </summary>
    public class Order<T> : IOrder<T>
        where T : class
    {
        readonly List<IOrderItem<T>> orders = new List<IOrderItem<T>>();

        /// <summary>
        /// 空的排序
        /// </summary>
        public readonly static Order<T> Empty = new Order<T>();

        private Order()
        { }

        private Order<T> Add(IOrderItem<T> order)
        {
            orders.Add(order);

            return this;
        }

        /// <summary>
        /// 升序
        /// </summary>
        public static Order<T> OrderBy(Expression<Func<T, dynamic>> expression)
        {
            Assertions.NotNull(expression, "expression");

            return new Order<T>().Add(new OrderItem(expression, SortOrder.Ascending));
        }
        /// <summary>
        /// 降序
        /// </summary>
        public static Order<T> OrderByDescending(Expression<Func<T, dynamic>> expression)
        {
            Assertions.NotNull(expression, "expression");

            return new Order<T>().Add(new OrderItem(expression, SortOrder.Descending));
        }

        /// <summary>
        /// 升序
        /// </summary>
        public Order<T> ThenBy(Expression<Func<T, dynamic>> expression)
        {
            Assertions.NotNull(expression, "expression");

            return this.Add(new OrderItem(expression, SortOrder.Ascending));
        }
        /// <summary>
        /// 降序
        /// </summary>
        public Order<T> ThenByDescending(Expression<Func<T, dynamic>> expression)
        {
            Assertions.NotNull(expression, "expression");

            return this.Add(new OrderItem(expression, SortOrder.Descending));
        }


        #region ISortSet<T> 成员

        IEnumerable<IOrderItem<T>> IOrder<T>.OrderItems
        {
            get { return orders; }
        }


        IQueryable<T> IOrder<T>.Arranged(IQueryable<T> source)
        {
            if (!orders.Any()) {
                return source;
            }

            string methodAsc = "OrderBy";
            string methodDesc = "OrderByDescending";
            Expression queryExpr = source.Expression;
            foreach (var sort in orders) {
                MemberExpression selector = (sort.Expression as LambdaExpression).Body.RemoveConvert() as MemberExpression;
                if (selector == null) {
                    throw new InvalidOperationException("不支持的排序类型。");
                }
                Type resultType = selector.Type;

                Expression exp = Expression.Quote(Expression.Lambda(selector, selector.Parameter()));
                if (resultType.IsValueType || resultType == typeof(string)) {
                    queryExpr = Expression.Call(
                    typeof(Queryable), sort.SortOrder == SortOrder.Ascending ? methodAsc : methodDesc,
                    new Type[] { source.ElementType, resultType },
                    queryExpr, exp);
                    methodAsc = "ThenBy";
                    methodDesc = "ThenByDescending";
                }
                else {
                    throw new InvalidOperationException(string.Format("不支持的排序类型：{0}", resultType.FullName));
                }

            }
            return source.Provider.CreateQuery<T>(queryExpr);
            //IOrderedQueryable<T> orderenumerable = null;

            //ISortItem<T> first = orders.First();
            //switch (first.SortOrder) {
            //    case SortOrder.Ascending:
            //        orderenumerable = enumerable.OrderBy(first.Expression);
            //        break;
            //    case SortOrder.Descending:
            //        orderenumerable = enumerable.OrderByDescending(first.Expression);
            //        break;
            //}


            //foreach (ISortItem<T> sort in orders.Skip(1)) {
            //    switch (sort.SortOrder) {
            //        case SortOrder.Ascending:
            //            orderenumerable = orderenumerable.ThenBy(sort.Expression);
            //            break;
            //        case SortOrder.Descending:
            //            orderenumerable = orderenumerable.ThenByDescending(sort.Expression);
            //            break;
            //    }
            //}

            //return orderenumerable;
        }

        #endregion


        internal class OrderItem : IOrderItem<T>
        {
            Expression<Func<T, dynamic>> sortPredicate = null;
            SortOrder sortOrder = SortOrder.Unspecified;
            public OrderItem(Expression<Func<T, dynamic>> sortPredicate, SortOrder sortOrder)
            {
                this.sortPredicate = sortPredicate;
                this.sortOrder = sortOrder;
            }


            Expression<Func<T, dynamic>> IOrderItem<T>.Expression
            {
                get { return sortPredicate; }
            }

            SortOrder IOrderItem<T>.SortOrder
            {
                get { return sortOrder; }
            }
        }
    }
}
