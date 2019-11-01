﻿using SkyBuilding.ORM.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SkyBuilding.ORM.SqlServer
{
    /// <summary>
    /// SqlServer矫正设置
    /// </summary>
    public class SqlServerCorrectSettings : ISQLCorrectSettings
    {
        private readonly static ConcurrentDictionary<string, Tuple<string, bool>> mapperCache = new ConcurrentDictionary<string, Tuple<string, bool>>();

        private readonly static Regex PatternColumn = new Regex(@"\bselect[\x20\t\r\n\f]+(?<column>((?!\b(select|where))[\s\S])+(select((?!\b(from|select)\b)[\s\S])+from((?!\b(from|select)\b)[\s\S])+)*((?!\b(from|select)\b)[\s\S])*)[\x20\t\r\n\f]+from[\x20\t\r\n\f]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly static Regex PatternOrderBy = new Regex(@"\border[\x20\t\r\n\f]+by[\x20\t\r\n\f]+[\s\S]+?$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.RightToLeft);

        private readonly static Regex PatternSingleAsColumn = new Regex(@"([\x20\t\r\n\f]+as[\x20\t\r\n\f]+)?(\[\w+\]\.)*(?<name>(\[\w+\]))[\x20\t\r\n\f]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.RightToLeft);

        public string Substring => "SUBSTRING";

        public string IndexOf => "CHARINDEX";

        public string Length => "LEN";

        public bool IndexOfSwapPlaces => true;

        private List<IVisitter> visitters;

        /// <summary>
        /// 格式化
        /// </summary>
        public IList<IVisitter> Visitters => visitters ?? (visitters = new List<IVisitter>());

        public DatabaseEngine Engine => DatabaseEngine.SqlServer;

        public string Name(string name) => string.Concat("[", name, "]");
        public string AsName(string name) => Name(name);
        public string TableName(string name) => Name(name);
        public string ParamterName(string name) => string.Concat("@", name);
        public virtual string PageSql(string sql, int take, int skip)
        {
            var sb = new StringBuilder();
            Match match;
            if (skip < 1)
            {
                match = PatternColumn.Match(sql);

                sql = sql.Substring(match.Length);

                return sb.Append(" SELECT TOP ")
                     .Append(take)
                     .Append(" ")
                     .Append(match.Groups["column"].Value)
                     .Append(" FROM ")
                     .Append(sql)
                     .ToString();
            }

            match = PatternOrderBy.Match(sql);

            if (!match.Success)
                throw new DException("使用Skip函数需要设置排序字段!");

            string orderBy = match.Value;

            sql = sql.Substring(0, sql.Length - match.Length);

            match = PatternColumn.Match(sql);

            if (!match.Success)
                throw new DException("使用Skip函数需要设置排序字段!");

            sql = sql.Substring(match.Length);

            string value = match.Groups["column"].Value;

            Tuple<string, bool> tuple = GetColumns(value);

            if (tuple.Item2)
            {
                value += " AS " + tuple.Item1;
            }

            string row_name = AsName("__Row_number_");

            return sb.Append("SELECT ")
                 .Append(tuple.Item1)
                 .Append(" FROM (")
                 .Append("SELECT ")
                 .Append(value)
                 .Append(", ROW_NUMBER() OVER(")
                 .Append(orderBy)
                 .Append(") AS ")
                 .Append(row_name)
                 .Append(" FROM ")
                 .Append(sql)
                 .Append(") ")
                 .Append(TableName("CTE"))
                 .Append(" WHERE ")
                 .Append(row_name)
                 .Append(" > ")
                 .Append(skip)
                 .Append(" AND ")
                 .Append(row_name)
                 .Append(" <= ")
                 .Append(skip + take)
                 .ToString();
        }
        private Tuple<string, bool> GetColumns(string columns) => mapperCache.GetOrAdd(columns, _ =>
        {
            var list = ToSingleColumnCodeBlock(columns);

            if (list.Count == 1)
            {
                var match = PatternSingleAsColumn.Match(list.First());

                if (match.Success)
                {
                    return Tuple.Create(match.Groups["name"].Value, false);
                }

                return Tuple.Create(AsName("__sql_server_col"), true);
            }

            return Tuple.Create(string.Join(",", list.ConvertAll(item =>
            {
                var match = PatternSingleAsColumn.Match(item);

                if (match.Success)
                {
                    return match.Groups["name"].Value;
                }

                throw new DException("分页且多字段时,必须指定字段名!");
            })), false);
        });

        /// <summary>
        /// 每列代码块（如:[x].[id],substring([x].[value],[x].[index],[x].[len]) as [total] => new List<string>{ "[x].[id]","substring([x].[value],[x].[index],[x].[len]) as [total]" }）
        /// </summary>
        /// <param name="columns">以“,”分割的列集合</param>
        /// <returns></returns>
        protected virtual List<string> ToSingleColumnCodeBlock(string columns) => CommonSettings.ToSingleColumnCodeBlock(columns);

        /// <summary>
        /// 组合查询语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public virtual string PageUnionSql(string sql, int take, int skip, string orderBy)
        {
            var sb = new StringBuilder();
            if (skip < 1)
            {
                return sb.Append("SELECT TOP ")
                     .Append(take)
                     .Append(" * FROM (")
                     .Append(sql)
                     .Append(") ")
                     .Append(TableName("CTE"))
                     .Append(" ")
                     .Append(orderBy)
                     .ToString();
            }

            if (string.IsNullOrEmpty(orderBy))
                throw new DException("使用Skip函数需要设置排序字段!");

            Match match = PatternColumn.Match(sql);

            string value = match.Groups["column"].Value;

            Tuple<string, bool> tuple = GetColumns(value);

            if (tuple.Item2)
            {
                throw new DException("组合查询必须指定字段名!");
            }

            string row_name = AsName("__Row_number_");

           return sb.Append("SELECT ")
                .Append(tuple.Item1)
                .Append(" FROM (")
                .Append("SELECT ")
                .Append(tuple.Item1)
                .Append(", ROW_NUMBER() OVER(")
                .Append(orderBy)
                .Append(") AS ")
                .Append(row_name)
                .Append(" FROM (")
                .Append(sql)
                .Append(") ")
                .Append(TableName("CTE_ROW_NUMBER"))
                .Append(") ")
                .Append(TableName("CTE"))
                .Append(" WHERE ")
                .Append(row_name)
                .Append(" > ")
                .Append(skip)
                .Append(" AND ")
                .Append(row_name)
                .Append(" <= ")
                .Append(skip + take)
                .ToString();
        }
    }
}
