using CodeArts.Db.Exceptions;
using System;
using System.Collections.Generic;

namespace CodeArts.Db.Dapper
{
    /// <summary>
    /// 数据库链接管理器。
    /// </summary>
    public static class DapperConnectionManager
    {
        private static readonly Dictionary<string, IDbConnectionAdapter> Adapters;
        static DapperConnectionManager() => Adapters = new Dictionary<string, IDbConnectionAdapter>();

        /// <summary> 注册适配器。</summary>
        /// <param name="adapter">适配器。</param>
        public static void RegisterAdapter(IDbConnectionAdapter adapter)
        {
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (string.IsNullOrWhiteSpace(adapter.ProviderName))
            {
                throw new DException("数据库适配器名称不能为空!");
            }

            Adapters[adapter.ProviderName.ToLower()] = adapter;
        }

        /// <summary> 创建数据库适配器。</summary>
        /// <param name="providerName">供应商名称。</param>
        /// <returns></returns>
        public static IDbConnectionAdapter Get(string providerName)
        {
            if (providerName is null)
            {
                throw new ArgumentNullException(nameof(providerName));
            }

            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentException("数据库适配器名称不能为空", nameof(providerName));
            }

            if (Adapters.TryGetValue(providerName.ToLower(), out IDbConnectionAdapter adapter))
            {
                return adapter;
            }

            throw new DException($"不支持的适配器：{providerName}");
        }
    }
}