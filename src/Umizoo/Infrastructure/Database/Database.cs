// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Umizoo.Infrastructure.Database.Contexts;

namespace Umizoo.Infrastructure.Database
{
    public abstract class Database : DisposableObject, IDatabase, IContext
    {
        #region Private Static Methods
        private static void CloseConnection(IDbConnection connection)
        {
            if (connection == null) {
                return;
            }

            if (connection.State != ConnectionState.Closed) {
                connection.Close();
            }
        }

        private static void DisposeTransaction(IDbTransaction transaction)
        {
            if (transaction == null) {
                return;
            }

            transaction.Dispose();
        }
        private static void DisposeConnection(IDbConnection connection)
        {
            CloseConnection(connection);
            connection.Dispose();
        }

        private static void AttachParameters(IDbCommand command, IEnumerable<DbParameter> commandParameters)
        {
            if (commandParameters != null) {
                foreach (var parameter in commandParameters) {
                    if (parameter != null) {
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && (parameter.Value == null)) {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
            }
        }
        #endregion


        #region Methods
        /// <summary>
        /// 为当前对象创建一个数据连接
        /// </summary>
        protected abstract DbConnection CreateConnection();
        private DbConnection CreateConnection(string connectionString)
        {
            DbConnection connection = null;
            try {
                connection = CreateConnection();
                connection.ConnectionString = connectionString;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                return connection;
            }
            catch (Exception ex) {
                CloseConnection(connection);
                throw ex;
            }
        }

        /// <summary>
        /// 创建数据库命令
        /// </summary>
        protected abstract DbCommand CreateCommand();
        private DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters)
        {
            var command = CreateCommand();
            command.CommandType = commandType;
            command.CommandText = commandText;
            command.Connection = connection;
            if (transaction != null) {
                command.Transaction = transaction;
            }

            if (commandParameters != null) {
                AttachParameters(command, commandParameters);
            }

            return command;
        }

        /// <summary>
        /// 创建一个DbCommand
        /// </summary>
        protected DbCommand CreateCommandByCommandType(CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters)
        {
            return CreateCommand(_connection, _transaction, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// 创建数据库命令参数
        /// </summary>
        protected abstract DbParameter CreateParameter();

        protected abstract DbDataAdapter CreateDataAdapter();


        private DbConnection _connection;
        /// <summary>
        /// 当前的数据源连接
        /// </summary>
        public DbConnection Connection
        {
            get { return this._connection; }
        }
        private DbTransaction _transaction;
        /// <summary>
        /// 当前的数据源事务
        /// </summary>
        public DbTransaction Transaction
        {
            get { return this._transaction; }
        }
        #endregion

        #region Ctor
        private readonly string _connectionString;
        private readonly bool _closeConnection = true;

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected Database(DbConnection connection)
        {
            Assertions.NotNull(connection, "connection");

            this._connection = connection;
            this._connectionString = connection.ConnectionString;
            this._closeConnection = false;
        }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected Database(string nameOrConnectionString)
        {
            Assertions.NotNullOrEmpty(nameOrConnectionString, "nameOrConnectionString");

            if (nameOrConnectionString.StartsWith("name=", StringComparison.CurrentCultureIgnoreCase)) {
                this._connectionString = ConfigurationManager.ConnectionStrings[nameOrConnectionString.Substring(nameOrConnectionString.IndexOf("=") + 1)].ConnectionString;
            }
            else if (nameOrConnectionString.IndexOf(';') != -1 && nameOrConnectionString.IndexOf('=') != -1) {
                this._connectionString = nameOrConnectionString;
            }
            else {
                this._connectionString = ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString;
            }

            this.Reconnect();
        }

        private readonly IContextManager _contextManager;
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected Database(string nameOrConnectionString, IContextManager contextManager)
            : this(nameOrConnectionString)
        {
            this._contextManager = contextManager;
        }
        #endregion


        /// <summary>
        /// 尝试重新连接数据库
        /// </summary>
        public void Reconnect()
        {
            if (!_closeConnection)
                return;


            if (_connection == null) {
                _connection = CreateConnection(_connectionString);
            }
            if (_connection.State != ConnectionState.Open) {
                _connection.Open();
            }
        }

        /// <summary>
        /// 断开数据库连接
        /// </summary>
        public void Disconnect()
        {
            DisposeTransaction(_transaction);
            _transaction = null;

            if (_closeConnection) {
                CloseConnection(_connection);
            }
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public DbTransaction BeginTransaction()
        {
            Reconnect();

            if (_transaction == null) {
                _transaction = _connection.BeginTransaction();
            }

            return _transaction;
        }

        /// <summary>
        /// 执行当前数据库连接对象的命令,指定参数.
        /// </summary>
        public int ExecuteNonQuery(CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters)
        {
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException("commandText");


            using (DbCommand command = CreateCommandByCommandType(commandType, commandText, commandParameters)) {
                try {
                    return command.ExecuteNonQuery();
                }
                finally {
                    command.Parameters.Clear();
                }
            }
        }

        /// <summary>
        /// 执行当前数据库连接对象的数据阅读器,指定参数.
        /// </summary>
        public DbDataReader ExecuteReader(CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters)
        {
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException("commandText");


            using (DbCommand command = CreateCommandByCommandType(commandType, commandText, commandParameters)) {
                try {
                    return command.ExecuteReader();
                }
                finally {
                    bool canClear = true;
                    foreach (IDataParameter parameter in command.Parameters) {
                        if (parameter.Direction != ParameterDirection.Input)
                            canClear = false;
                    }

                    if (canClear) {
                        command.Parameters.Clear();
                    }
                }
            }
        }
        /// <summary>
        /// 执行指定数据库连接对象的命令,指定参数,返回结果集中的第一行第一列.
        /// </summary>
        public object ExecuteScalar(CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText, commandParameters)) {
                try {
                    return command.ExecuteScalar();
                }
                finally {
                    command.Parameters.Clear();
                }
            }
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,指定参数值.返回DataSet.
        /// </summary>
        public DataSet ExecuteDataset(CommandType commandType, string commandText, IEnumerable<DbParameter> commandParameters)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText, commandParameters)) {
                using (DbDataAdapter da = CreateDataAdapter()) {
                    da.SelectCommand = command;
                    DataSet ds = new DataSet();

                    try {
                        da.Fill(ds);
                    }
                    finally {
                        command.Parameters.Clear();
                    }

                    return ds;
                }
            }
        }

        /// <summary>
        /// Command对象的传入参数
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="dbType">数据类型</param>
        /// <param name="size">数据长度</param>
        /// <param name="value">值</param>
        public DbParameter CreateInParameter(string paramName, DbType dbType, int size, object value)
        {
            return CreateParameter(paramName, dbType, ParameterDirection.Input, size, value);
        }

        /// <summary>
        /// Command对象的传出参数
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="dbType">数据类型</param>
        /// <param name="size">数据长度</param>
        public DbParameter CreateOutParameter(string paramName, DbType dbType, int size)
        {
            return CreateParameter(paramName, dbType, ParameterDirection.Output, size, null);
        }
        

        private DbParameter CreateParameter(string paramName, DbType dbType, ParameterDirection direction, int size, object value)
        {
            DbParameter param = CreateParameter();
            param.ParameterName = BuildParameterName(paramName);
            if((int)dbType > 0) {
                param.DbType = dbType;
            }            
            param.Direction = direction;
            if (size > 0)
                param.Size = size;
            if (!(direction == ParameterDirection.Output || value == DBNull.Value || value == null))
                param.Value = value;

            return param;
        }

        /// <summary>
        /// Builds a value parameter name for the current database.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>A correctly formated parameter name.</returns>
        protected virtual string BuildParameterName(string name)
        {
            return name;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                DisposeTransaction(_transaction);
                _transaction = null;
                if (_closeConnection) {
                    DisposeConnection(_connection);
                    _connection = null;
                }
            }
        }

        IContextManager IContext.ContextManager
        {
            get { return this._contextManager; }
        }
    }
}
