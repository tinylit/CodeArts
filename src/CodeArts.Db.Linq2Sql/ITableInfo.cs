using System;
using System.Collections.Generic;
#if NET40
using System.Collections.ObjectModel;
#endif


namespace CodeArts.Db
{
    /// <summary>
    /// 范围。
    /// </summary>
    public interface ITableInfo
    {
        /// <summary>
        /// 表实体类型。
        /// </summary>
        Type TableType { get; }

        /// <summary>
        /// 数据库表名称。
        /// </summary>
        string TableName { get; }

#if NET40
        /// <summary>
        /// 主键。
        /// </summary>
        ReadOnlyCollection<string> Keys { get; }

        /// <summary>
        /// 只读键。
        /// </summary>
        ReadOnlyCollection<string> ReadOnlys { get; }

        /// <summary>
        /// 令牌。
        /// </summary>
        IDictionary<string, TokenAttribute> Tokens { get; }

        /// <summary>
        /// 可读写的键。
        /// </summary>
        IDictionary<string, string> ReadWrites { get; }

        /// <summary>
        /// 属性和字段。
        /// </summary>
        IDictionary<string, string> ReadOrWrites { get; }
#else
        /// <summary>
        /// 主键。
        /// </summary>
        IReadOnlyCollection<string> Keys { get; }

        /// <summary>
        /// 只读键。
        /// </summary>
        IReadOnlyCollection<string> ReadOnlys { get; }

        /// <summary>
        /// 令牌。
        /// </summary>
        IReadOnlyDictionary<string, TokenAttribute> Tokens { get; }

        /// <summary>
        /// 可读写的键。
        /// </summary>
        IReadOnlyDictionary<string, string> ReadWrites { get; }

        /// <summary>
        /// 属性和字段。
        /// </summary>
        IReadOnlyDictionary<string, string> ReadOrWrites { get; }
#endif
    }
}
