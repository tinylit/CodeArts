﻿using System;

namespace CodeArts.Db
{
    /// <summary>
    /// 只读连接。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    public class DbReadConfigAttribute : DbConfigAttribute
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public DbReadConfigAttribute() : base()
        {
        }
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="configName"></param>
        public DbReadConfigAttribute(string configName) : base(configName)
        {
        }
    }
}