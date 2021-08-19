using System;
using System.Linq.Expressions;
#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
#endif

namespace CodeArts.Db.Lts
{
    /// <summary>
    /// 动作能力。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    public interface IDbRouteExecuter<TEntity> where TEntity : class, IEntiy
    {
        /// <summary>
        /// 开启事务保护，使用数据库默认隔离级别。
        /// <see cref="System.Data.IDbConnection.BeginTransaction()"/>
        /// </summary>
        /// <returns></returns>
        IDbRouteExecuter<TEntity> UseTransaction();

        /// <summary>
        /// 开启事务保护，设置事务隔离级别。
        /// <see cref="System.Data.IDbConnection.BeginTransaction(System.Data.IsolationLevel)"/>
        /// </summary>
        /// <param name="isolationLevel">隔离级别。</param>
        /// <returns></returns>
        IDbRouteExecuter<TEntity> UseTransaction(System.Data.IsolationLevel isolationLevel);

        /// <summary>
        /// 执行指令。
        /// </summary>
        /// <param name="commandTimeout">超时时间，单位：秒。<see cref="System.Data.IDbCommand.CommandTimeout"/></param>
        /// <returns></returns>
        int ExecuteCommand(int? commandTimeout = null);

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER
        /// <summary>
        /// 执行指令。
        /// </summary>
        /// <param name="cancellationToken">取消。</param>
        /// <returns></returns>
        Task<int> ExecuteCommandAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行指令。
        /// </summary>
        /// <param name="commandTimeout">超时时间，单位：秒。<see cref="System.Data.IDbCommand.CommandTimeout"/></param>
        /// <param name="cancellationToken">取消。</param>
        /// <returns></returns>
        Task<int> ExecuteCommandAsync(int? commandTimeout, CancellationToken cancellationToken = default);
#endif

        /// <summary>
        /// 执行提供器。
        /// </summary>
        IDbRouter<TEntity> DbRouter { get; }
    }

    /// <summary>
    /// 删除能力。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    public interface IDeleteable<TEntity> : IDbRouteExecuter<TEntity> where TEntity : class, IEntiy
    {
        /// <summary>
        /// 数据源。
        /// </summary>
        /// <param name="table">表名称。</param>
        /// <returns></returns>
        IDeleteable<TEntity> From(Func<ITableInfo, string> table);

        /// <summary>
        /// 条件。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IDeleteable<TEntity> Where(string[] columns);

        /// <summary>
        /// 动作需要操作的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IDeleteable<TEntity> Where<TColumn>(Expression<Func<TEntity, TColumn>> columns);
    }

    /// <summary>
    /// 插入能力。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    public interface IInsertable<TEntity> : IDbRouteExecuter<TEntity> where TEntity : class, IEntiy
    {
        /// <summary>
        /// 数据源。
        /// </summary>
        /// <param name="table">表名称。</param>
        /// <returns></returns>
        IInsertable<TEntity> From(Func<ITableInfo, string> table);

        /// <summary>
        /// 只更新或只插入的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IInsertable<TEntity> Limit(string[] columns);

        /// <summary>
        /// 只更新或只插入的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IInsertable<TEntity> Limit<TColumn>(Expression<Func<TEntity, TColumn>> columns);

        /// <summary>
        /// 不更新或不插入的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IInsertable<TEntity> Except(string[] columns);

        /// <summary>
        /// 不更新或不插入的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IInsertable<TEntity> Except<TColumn>(Expression<Func<TEntity, TColumn>> columns);
    }

    /// <summary>
    /// 更新能力。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    public interface IUpdateable<TEntity> : IDbRouteExecuter<TEntity> where TEntity : class, IEntiy
    {
        /// <summary>
        /// 数据源。
        /// </summary>
        /// <param name="table">表名称。</param>
        /// <returns></returns>
        IUpdateable<TEntity> From(Func<ITableInfo, string> table);

        /// <summary>
        /// 数据源。
        /// </summary>
        /// <param name="table">表名称。</param>
        /// <returns></returns>
        IUpdateable<TEntity> From(Func<ITableInfo, TEntity, string> table);

        /// <summary>
        /// 只更新或只插入的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IUpdateable<TEntity> Limit(string[] columns);

        /// <summary>
        /// 只更新或只插入的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IUpdateable<TEntity> Limit<TColumn>(Expression<Func<TEntity, TColumn>> columns);

        /// <summary>
        /// 不更新或不插入的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IUpdateable<TEntity> Except(string[] columns);

        /// <summary>
        /// 不更新或不插入的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IUpdateable<TEntity> Except<TColumn>(Expression<Func<TEntity, TColumn>> columns);

        /// <summary>
        /// 条件。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IUpdateable<TEntity> Where(string[] columns);

        /// <summary>
        /// 动作需要操作的字段。
        /// </summary>
        /// <param name="columns">字段。</param>
        /// <returns></returns>
        IUpdateable<TEntity> Where<TColumn>(Expression<Func<TEntity, TColumn>> columns);
    }
}
