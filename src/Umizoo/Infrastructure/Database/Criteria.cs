// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Linq;
using System.Linq.Expressions;


namespace Umizoo.Infrastructure.Database
{
    /// <summary>
    /// 查询表达式
    /// </summary>
    public class Criteria<T> : ICriteria<T> 
        where T : class
    {
        /// <summary>
        /// 表达式运算
        /// </summary>
        public static Criteria<T> Eval(Expression<Func<T, bool>> expression)
        {
            Assertions.NotNull(expression, "expression");

            return new Criteria<T>(expression);
        }

        /// <summary>
        /// 空的查询表达式
        /// </summary>
        public readonly static Criteria<T> Empty = new Criteria<T>();


        private Expression<Func<T, bool>> expressions = null;

        private Criteria()
        { }

        private Criteria(Expression<Func<T, bool>> expression)
        {
            this.expressions = expression;
        }

        /// <summary>
        /// And
        /// </summary>
        public Criteria<T> And(Expression<Func<T, bool>> expression)
        {
            Assertions.NotNull(expression, "expression");

            expressions = expressions.And(expression);

            return this;
        }
        /// <summary>
        /// And
        /// </summary>
        public Criteria<T> AndAlso(Expression<Func<T, bool>> expression)
        {
            Assertions.NotNull(expression, "expression");

            expressions = expressions.AndAlso(expression);

            return this;
        }
        /// <summary>
        /// 否定And
        /// </summary>
        public Criteria<T> AndNot(Expression<Func<T, bool>> expression)
        {
            Assertions.NotNull(expression, "expression");

            expressions = expressions.And(expression.Not());

            return this;
        }
        /// <summary>
        /// Or
        /// </summary>
        public Criteria<T> Or(Expression<Func<T, bool>> expression)
        {
            Assertions.NotNull(expression, "expression");

            expressions = expressions.Or(expression);

            return this;
        }

        /// <summary>
        /// 否定查询
        /// </summary>
        /// <returns></returns>
        public Criteria<T> Not()
        {
            if (expressions != null) {
                expressions = expressions.Not();
            }

            return this;
        }

        #region Criteria<T> 成员

        Expression<Func<T, bool>> ICriteria<T>.Expression
        {
            get { return expressions; }
        }

        IQueryable<T> ICriteria<T>.Filtered(IQueryable<T> enumerable)
        {
            if (expressions == null) 
                return enumerable;

            return enumerable.Where(expressions);
        }

        #endregion
    }
}
