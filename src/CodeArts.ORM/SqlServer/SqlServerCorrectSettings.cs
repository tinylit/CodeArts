﻿using CodeArts.ORM.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeArts.ORM.SqlServer
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
        /// <summary>
        /// 字符串截取。 SUBSTRING
        /// </summary>
        public string Substring => "SUBSTRING";

        /// <summary>
        /// 字符串索引。 CHARINDEX
        /// </summary>
        public string IndexOf => "CHARINDEX";

        /// <summary>
        /// 字符串长度。 LEN
        /// </summary>
        public string Length => "LEN";

        /// <summary>
        /// 索引位置对调。 true.
        /// </summary>
        public bool IndexOfSwapPlaces => true;

        private List<IVisitter> visitters;

        /// <summary>
        /// 格式化
        /// </summary>
        public IList<IVisitter> Visitters => visitters ?? (visitters = new List<IVisitter>());

        /// <summary>
        /// SqlServer
        /// </summary>
        public DatabaseEngine Engine => DatabaseEngine.SqlServer;


        private ICollection<IFormatter> formatters;
        /// <summary>
        /// 格式化集合
        /// </summary>
        public ICollection<IFormatter> Formatters => formatters ?? (formatters = new List<IFormatter>());

        /// <summary>
        /// 字段
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public string Name(string name) => string.Concat("[", name, "]");
        /// <summary>
        /// 参数名称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public string ParamterName(string name) => string.Concat("@", name);

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

                return Tuple.Create(Name("__sql_server_col"), true);
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
        /// 每列代码块（如:[x].[id],substring([x].[value],[x].[index],[x].[len]) as [total] => new List&lt;string&gt;{ "[x].[id]","substring([x].[value],[x].[index],[x].[len]) as [total]" }）
        /// </summary>
        /// <param name="columns">以“,”分割的列集合</param>
        /// <returns></returns>
        protected virtual List<string> ToSingleColumnCodeBlock(string columns) => CommonSettings.ToSingleColumnCodeBlock(columns);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="take">获取N行。</param>
        /// <param name="skip">跳过M行</param>
        /// <returns></returns>
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

            string row_name = Name("__Row_number_");

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
                 .Append(Name("CTE"))
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
        /// <summary>
        /// 分页（交集、并集等）
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="take">获取N行。</param>
        /// <param name="skip">跳过M行</param>
        /// <param name="orderBy">排序</param>
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
                     .Append(Name("CTE"))
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

            string row_name = Name("__Row_number_");

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
                .Append(Name("CTE_ROW_NUMBER"))
                .Append(") ")
                .Append(Name("CTE"))
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