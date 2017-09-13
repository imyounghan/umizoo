// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Umizoo.Infrastructure.Database
{
    public interface IDatabase : IDisposable
    {
        /// <summary>
        /// 重新连接数据库
        /// </summary>
        void Reconnect();
        /// <summary>
        /// 断开数据库连接
        /// </summary>
        void Disconnect();
        /// <summary>
        /// 开始事务
        /// </summary>
        DbTransaction BeginTransaction();

        /// <summary>
        /// 获取当前的数据库连接
        /// </summary>
        DbConnection Connection { get; }
        /// <summary>
        /// 获取当前执行的事务
        /// </summary>
        DbTransaction Transaction { get; }


        /// <summary>
        /// 执行当前数据库连接对象的命令,指定参数.
        /// </summary>
        int ExecuteNonQuery(CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters);

        /// <summary>
        /// 执行当前数据库连接对象的数据阅读器,指定参数.
        /// </summary>
        DbDataReader ExecuteReader(CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters);

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定参数,返回结果集中的第一行第一列.
        /// </summary>
        object ExecuteScalar(CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters);

        /// <summary>
        /// 执行指定数据库连接字符串的命令,指定参数值.返回DataSet.
        /// </summary>
        DataSet ExecuteDataset(CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters);

        ///// <summary>
        ///// Command对象的传入参数
        ///// </summary>
        ///// <param name="paramName">参数名称</param>
        ///// <param name="value">值</param>
        //DbParameter CreateInParameter(string paramName, object value);
        ///// <summary>
        ///// Command对象的传入参数
        ///// </summary>
        ///// <param name="paramName">参数名称</param>
        ///// <param name="dbType">数据类型</param>
        ///// <param name="value">值</param>
        //DbParameter CreateInParameter(string paramName, DbType dbType, object value);
        /// <summary>
        /// Command对象的传入参数
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="dbType">数据类型</param>
        /// <param name="size">数据长度</param>
        /// <param name="value">值</param>
        DbParameter CreateInParameter(string paramName, DbType dbType, int size, object value);

        /// <summary>
        /// Command对象的传出参数
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="dbType">数据类型</param>
        /// <param name="size">数据长度</param>
        DbParameter CreateOutParameter(string paramName, DbType dbType, int size);
        ///// <summary>
        ///// Command对象的传出参数
        ///// </summary>
        ///// <param name="paramName">参数名称</param>
        ///// <param name="dbType">数据类型</param>
        //DbParameter CreateOutParameter(string paramName, DbType dbType);
        ///// <summary>
        ///// Command对象的传出参数
        ///// </summary>
        ///// <param name="paramName">参数名称</param>
        //DbParameter CreateOutParameter(string paramName);
    }
}
