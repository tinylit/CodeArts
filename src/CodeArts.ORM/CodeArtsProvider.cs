﻿using CodeArts.ORM.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace CodeArts.ORM
{
    /// <summary>
    /// 代码艺术
    /// </summary>
    public class CodeArtsProvider : RepositoryProvider
    {
        private readonly ISQLCorrectSettings settings;
        private static readonly Dictionary<Type, DbType> typeMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="settings">SQL矫正配置</param>
        public CodeArtsProvider(ISQLCorrectSettings settings) : base(settings)
        {
            this.settings = settings;
        }

        static CodeArtsProvider()
        {
            typeMap = new Dictionary<Type, DbType>
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = DbType.DateTime,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = DbType.Time,
                [typeof(byte[])] = DbType.Binary,
                [typeof(object)] = DbType.Object
            };
        }

        private static DbType LookupDbType(Type dataType)
        {
            if (dataType.IsEnum)
            {
                dataType = Enum.GetUnderlyingType(dataType);
            }
            else if (dataType.IsNullable())
            {
                dataType = Nullable.GetUnderlyingType(dataType);
            }

            if (typeMap.TryGetValue(dataType, out DbType dbType))
                return dbType;

            if (dataType.FullName == "System.Data.Linq.Binary")
            {
                return DbType.Binary;
            }

            return DbType.Object;
        }

        private void AddParameterAuto(IDbCommand command, Dictionary<string, object> parameters)
        {
            if (parameters is null || parameters.Count == 0)
                return;

            foreach (var kv in parameters)
            {
                AddParameterAuto(command, kv.Key, kv.Value);
            }
        }

        private void AddParameterAuto(IDbCommand command, string key, object value)
        {
            if (key[0] == '@' || key[0] == '?' || key[0] == ':')
            {
                key = key.Substring(1);
            }

            var dbParameter = command.CreateParameter();

            dbParameter.Value = value ?? DBNull.Value;
            dbParameter.ParameterName = settings.ParamterName(key);
            dbParameter.Direction = ParameterDirection.Input;
            dbParameter.DbType = value == null ? DbType.Object : LookupDbType(value.GetType());

            command.Parameters.Add(dbParameter);
        }

        /// <summary>
        /// 执行SQL。
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public override int Execute(IDbConnection conn, string sql, Dictionary<string, object> parameters = null)
        {
            OpenConnection(conn);

            using (var command = conn.CreateCommand())
            {
                command.CommandText = sql;

                AddParameterAuto(command, parameters);

                return command.ExecuteNonQuery();
            }
        }

        private static void OpenConnection(IDbConnection conn)
        {
            switch (conn.State)
            {
                case ConnectionState.Closed:
                    conn.Open();
                    break;
                case ConnectionState.Connecting:
                    do
                    {
                        Thread.Sleep(5);

                    } while (conn.State == ConnectionState.Connecting);
                    break;
                case ConnectionState.Broken:
                    conn.Close();
                    conn.Open();
                    break;
            }
        }

        /// <summary>
        /// 查询。
        /// </summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="conn">数据库连接</param>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public override IEnumerable<T> Query<T>(IDbConnection conn, string sql, Dictionary<string, object> parameters = null)
        {
            OpenConnection(conn);

            CommandBehavior behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult;

            if (settings.Engine == DatabaseEngine.SQLite)
            {
                behavior &= ~CommandBehavior.SingleResult;
            }

            using (var command = conn.CreateCommand())
            {
                command.CommandText = sql;

                AddParameterAuto(command, parameters);

                using (var dr = command.ExecuteReader(behavior))
                {
                    while (dr.Read())
                    {
                        yield return dr.MapTo<T>();
                    }

                    while (dr.NextResult()) { /* ignore subsequent result sets */ }
                }
            }
        }

        /// <summary>
        /// 查询。
        /// </summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="conn">数据库连接</param>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="reqiured">是否必须。</param>
        /// <param name="defaultValue">默认</param>
        /// <exception cref="DRequiredException">必须且数据库未查询到数据</exception>
        /// <returns></returns>
        public override T QueryFirst<T>(IDbConnection conn, string sql, Dictionary<string, object> parameters = null, bool reqiured = false, T defaultValue = default)
        {
            OpenConnection(conn);

            CommandBehavior behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;

            if (settings.Engine == DatabaseEngine.SQLite)
            {
                behavior &= ~CommandBehavior.SingleResult;
            }

            using (var command = conn.CreateCommand())
            {
                command.CommandText = sql;

                AddParameterAuto(command, parameters);

                using (var dr = command.ExecuteReader(behavior))
                {
                    if (dr.Read())
                    {
                        defaultValue = dr.MapTo<T>();

                        while (dr.Read()) { /* ignore subsequent rows */ }
                    }
                    else if (reqiured)
                    {
                        throw new DRequiredException();
                    }

                    while (dr.NextResult()) { /* ignore subsequent result sets */ }
                }
            }

            return defaultValue;
        }
    }
}