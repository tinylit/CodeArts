using Dapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CodeArts.Db.Dapper
{
    /// <summary>
    /// 数据上下文。
    /// </summary>
    public class DbContext : IDisposable
    {
        private readonly IReadOnlyConnectionConfig connectionConfig;
        private readonly List<IDbConnection> connections = new List<IDbConnection>();
        private static readonly ConcurrentDictionary<Type, DbConfigAttribute> DbConfigCache = new ConcurrentDictionary<Type, DbConfigAttribute>();

        /// <summary>
        /// inheritdoc
        /// </summary>
        public DbContext()
        {
            connectionConfig = GetDbConfig();
        }

        /// <summary>
        /// inheritdoc
        /// </summary>
        public DbContext(IReadOnlyConnectionConfig connectionConfig)
        {
            this.connectionConfig = connectionConfig ?? throw new ArgumentNullException(nameof(connectionConfig));
        }

        /// <summary>
        /// 获取数据库配置。
        /// </summary>
        /// <returns></returns>
        protected virtual IReadOnlyConnectionConfig GetDbConfig()
        {
            var attr = DbConfigCache.GetOrAdd(GetType(), type =>
            {
                return (DbConfigAttribute)Attribute.GetCustomAttribute(type, typeof(DbConfigAttribute), true);
            });

            return attr.GetConfig();
        }

        /// <summary> 连接名称。 </summary>
        public string Name => connectionConfig.Name;

        /// <summary> 数据库驱动名称。 </summary>
        public string ProviderName => connectionConfig.ProviderName;

        /// <summary>
        /// SQL 矫正。
        /// </summary>
        public ISQLCorrectSettings Settings => DbAdapter.Settings;

        private IDbConnectionAdapter connectionAdapter;
        private bool disposedValue;

        /// <summary>
        /// 适配器。
        /// </summary>
        protected IDbConnectionAdapter DbAdapter
        {
            get
            {
                if (connectionAdapter is null || !string.Equals(connectionAdapter.ProviderName, connectionConfig.ProviderName))
                {
                    connectionAdapter = CreateDbAdapter(connectionConfig.ProviderName);
                }

                return connectionAdapter;
            }
        }

        /// <summary>
        /// 创建适配器。
        /// </summary>
        /// <returns></returns>
        protected virtual IDbConnectionAdapter CreateDbAdapter(string providerName) => DapperConnectionManager.Get(providerName);

        /// <summary>
        /// 创建数据库查询器。
        /// </summary>
        /// <param name="useCache">优先复用链接池，否则：始终创建新链接。</param>
        /// <returns></returns>
        public virtual IDbConnection CreateDb(bool useCache = true)
        {
            var connection = TransactionConnections.GetConnection(connectionConfig.ConnectionString, DbAdapter) ?? DispatchConnections.Instance.GetConnection(connectionConfig.ConnectionString, DbAdapter, useCache);

            connections.Add(connection);

            return connection;
        }

        /// <summary>
        /// 转分页SQL（countSql;listSql）。
        /// </summary>
        /// <param name="sql">语句。</param>
        /// <param name="pageIndex">页面索引。</param>
        /// <param name="pageSize">分页条数。</param>
        /// <returns></returns>
        protected virtual string ToPagedSql(SQL sql, int pageIndex, int pageSize)
        {
            var value = sql.ToCountSQL() + sql.ToSQL(pageIndex, pageSize);

            return value.ToString(Settings);
        }

        /// <summary>
        /// 查询分页数据。
        /// </summary>
        /// <typeparam name="T">集合元素类型。</typeparam>
        /// <param name="sql">查询语句。</param>
        /// <param name="pageIndex">页面，索引从“0”开始。</param>
        /// <param name="pageSize">一页显示多少条。</param>
        /// <param name="param">参数。</param>
        /// <param name="commandTimeout">超时时间。</param>
        /// <returns></returns>
        public PagedList<T> Query<T>(SQL sql, int pageIndex, int pageSize, object param = null, int? commandTimeout = null)
        {
            var sqlStr = ToPagedSql(sql, pageIndex, pageSize);

            using (IDbConnection connection = CreateDb())
            {
                using (var reader = connection.QueryMultiple(sqlStr, param, commandTimeout: commandTimeout))
                {
                    int count = reader.ReadFirst<int>();

                    var results = reader.Read<T>();

                    return new PagedList<T>(results as List<T> ?? results.ToList(), pageIndex, pageSize, count);
                }
            }
        }

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER

        /// <summary>
        /// 查询分页数据。
        /// </summary>
        /// <typeparam name="T">集合元素类型。</typeparam>
        /// <param name="sql">查询语句。</param>
        /// <param name="pageIndex">页面，索引从“0”开始。</param>
        /// <param name="pageSize">一页显示多少条。</param>
        /// <param name="param">参数。</param>
        /// <param name="commandTimeout">超时时间。</param>
        /// <returns></returns>
        public async Task<PagedList<T>> QueryAsync<T>(SQL sql, int pageIndex, int pageSize, object param = null, int? commandTimeout = null)
        {
            var sqlStr = ToPagedSql(sql, pageIndex, pageSize);

            using (IDbConnection connection = CreateDb())
            {
                using (var reader = await connection.QueryMultipleAsync(sqlStr, param, commandTimeout: commandTimeout).ConfigureAwait(false))
                {
                    int count = await reader.ReadFirstAsync<int>().ConfigureAwait(false);

                    var results = await reader.ReadAsync<T>().ConfigureAwait(false);

                    return new PagedList<T>(results as List<T> ?? results.ToList(), pageIndex, pageSize, count);
                }
            }
        }
#endif

        /// <summary>
        /// 释放。
        /// </summary>
        /// <param name="disposing">释放。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)

                    foreach (var connection in connections)
                    {
                        connection.Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }


        /// <summary>
        /// // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        /// ~DbContext()
        /// {
        ///     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        ///     Dispose(disposing: false);
        /// }
        /// </summary>
        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }
    }
}
