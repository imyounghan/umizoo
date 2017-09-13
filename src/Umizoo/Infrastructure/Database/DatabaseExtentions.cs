// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Umizoo.Infrastructure.Database
{
    public static class DatabaseExtentions
    {
        /// <summary>
        /// 执行当前数据库连接对象的命令,指定参数.
        /// </summary>
        /// <remarks>
        /// 示例: int result = ExecuteNonQuery(CommandType.StoredProcedure, "PublishOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="database">数据对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的DbParamter参数数组(无参数请写(DbParameter[])null)</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(this IDatabase database, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return database.ExecuteNonQuery(commandType, commandText, (IEnumerable<DbParameter>)commandParameters);
        }

        /// <summary>
        /// 执行当前数据库连接对象的数据阅读器,指定参数.
        /// </summary>
        /// <remarks>
        /// 示例: IDataReader dr = ExecuteReader(CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="database">数据对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">分配给命令的DbParamter参数数组(无参数请写(DbParameter[])null)</param>
        /// <returns>返回包含结果集的IDataReader</returns>
        public static DbDataReader ExecuteReader(this IDatabase database, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return database.ExecuteReader(commandType, commandText, (IEnumerable<DbParameter>)commandParameters);
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定参数,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 示例: int orderCount = (int)ExecuteScalar(CommandType.StoredProcedure, "GetOrderCount", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="database">数据对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的DbParamter参数数组(无参数请写(DbParameter[])null)</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(this IDatabase database, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return database.ExecuteScalar(commandType, commandText, (IEnumerable<DbParameter>)commandParameters);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,指定参数值.返回DataSet.
        /// </summary>
        /// <remarks>
        /// 示例: DataSet ds = ExecuteDataset(CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="database">数据对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的DbParamter参数数组(无参数请写(DbParameter[])null)</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(this IDatabase database, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return database.ExecuteDataset(commandType, commandText, (IEnumerable<DbParameter>)commandParameters);
        }


        /// <summary>
        /// Command对象的传入参数
        /// </summary>
        /// <param name="database">数据对象</param>
        /// <param name="paramName">参数名称</param>
        /// <param name="value">值</param>
        public static DbParameter CreateInParameter(this IDatabase database, string paramName, object value)
        {
            return database.CreateInParameter(paramName, (DbType)(-1), -1, value);
        }

        /// <summary>
        /// Command对象的传入参数
        /// </summary>
        /// <param name="database">数据对象</param>
        /// <param name="paramName">参数名称</param>
        /// <param name="dbType">数据类型</param>
        /// <param name="value">值</param>
        public static DbParameter CreateInParameter(this IDatabase database, string paramName, DbType dbType, object value)
        {
            return database.CreateInParameter(paramName, dbType, -1, value);
        }

        /// <summary>
        /// Command对象的传出参数
        /// </summary>
        /// <param name="database">数据对象</param>
        /// <param name="paramName">参数名称</param>
        public static DbParameter CreateOutParameter(this IDatabase database, string paramName)
        {
            return database.CreateOutParameter(paramName, (DbType)(-1), -1);
        }

        /// <summary>
        /// Command对象的传出参数
        /// </summary>
        /// <param name="database">数据对象</param>
        /// <param name="paramName">参数名称</param>
        /// <param name="dbType">数据类型</param>
        public static DbParameter CreateOutParameter(this IDatabase database, string paramName, DbType dbType)
        {
            return database.CreateOutParameter(paramName, dbType, -1);
        }
    }
}
