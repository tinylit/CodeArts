﻿using CodeArts.Db.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeArts.Db
{
    /// <summary>
    /// 默认SQL解析器。
    /// </summary>
    public class DefaultSqlAdpter : ISqlAdpter
    {
        /// <summary>
        /// 注解。
        /// </summary>
        private static readonly Regex PatternAnnotate = new Regex(@"--.*|/\*[\s\S]*?\*/", RegexOptions.Compiled);

        /// <summary>
        /// 提取字符串。
        /// </summary>
        private static readonly Regex PatternCharacter = new Regex("(['\"])(?:\\\\.|[^\\\\])*?\\1", RegexOptions.Compiled);

        /// <summary>
        /// 空白符。
        /// </summary>
        private static readonly Regex PatternWhitespace = new Regex(@"^[\x20\t\r\n\f]+|(?<=[\r\n]{2})[\r\n]+|(?<=[\x20\t\f])[\x20\t\f]+|(?<=\()[\x20\t\r\n\f]+|[\x20\t\r\n\f]+(?=\))|[\x20\t\r\n\f]+$", RegexOptions.Compiled);

        /// <summary>
        /// 移除所有前导空白字符和尾部空白字符函数修复。
        /// </summary>
        private static readonly Regex PatternTrim = new Regex(@"\b(?<name>(trim))\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 修改命令。
        /// </summary>
        private static readonly Regex PatternAlter = new Regex(@"\balter[\x20\t\r\n\f]+(table|view)[\x20\t\r\n\f]+(\w+\.)*(?<name>\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 插入命令。
        /// </summary>
        private static readonly Regex PatternInsert = new Regex(@"\binsert[\x20\t\r\n\f]+into[\x20\t\r\n\f]+(\w+\.)*(?<name>\w+)(?=[\x20\t\r\n\f]*\()", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 修改命令。
        /// </summary>
        private static readonly Regex PatternChange = new Regex(@"\b((?<type>delete)[\x20\t\r\n\f]+from|(?<type>update))[\x20\t\r\n\f]+([\w\[\]]+\.)*(?<table>(?<name>\w+)|\[(?<name>\w+)\])[\x20\t\r\n\f]+set[\x20\t\r\n\f]+((?!\b(select|insert|update|delete|create|drop|alter|truncate|use|set|declare|exec|execute|sp_executesql)\b)[^;])+;?", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// 复杂修改命令。
        /// </summary>
        private static readonly Regex PatternChangeComplex = new Regex(@"\b((?<type>delete)[\x20\t\r\n\f]+(\[\w+\]|(?<alias>\w+))|(?<type>update)[\x20\t\r\n\f]+(\[\w+\]|(?<alias>\w+))[\x20\t\r\n\f]+set([\x20\t\r\n\f]+[\w\.\[\]]+[\x20\t\r\n\f]*=[\x20\t\r\n\f]*[?:@]?[\w\.\[\]]+,[\x20\t\r\n\f]*)*[\x20\t\r\n\f]+[\w\.\[\]]+[\x20\t\r\n\f]*=[\x20\t\r\n\f]*[?:@]?[\w\.\[\]]+)[\x20\t\r\n\f]+from[\x20\t\r\n\f]+([\w\[\]]+\.)*(?<table>(?<name>\w+)|\[(?<name>\w+)\])((?!\b(insert|update|delete|create|drop|alter|truncate|use|set|declare|exec|execute|sp_executesql)\b)[^;])+;?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 创建表命令。
        /// </summary>
        private static readonly Regex PatternCreate = new Regex(@"\bcreate[\x20\t\r\n\f]+(table|view)[\x20\t\r\n\f]+(?<if>if[\x20\t\r\n\f]+not[\x20\t\r\n\f]+exists[\x20\t\r\n\f]+)?([\w\[\]]+\.)*(?<table>(?<name>\w+)|\[(?<name>\w+)\])[\x20\t\r\n\f]*\(((?!\b(select|insert|update|delete|create|drop|alter|truncate|use)\b)(on[\x20\t\r\n\f]+(update|delete)|[^;]))+;?", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// 删除表命令。
        /// </summary>
        private static readonly Regex PatternDrop = new Regex(@"\bdrop[\x20\t\r\n\f]+(table|view)[\x20\t\r\n\f]+(?<if>if[\x20\t\r\n\f]+exists[\x20\t\r\n\f]+)?([\w\[\]]+\.)*(?<table>(?<name>\w+)|\[(?<name>\w+)\])[\x20\t\r\n\f]*;?", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// 表命令。
        /// </summary>
        private static readonly Regex PatternForm = new Regex(@"\b(from|join)[\x20\t\r\n\f]+([\w+\[\]]\.)*(?<table>(?<name>\w+)|\[(?<name>\w+)\])([\x20\t\r\n\f]+(as[\x20\t\r\n\f]+(?<alias>\w+)|(?<alias>(?!\b(where|on|join|group|order|select|into|limit|(left|right|inner|outer)[\x20\t\r\n\f]+join)\b)\w+)))?(?<follow>((?!\b(where|on|join|order|group|having|select|insert|update|delete|create|drop|alter|truncate|use|set|(left|right|inner|outer|full)[\x20\t\r\n\f]+join)\b)[^;])+)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 连表命令。
        /// </summary>
        private static readonly Regex PatternFormFollow = new Regex(@",[\x20\t\r\n\f]*(?<table>(?<name>\w+)|\[(?<name>\w+)\])([\x20\t\r\n\f]+(as[\x20\t\r\n\f]+)?(?<alias>(?!\bas\b)\w+))?(?=[\x20\t\r\n\f]*(,|$))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 参数。
        /// </summary>
        private static readonly Regex PatternParameter = new Regex(@"(?<![\p{L}\p{N}@_])[?:@](?<name>[\p{L}\p{N}_][\p{L}\p{N}@_]*)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// 字段名称。
        /// </summary>
        private static readonly Regex PatternField;

        /// <summary>
        /// 字段名称补充。
        /// </summary>
        private static readonly Regex PatternFieldEmbody = new Regex(@"\[\w+\][\x20\t\r\n\f]*,[\x20\t\r\n\f]*(?<name>[_a-zA-Z]\w*)(?=[\x20\t\r\n\f]+[^\x20\t\r\n\f\(]|[^\x20\t\r\n\f\w\.\]\}\(]|[\x20\t\r\n\f]*$)", RegexOptions.Compiled);

        /// <summary>
        /// 别名字段。
        /// </summary>
        private static readonly Regex PatternAliasField = new Regex(@"(?<alias>[\w-]+)\.(?<name>(?!\d+)[\w-]+)(?=[^\w\.\]\}\(]|$)", RegexOptions.Compiled);

        /// <summary>
        /// 独立参数字段。
        /// </summary>
        private static readonly Regex PatternSingleArgField = new Regex(@"(?<!with[\x20\t\r\n\f]*)\([\x20\t\r\n\f]*(?<name>(?!\d+)(?![_A-Z]+)(?!\b(max|min)\b)\w+)[\x20\t\r\n\f]*\)", RegexOptions.Compiled);

        /// <summary>
        /// 字段名称（创建别命令中）。
        /// </summary>
        private static readonly Regex PatternFieldCreate;

        /// <summary>
        /// 字段别名。
        /// </summary>
        private static readonly Regex PatternAsField = new Regex(@"\[\w+\][\x20\t\r\n\f]+as[\x20\t\r\n\f]+(?<name>[\p{L}\p{N}@_]+)|\bas[\x20\t\r\n\f]+(?<name>(?!\bselect\b)[\p{L}\p{N}@_]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// 查询语句所有字段。
        /// </summary>
        private static readonly Regex PatternColumn = new Regex(@"\bselect[\x20\t\r\n\f]+(?<distinct>distinct[\x20\t\r\n\f]+)?(?<cols>((?!\b(select|where)\b)[\s\S])+(select((?!\b(from|select)\b)[\s\S])+from((?!\b(from|select)\b)[\s\S])+)*((?!\b(from|select)\b)[\s\S])*)[\x20\t\r\n\f]+from[\x20\t\r\n\f]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// 查询语句排序内容。
        /// </summary>
        private static readonly Regex PatternOrderBy = new Regex(@"[\x20\t\r\n\f]+order[\x20\t\r\n\f]+by[\x20\t\r\n\f]+((?!\b(select|where)\b)[\s\S])+(select((?!\b(from|select)\b)[\s\S])+from((?!\b(from|select)\b)[\s\S])+)*((?!\b(from|select)\b)[\s\S])*$", RegexOptions.IgnoreCase | RegexOptions.RightToLeft | RegexOptions.Compiled);

        /// <summary>
        /// 分页。
        /// </summary>
        private static readonly Regex PatternPaging = new Regex("PAGING\\(`(?<main>(?:\\\\.|[^\\\\])*?)`,(?<index>\\d+),(?<size>\\d+)(,`(?<orderby>(?:\\\\.|[^\\\\])*?)`)?\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 查询别名。
        /// </summary>
        private static readonly Regex PatternWithAs = new Regex(@"\bwith[\x20\t\r\n\f]+(?<name>[^\x20\t\r\n\f]+)[\x20\t\r\n\f]+as[\x20\t\r\n\f]*\((?<sql>.+?)\)([\x20\t\r\n\f]*,[\x20\t\r\n\f]*(?<name>[^\x20\t\r\n\f]+)[\x20\t\r\n\f]+as[\x20\t\r\n\f]*\((?<sql>.+?)\))*[\x20\t\r\n\f]*(?=select|insert|update|delete[\x20\t\r\n\f]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// UNION/UNION ALL/EXCEPT/INTERSECT
        /// </summary>
        private static readonly Regex PatternJoint = new Regex(@"\b(union([\x20\t\r\n\f]+all)|except|intersect)[\x20\t\r\n\f]+select$", RegexOptions.IgnoreCase | RegexOptions.RightToLeft | RegexOptions.Compiled);

        #region UseSettings

        /// <summary>
        /// 信任的函数。
        /// </summary>
        private static readonly Regex PatternConvincedMethod = new Regex(@"\b(?<name>(len|length|substr|substring|indexof))(?=\()", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 字符串截取函数修复。
        /// </summary>
        private static readonly Regex PatternIndexOf = new Regex(@"\bindexOf\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 名称令牌。
        /// </summary>
        private static readonly Regex PatternNameToken = new Regex(@"\[(?<name>\w+)\]", RegexOptions.Compiled);

        /// <summary>
        /// 参数令牌。
        /// </summary>
        private static readonly Regex PatternParameterToken = new Regex(@"\{(?<name>[\p{L}\p{N}@_]+)\}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// 聚合函数。
        /// </summary>
        private static readonly Regex PatternAggregationFn = new Regex(@"\b(sum|max|min|avg|count)[\x20\t\r\n\f]*\(", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        #endregion

        private static readonly ConcurrentDictionary<string, string> SqlCache = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, string> SqlCountCache = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, string> SqlPagedCache = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, List<string>> CharacterCache = new ConcurrentDictionary<string, List<string>>();
        private static readonly ConcurrentDictionary<string, IReadOnlyList<TableToken>> TableCache = new ConcurrentDictionary<string, IReadOnlyList<TableToken>>();

        /// <summary>
        /// 静态构造函数。
        /// </summary>
        static DefaultSqlAdpter()
        {
            var whitespace = @"[\x20\t\r\n\f]";
            var check = @"(?=[\x20\t\r\n\f]+[^\x20\t\r\n\f\(]|[^\x20\t\r\n\f\w\.\]\}\(]|[\x20\t\r\n\f]*$)";

            var sb = new StringBuilder()
                .Append(",")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\d+)\w+)")
                    .Append(whitespace)
                    .Append(@"*(?=[=,])") //? ,[name], ,[name]=[value]
                .Append(@"|\bcolumn")
                    .Append(whitespace)
                    .Append(@"+(?<name>(?!\d+)\w+)") //? 字段 column [name]
                .Append(@"|\bafter")
                    .Append(whitespace)
                    .Append(@"+(?<name>(?!\d+)\w+)")
                    .Append(whitespace)
                    .Append("*(?=(;|$))") //? after [name] --mysql alter table `yep_developers` add column `level` int default 0 after `status`;
                .Append(@"|\bupdate")
                    .Append(whitespace)
                    .Append(@"+.+?")
                    .Append(whitespace)
                    .Append("+set")
                    .Append(whitespace)
                    .Append(@"+(?<name>(?!\d+)(?!\bwhen\b)\w+)")
                    .Append(check) //? 更新语句SET字段
                .Append(@"|\bcase").Append(whitespace)
                    .Append(@"+(?<name>(?!\d+)(?!\bwhen\b)\w+)")
                    .Append(check) //? case 函数
                .Append(@"|\bwhen")
                    .Append(whitespace)
                    .Append(@"+(")
                        .Append("(not")
                            .Append(whitespace)
                            .Append("+)?exists")
                            .Append(whitespace)
                            .Append(@"*\(")
                            .Append(whitespace)
                            .Append("*select")
                            .Append(whitespace)
                            .Append("+(distinct")
                            .Append(whitespace)
                            .Append("+)?")
                            .Append("(top")
                                .Append(whitespace)
                                .Append(@"+\d+")
                                .Append(whitespace)
                                .Append("+")
                            .Append(")?")
                        .Append(")?")
                    .Append(@"(?<name>(?!\d+)(?!\b(not|then|top|distinct)\b)\w+)")
                    .Append(check) //? when 函数
                .Append(@"|\b(then|else)")
                    .Append(whitespace)
                    .Append(@"+(?<name>(?!\d+)(?!\bnull\b)\w+)")
                    .Append(check) //? case [name] when [name] then [name] else [name] end;
                .Append(@"|\bby")
                    .Append(whitespace)
                    .Append(@"+(?<name>(?!\d+)\w+)")
                    .Append(check) //?  by [name];
                .Append(@"|\bselect")
                    .Append(whitespace)
                    .Append("+(distinct")
                    .Append(whitespace)
                    .Append("+)?")
                    .Append("(top")
                    .Append(whitespace)
                    .Append(@"+\d+")
                    .Append(whitespace)
                    .Append("+")
                    .Append(")?")
                    .Append(@"(?<name>(?!\d+)(?!\b(top|distinct)\b)\w+)")
                    .Append(check) //? select [name]; select top 10 [name]
                .Append(@"|\b(where|and|or|between)")
                    .Append(whitespace)
                    .Append(@"+(")
                        .Append("(not")
                            .Append(whitespace)
                            .Append("+)?exists")
                            .Append(whitespace)
                            .Append(@"*\(")
                            .Append(whitespace)
                            .Append("*select")
                            .Append(whitespace)
                            .Append("+(distinct")
                            .Append(whitespace)
                            .Append("+)?")
                            .Append("(top")
                                .Append(whitespace)
                                .Append(@"+\d+")
                                .Append(whitespace)
                                .Append("+")
                            .Append(")?")
                        .Append(")?")
                    .Append(@"(?<name>(?!\d+)(?!\b(not|top|distinct)\b)\w+)")
                    .Append(check) //? where not exists() where [name];
                .Append(@"|\bon")
                    .Append(whitespace)
                    .Append(@"+(?<name>(?!\d+)(?!\b(update|delete)\b)\w+)")
                    .Append(check) //? on [name]; -- mysql `modified` timestamp(0) NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(0)
                .Append("|[<=%>+/-]")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\d+)\w+)")
                    .Append(whitespace)
                    .Append(@"+(?=(and|or|case|when|then|else|end|between|select|by|join|on|(left|right|inner|outer)")
                    .Append(whitespace)
                    .Append(@"+join") // ? =[name] and
                    .Append(whitespace)
                    .Append("+))")
                .Append(@"|\*")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\d+)(?!\bfrom\b)\w+)")
                    .Append(check) //? *[value]
                .Append(@"|(?<!convert[\x20\t\r\n\f]*)\(")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\d+)\w+)")
                    .Append(whitespace)
                    .Append(@"*,") //? ([name],
                .Append(@"|,")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\d+)\w+)")
                    .Append(whitespace)
                    .Append(@"*\)") //? ,[name])
                .Append(@"|\(")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\d+)\w+)")
                    .Append(whitespace)
                    .Append(@"*(?=[<=%>+/-])") //? ([name]=
                .Append(@"|[<=%>+/-]")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\d+)\w+)")
                    .Append(whitespace)
                    .Append(@"*\)") //? =[name])
                .Append("|[&|^]")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\d+)\w+)")
                    .Append(whitespace)
                    .Append("*(?=[<=%>+/-])") //? &[name]= |[name]= -- 位运算
                .Append("|[<=%>+/-]")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\d+)\w+)")
                    .Append(whitespace)
                    .Append("*(?=[&|^])"); //? +[name]& +[name]| -- 位运算

            PatternField = new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);

            sb = new StringBuilder();

            sb.Append("^")
                .Append(whitespace)
                .Append(@"*\(")
                .Append(whitespace)
                .Append(@"*(?<name>[_a-z]\w*)")
                .Append(check) //? ([name]
                    .Append("|(,")
                    .Append(whitespace)
                    .Append(@"*(?<name>(?!\b(key|primary|unique|foreign|constraint)\b)[_a-z]\w*)")
                    .Append(check)
                    .Append(")");

            PatternFieldCreate = new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        #region Private
        /// <summary>
        /// 分析参数。
        /// </summary>
        /// <param name="value">内容。</param>
        /// <param name="startIndex">开始位置。</param>
        /// <returns></returns>
        private static Tuple<int, string[]> ParameterAnalysis(string value, int startIndex)
        {
            var list = new List<string>();
            int leftCount = 1;
            int rightCount = 0;
            int dataIndex = startIndex;

            int rightIndex;
            do
            {
                rightIndex = value.IndexOf(')', startIndex);

                if (rightIndex == -1)
                    throw new DException("SQL有语法错误!");

                rightCount += 1;

                int leftIndex;

                int commaIndex;

                do
                {

                    if (leftCount == rightCount)
                    {
                        commaIndex = value.IndexOf(',', startIndex);

                        if (commaIndex < rightIndex)
                        {
#if NETSTANDARD2_1_OR_GREATER
                            list.Add(value[dataIndex..commaIndex]);
#else
                            list.Add(value.Substring(dataIndex, commaIndex - dataIndex));
#endif

                            startIndex = dataIndex = commaIndex + 1;
                        }
                    }

                    leftIndex = value.IndexOf('(', startIndex);

                    if (leftIndex > 0 && leftIndex < rightIndex)
                    {
                        startIndex = leftIndex + 1;
                        leftCount += 1;

                        continue;
                    }

                    break;

                } while (leftIndex < rightIndex);

                startIndex = rightIndex + 1;

            } while (leftCount > rightCount);

#if NETSTANDARD2_1_OR_GREATER
            list.Add(value[dataIndex..rightIndex]);
#else
            list.Add(value.Substring(dataIndex, rightIndex - dataIndex));
#endif

            return Tuple.Create(rightIndex, list.ToArray());
        }

        /// <summary>
        /// 是空白符。
        /// </summary>
        /// <param name="c">符号。</param>
        /// <returns></returns>
        private static bool IsWhitespace(char c) => c == '\x20' || c == '\t' || c == '\r' || c == '\n' || c == '\f';

        private static bool TryDistinctField(string cols, out string col)
        {
            if (cols.Length == 0 || PatternAggregationFn.IsMatch(cols))
            {
                goto label_false;
            }

            int startIndex = -1;
            int i = 0, length = cols.Length;

        label_core:

            for (; i < length; i++)
            {
                char c = cols[i];

                if (IsWhitespace(c))
                {
                    if (startIndex == -1)
                    {
                        startIndex = i;

                        continue;
                    }

                    bool quotesFlag = false;

                    for (int j = i + 1; j < length; j++)
                    {
                        c = cols[j];

                        if (c == '\'')
                        {
                            if (quotesFlag) //? 右引号。
                            {
                                quotesFlag = false;

                                continue;
                            }

                            quotesFlag = true;
                        }

                        if (quotesFlag) //? 占位符。
                        {
                            continue;
                        }

                        if (c == ']' || c == '`' || c == '"')//? 字段标识。
                        {
                            i = j + 1;

                            continue;
                        }

                        if (c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == ',' || c == '(' || c == ')') //? 分隔符。
                        {
                            i = j + 1;

                            goto label_core;
                        }

                        if (IsWhitespace(c))
                        {
                            break;
                        }
                    }

                    break;
                }
                else if (startIndex == -1)
                {
                    startIndex = i;
                }
            }

            if (startIndex > -1 && LikeAs(cols, i))
            {
                col = cols.Substring(startIndex, i - startIndex);

                return true;
            }

        label_false:

            col = null;

            return false;
        }

        private static bool LikeAs(string cols, int startIndex)
        {
            bool firstAwordsFlag = false;
            bool nonfirstWordsFlag = true;
            int i = startIndex, length = cols.Length;

            for (; i < length; i++)
            {
                char c = cols[i];

                if (IsWhitespace(c))
                {
                    if (nonfirstWordsFlag) //? 首个字母。
                    {
                        continue;
                    }

                    return false;
                }
                else if (nonfirstWordsFlag)
                {
                    if (firstAwordsFlag)
                    {
                        firstAwordsFlag = false;

                        if (c == 's' || c == 'S')
                        {
                            continue;
                        }
                    }

                    if (c == 'a' || c == 'A')
                    {
                        firstAwordsFlag = true;

                        continue;
                    }

                    nonfirstWordsFlag = false;
                }
            }

            return true;
        }


        /// <summary>
        /// 独立SQL分析。
        /// </summary>
        /// <param name="sql">内容。</param>
        /// <param name="startIndex">开始位置。</param>
        /// <returns></returns>
        private static int IndependentSqlAnalysis(string sql, int startIndex)
        {
            int length = sql.Length;

            for (; startIndex < length; startIndex++) //? 空白符处理。
            {
                char c = sql[startIndex];

                if (IsWhitespace(c))
                {
                    continue;
                }

                break;
            }

            int indexOfCemicolon = sql.IndexOf(';', startIndex);

            if (indexOfCemicolon > -1) //? 分号分割。
            {
                length = indexOfCemicolon;
            }

            bool flag = true;
            bool quotesFlag = false;

            int letterStart = 0;

            int bracketLeft = 0;
            int bracketRight = 0;

            for (int i = startIndex + 6/* 关键字 */; i < length; i++) //? 空白符处理。
            {
                char c = sql[i];

                if (c == '\'')
                {
                    if (quotesFlag) //? 右引号。
                    {
                        quotesFlag = false;

                        continue;
                    }

                    quotesFlag = true;
                }

                if (quotesFlag) //? 占位符。
                {
                    continue;
                }

                if (char.IsLetter(c))
                {
                    if (flag)
                    {
                        flag = false;

                        letterStart = i;
                    }

                    continue;
                }

                if (flag)
                {
                    goto label_bracket;
                }

                if (IsWhitespace(c))
                {
                    flag = true;

                    if (bracketLeft == bracketRight && letterStart > 0)
                    {
                        int offset = i - letterStart;

                        if (offset == 4 && IsWith(sql, letterStart)) //? with
                        {
                            return letterStart - 1;
                        }

                        if (offset == 6 && (IsDelete(sql, letterStart)
                            || IsInsert(sql, letterStart)
                            || IsUpdate(sql, letterStart)))
                        {
                            return letterStart - 1;
                        }

                        if (offset == 6 && IsSelect(sql, letterStart))
                        {
                            int j = letterStart - 1;

                            while (IsWhitespace(sql[j])) //? 去空格。
                            {
                                j--;
                            }

                            if (IsUnion(sql, j) || IsUnionAll(sql, j) || IsExcept(sql, j) || IsIntersect(sql, j))
                            {
                                continue;
                            }

                            return letterStart - 1;
                        }

                        continue;
                    }
                }

            label_bracket:

                if (c == '(')
                {
                    bracketLeft++;
                }
                else if (c == ')')
                {
                    bracketRight++;
                }
            }

            return length;
        }

        /// <summary>
        /// 独立查询SQL类型。
        /// </summary>
        private enum IndependentSelectSqlType
        {
            None = 0, //? 未知。
            Normal = 1, //? 常规。
            HorizontalCombination = 2 //? 平级组合。
        }

        /// <summary>
        /// 是独立的查询SQL语句。
        /// </summary>
        /// <param name="sql">内容。</param>
        /// <returns></returns>
        private static IndependentSelectSqlType IndependentSelectSqlAnalysis(string sql)
        {
            int i = 0;

            int length = sql.Length;

            for (; i < length; i++) //? 空白符处理。
            {
                char c = sql[i];

                if (IsWhitespace(c))
                {
                    continue;
                }

                break;
            }

            for (int j = 0; j < 6; j++) //? 不是查询语句。
            {
                if (SelectChars[j].Equals(char.ToLower(sql[i++])))
                {
                    continue;
                }

                return IndependentSelectSqlType.None;
            }

            if (!IsWhitespace(sql[i])) //? 单词界限。
            {
                return IndependentSelectSqlType.None;
            }

            int indexOfCemicolon = sql.IndexOf(';', i);

            if (indexOfCemicolon > -1) //? 分号分割。
            {
                for (int j = indexOfCemicolon; j < length; j++)
                {
                    char c = sql[j];

                    if (IsWhitespace(c))
                    {
                        continue;
                    }

                    return IndependentSelectSqlType.None;
                }
            }

            bool flag = true;
            bool quotesFlag = false;
            bool isHorizontalCombination = false;

            int letterStart = 0;

            int bracketLeft = 0;
            int bracketRight = 0;

            for (; i < length; i++) //? 空白符处理。
            {
                char c = sql[i];

                if (c == '\'')
                {
                    if (quotesFlag) //? 右引号。
                    {
                        quotesFlag = false;

                        continue;
                    }

                    quotesFlag = true;
                }

                if (quotesFlag) //? 占位符。
                {
                    continue;
                }

                if (char.IsLetter(c))
                {
                    if (flag)
                    {
                        flag = false;

                        letterStart = i;
                    }

                    continue;
                }

                if (flag)
                {
                    goto label_bracket;
                }

                if (IsWhitespace(c))
                {
                    flag = true;

                    if (letterStart == 0)
                    {
                        continue;
                    }

                    if (bracketLeft == bracketRight)
                    {
                        int offset = i - letterStart;

                        if (offset == 4 && IsWith(sql, letterStart)) //? with
                        {
                            return IndependentSelectSqlType.None;
                        }

                        if (offset == 6 && (IsDelete(sql, letterStart)
                            || IsInsert(sql, letterStart)
                            || IsUpdate(sql, letterStart)))
                        {
                            return IndependentSelectSqlType.None;
                        }

                        if (offset == 6 && IsSelect(sql, letterStart))
                        {
                            int j = letterStart - 1;

                            while (IsWhitespace(sql[j])) //? 去空格。
                            {
                                j--;
                            }

                            if (IsUnion(sql, j) || IsUnionAll(sql, j) || IsExcept(sql, j) || IsIntersect(sql, j))
                            {
                                isHorizontalCombination = true;

                                continue;
                            }

                            return IndependentSelectSqlType.None;
                        }
                    }

                    continue;
                }

            label_bracket:

                if (c == '(')
                {
                    bracketLeft++;
                }
                else if (c == ')')
                {
                    bracketRight++;
                }
            }

            return isHorizontalCombination
                ? IndependentSelectSqlType.HorizontalCombination
                : IndependentSelectSqlType.Normal;
        }

        private static readonly char[] SelectChars = new char[] { 's', 'e', 'l', 'e', 'c', 't' };
        private static bool IsSelect(string sql, int startIndex)
        {
            for (int i = 0; i < 6; i++)
            {
                if (SelectChars[i].Equals(char.ToLower(sql[startIndex + i])))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static readonly char[] InsertChars = new char[] { 'i', 'n', 's', 'e', 'r', 't' };

        private static bool IsInsert(string sql, int startIndex)
        {
            for (int i = 0; i < 6; i++)
            {
                if (InsertChars[i].Equals(char.ToLower(sql[startIndex + i])))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static readonly char[] UpdateChars = new char[] { 'u', 'p', 'd', 'a', 't', 'e' };
        private static bool IsUpdate(string sql, int startIndex)
        {
            for (int i = 0; i < 6; i++)
            {
                if (UpdateChars[i].Equals(char.ToLower(sql[startIndex + i])))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static readonly char[] DeleteChars = new char[] { 'd', 'e', 'l', 'e', 't', 'e' };
        private static bool IsDelete(string sql, int startIndex)
        {
            for (int i = 0; i < 6; i++)
            {
                if (DeleteChars[i].Equals(char.ToLower(sql[startIndex + i])))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static readonly char[] WithChars = new char[] { 'w', 'i', 't', 'h' };
        private static bool IsWith(string sql, int startIndex)
        {
            for (int i = 0; i < 4; i++)
            {
                if (WithChars[i].Equals(char.ToLower(sql[startIndex + i])))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static readonly char[] AllChars = new char[] { 'a', 'l', 'l' };
        private static readonly char[] UnionChars = new char[] { 'u', 'n', 'i', 'o', 'n' };

        private static bool IsUnion(string sql, int endIndex)
        {
            if (endIndex < 5)
            {
                return false;
            }

            int startIndex = endIndex - 4;

            for (int i = 0; i < 5; i++)
            {
                if (UnionChars[i].Equals(char.ToLower(sql[startIndex + i])))
                {
                    continue;
                }

                return false;
            }

            return startIndex == 0 || IsWhitespace(sql[startIndex - 1]);
        }
        private static bool IsUnionAll(string sql, int endIndex)
        {
            if (endIndex < 9)
            {
                return false;
            }
            int startIndex = endIndex - 2;

            for (int i = 0; i < 3; i++)
            {
                if (AllChars[i].Equals(char.ToLower(sql[startIndex + i])))
                {
                    continue;
                }

                return false;
            }

            int j = startIndex - 1;

            while (IsWhitespace(sql[j])) //? 去空格。
            {
                j--;
            }

            return IsUnion(sql, j);
        }

        private static readonly char[] IntersectChars = new char[] { 'i', 'n', 't', 'e', 'r', 's', 'e', 'c', 't' };

        private static bool IsIntersect(string value, int endIndex)
        {
            if (endIndex < 9)
            {
                return false;
            }

            int startIndex = endIndex - 8;

            for (int i = 0; i < 9; i++)
            {
                if (IntersectChars[i].Equals(char.ToLower(value[startIndex + i])))
                {
                    continue;
                }

                return false;
            }

            return startIndex == 0 || IsWhitespace(value[startIndex - 1]);
        }

        private static readonly char[] ExceptChars = new char[] { 'e', 'x', 'c', 'e', 'p', 't' };

        private static bool IsExcept(string value, int endIndex)
        {
            if (endIndex < 6)
            {
                return false;
            }

            int startIndex = endIndex - 5;

            for (int i = 0; i < 6; i++)
            {
                if (ExceptChars[i].Equals(char.ToLower(value[startIndex + i])))
                {
                    continue;
                }

                return false;
            }

            return startIndex == 0 || IsWhitespace(value[startIndex - 1]);
        }

        #endregion

        /// <summary>
        /// SQL 分析。
        /// </summary>
        /// <param name="sql">语句。</param>
        /// <returns></returns>
        public string Analyze(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("语句不能为空或空字符串!");
            }

            return SqlCache.GetOrAdd(sql, Aw_Analyze);
        }

        private static string Aw_Analyze(string sql)
        {
            //? 注解
            sql = PatternAnnotate.Replace(sql, string.Empty);

            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new DSyntaxErrorException("未检测到可执行的语句!");
            }

            List<string> characters = new List<string>();

            //? 提取字符串
            sql = PatternCharacter.Replace(sql, item =>
            {
                string value = item.Value;

                if (value.Length > 2)
                {
                    characters.Add(value.Substring(1, item.Value.Length - 2));

                    var charStr = char.ToString(value[0]);

                    return string.Concat(charStr, "?", charStr);
                }

                return value;
            });

            //? 去除多余的空白符。
            sql = PatternWhitespace.Replace(sql, string.Empty);

            //? Trim
            sql = PatternTrim.Replace(sql, item =>
            {
                Group nameGrp = item.Groups["name"];

                string name = nameGrp.Value;

                return string.Concat("L", name, "(R", name,
#if NETSTANDARD2_1_OR_GREATER
                    item.Value[nameGrp.Length..],
#else
                    item.Value.Substring(nameGrp.Length),
#endif
                    ")");
            });

            int offset = 0;

            StringBuilder sbSql = new StringBuilder();

            List<TableToken> tables = new List<TableToken>();

            HashSet<string> withAs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var withAsMt = PatternWithAs.Match(sql);

            while (withAsMt.Success)
            {
                if (withAsMt.Index > offset) //? 前部分。
                {
                    sbSql.Append(Done(sql.Substring(offset, withAsMt.Index - offset)));
                }

                var nameGrp = withAsMt.Groups["name"];
                var sqlGrp = withAsMt.Groups["sql"];

                for (int i = 0, length = nameGrp.Captures.Count; i < length; i++)
                {
                    var nameCap = nameGrp.Captures[i];
                    var sqlCap = sqlGrp.Captures[i];

                    if (i == 0)
                    {
                        sbSql.Append(sql.Substring(withAsMt.Index, nameCap.Index - withAsMt.Index));
                    }
                    else
                    {
                        sbSql.Append('\x20')
                            .Append(',');
                    }

                    sbSql.Append('[')
                        .Append(nameCap.Value)
                        .Append(']')
                        .Append(sql.Substring(nameCap.Index + nameCap.Length, sqlCap.Index - nameCap.Index - nameCap.Length))
                        .Append(Done(sqlCap.Value))
                        .Append(')')
                        .Append(Environment.NewLine);

                    withAs.Add(nameCap.Value);
                }

                offset = IndependentSqlAnalysis(sql, withAsMt.Index + withAsMt.Length);

                sbSql.Append(Done(sql.Substring(withAsMt.Index + withAsMt.Length, offset - withAsMt.Index - withAsMt.Length)));

                withAsMt = withAsMt.NextMatch();

                withAs.Clear(); //? 清空别名集合。
            }

            if (sql.Length > offset)
            {
                sbSql.Append(Done(sql.Substring(offset)));
            }

            string result = sbSql.ToString();

            if (tables.Count > 0)
            {
#if NET40
                TableCache.TryAdd(result, tables.ToReadOnlyList());
#else
                TableCache.TryAdd(result, tables);
#endif
            }

            if (characters.Count > 0)
            {
                CharacterCache.GetOrAdd(result, characters);
            }

            return result;

            string Done(string sqlStr)
            {
                //? 创建表指令。
                sqlStr = PatternCreate.Replace(sqlStr, item =>
                {
                    var nameGrp = item.Groups["name"];
                    var tableGrp = item.Groups["table"];

                    var sb = new StringBuilder();

                    AddTableToken(CommandTypes.Create, tableGrp.Value);

                    return sb.Append(item.Value.Substring(0, tableGrp.Index - item.Index))
                     .Append("[")
                     .Append(nameGrp.Value)
                     .Append("]")
#if NETSTANDARD2_1_OR_GREATER
                 .Append(PatternFieldCreate.Replace(item.Value[(tableGrp.Index - item.Index + tableGrp.Length)..], match =>
#else
                 .Append(PatternFieldCreate.Replace(item.Value.Substring(tableGrp.Index - item.Index + tableGrp.Length), match =>
#endif
                 {
                     Group nameGrp2 = match.Groups["name"];

                     string value2 = string.Concat("[", nameGrp2.Value, "]");

                     if (nameGrp2.Index == item.Index && nameGrp2.Length == match.Length)
                         return value2;

                     return string.Concat(match.Value.Substring(0, nameGrp2.Index - match.Index), value2,
#if NETSTANDARD2_1_OR_GREATER
                         match.Value[(nameGrp2.Index - match.Index + nameGrp2.Length)..]
#else
                         match.Value.Substring(nameGrp2.Index - match.Index + nameGrp2.Length)
#endif
                         );
                 }))
                     .ToString();
                });

                //? 删除表指令。
                sqlStr = PatternDrop.Replace(sqlStr, item =>
                {
                    var nameGrp = item.Groups["name"];
                    var tableGrp = item.Groups["table"];

                    var sb = new StringBuilder();

                    AddTableToken(CommandTypes.Drop, tableGrp.Value);

                    return sb.Append(item.Value.Substring(0, tableGrp.Index - item.Index))
                     .Append("[")
                     .Append(nameGrp.Value)
                     .Append("]")
#if NETSTANDARD2_1_OR_GREATER
                 .Append(item.Value[(tableGrp.Index - item.Index + tableGrp.Length)..])
#else
                 .Append(item.Value.Substring(tableGrp.Index - item.Index + tableGrp.Length))
#endif
                 .ToString();
                });

                //? 修改表指令。
                sqlStr = PatternAlter.Replace(sqlStr, item =>
                {
                    var nameGrp = item.Groups["name"];

                    AddTableToken(CommandTypes.Alter, nameGrp.Value);

                    return string.Concat(item.Value.Substring(0, nameGrp.Index - item.Index), "[", nameGrp.Value, "]");
                });

                //? 插入表指令。
                sqlStr = PatternInsert.Replace(sqlStr, item =>
                {
                    var nameGrp = item.Groups["name"];

                    AddTableToken(CommandTypes.Insert, nameGrp.Value);

                    return string.Concat(item.Value.Substring(0, nameGrp.Index - item.Index), "[", nameGrp.Value, "]");
                });

                //? 复杂结构表名称处理
                sqlStr = PatternChangeComplex.Replace(sqlStr, item =>
                {
                    var value = item.Value;

                    var typeGrp = item.Groups["type"];
                    var nameGrp = item.Groups["name"];
                    var tableGrp = item.Groups["table"];
                    var aliasGrp = item.Groups["alias"];

                    var type = typeGrp.Value.ToUpper();

                    var sb = new StringBuilder();

                    AddTableToken(type, nameGrp.Value);

                    sb.Append(value.Substring(0, aliasGrp.Index))
                    .Append("[")
                    .Append(aliasGrp.Value)
                    .Append("]")
                    .Append(value.Substring(aliasGrp.Index + aliasGrp.Length, tableGrp.Index - aliasGrp.Index))
                    .Append("[")
                    .Append(nameGrp.Value)
                    .Append("]");

#if NETSTANDARD2_1_OR_GREATER
                value = value[(tableGrp.Index - item.Index + tableGrp.Length)..];
#else
                    value = value.Substring(tableGrp.Index - item.Index + tableGrp.Length);
#endif

                    var indexOf = value.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);

                    return sb.Append(PatternForm.Replace(value, match => Form(match, type), indexOf > -1 ? indexOf : value.Length)).ToString();
                });

                //? 表名称处理(表别名作为名称处理)
                sqlStr = PatternChange.Replace(sqlStr, item =>
                {
                    var value = item.Value;

                    var typeGrp = item.Groups["type"];
                    var nameGrp = item.Groups["name"];
                    var tableGrp = item.Groups["table"];

                    var type = typeGrp.Value.ToUpper();

                    var sb = new StringBuilder();

                    AddTableToken(type, nameGrp.Value);

                    sb.Append(value.Substring(0, tableGrp.Index - item.Index))
                    .Append("[")
                    .Append(nameGrp.Value)
                    .Append("]");

#if NETSTANDARD2_1_OR_GREATER
                value = value[(tableGrp.Index - item.Index + tableGrp.Length)..];
#else
                    value = value.Substring(tableGrp.Index - item.Index + tableGrp.Length);
#endif

                    var indexOf = value.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);

                    return sb.Append(PatternForm.Replace(value, match => Form(match, type), indexOf > -1 ? indexOf : value.Length)).ToString();
                });

                //? 查询语句。
                sqlStr = PatternForm.Replace(sqlStr, item => Form(item, "SELECT"));

                //? 参数处理。
                sqlStr = PatternParameter.Replace(sqlStr, item => string.Concat("{", item.Groups["name"].Value, "}"));

                //? 字段和别名处理。
                sqlStr = PatternAliasField.Replace(sqlStr, item =>
                {
                    var sb = new StringBuilder();

                    Group nameGrp = item.Groups["name"];
                    Group aliasGrp = item.Groups["alias"];

                    return sb.Append("[")
                          .Append(aliasGrp.Value)
                          .Append("].[")
                          .Append(nameGrp.Value)
                          .Append("]")
                          .ToString();
                });

                //? 独立参数字段处理
                sqlStr = PatternSingleArgField.Replace(sqlStr, item =>
                {
                    var sb = new StringBuilder();
                    Group nameGrp = item.Groups["name"];

                    return sb
                        .Append(item.Value.Substring(0, nameGrp.Index - item.Index))
                        .Append("[")
                        .Append(nameGrp.Value)
                        .Append("]")
#if NETSTANDARD2_1_OR_GREATER
                    .Append(item.Value[(nameGrp.Index - item.Index + nameGrp.Length)..])
#else
                    .Append(item.Value.Substring(nameGrp.Index - item.Index + nameGrp.Length))
#endif
                    .ToString();
                });

                //? 字段处理。
                sqlStr = PatternField.Replace(sqlStr, item =>
                {
                    var sb = new StringBuilder();

                    Group nameGrp = item.Groups["name"];

                    return sb.Append(item.Value.Substring(0, nameGrp.Index - item.Index))
                            .Append("[")
                            .Append(nameGrp.Value)
                            .Append("]")
#if NETSTANDARD2_1_OR_GREATER
                        .Append(item.Value[(nameGrp.Index - item.Index + nameGrp.Length)..])
#else
                        .Append(item.Value.Substring(nameGrp.Index - item.Index + nameGrp.Length))
#endif
                        .ToString();
                });

                //? 字段处理。
                sqlStr = PatternFieldEmbody.Replace(sqlStr, item =>
                {
                    Group nameGrp = item.Groups["name"];

                    string value = string.Concat("[", nameGrp.Value, "]");

                    if (nameGrp.Index == item.Index && nameGrp.Length == item.Length)
                        return value;

                    return item.Value.Substring(0, nameGrp.Index - item.Index) +
                    value +
#if NETSTANDARD2_1_OR_GREATER
                    item.Value[(nameGrp.Index - item.Index + nameGrp.Length)..];
#else
                    item.Value.Substring(nameGrp.Index - item.Index + nameGrp.Length);
#endif
                });

                //? 字段别名。
                sqlStr = PatternAsField.Replace(sqlStr, item =>
                {
                    Group nameGrp = item.Groups["name"];

                    return string.Concat(item.Value.Substring(0, nameGrp.Index - item.Index), "[", nameGrp.Value, "]");
                });

                return sqlStr;
            }

            string Form(Match item, string type)
            {
                var value = item.Value;

                var nameGrp = item.Groups["name"];

                var tableGrp = item.Groups["table"];
                var aliasGrp = item.Groups["alias"];
                var followGrp = item.Groups["follow"];

                var sb = new StringBuilder();

                if (!withAs.Contains(nameGrp.Value))
                {
                    AddTableToken(type, nameGrp.Value);
                }

                sb.Append(value.Substring(0, tableGrp.Index - item.Index))
                    .Append("[")
                    .Append(nameGrp.Value)
                    .Append("]");

                if (aliasGrp.Success)
                {
                    sb.Append(value.Substring(tableGrp.Index - item.Index + tableGrp.Length, aliasGrp.Index - tableGrp.Index - tableGrp.Length))
                        .Append("[")
                        .Append(aliasGrp.Value)
                        .Append("]");
                }
                else if (followGrp.Success)
                {
                    sb.Append(value.Substring(tableGrp.Index - item.Index + tableGrp.Length, followGrp.Index - tableGrp.Index - tableGrp.Length));
                }
                else
                {
#if NETSTANDARD2_1_OR_GREATER
                    sb.Append(value[(tableGrp.Index + tableGrp.Length - item.Index)..]);
#else
                    sb.Append(value.Substring(tableGrp.Index + tableGrp.Length - item.Index));
#endif
                }

                if (followGrp.Success)
                {
#if NETSTANDARD2_1_OR_GREATER
                    sb.Append(PatternFormFollow.Replace(value[(followGrp.Index - item.Index)..], match => Form(match, type)));
#else
                    sb.Append(PatternFormFollow.Replace(value.Substring(followGrp.Index - item.Index), match => Form(match, type)));
#endif
                }

                return sb.ToString();
            }

            void AddTableToken(UppercaseString commandType, string name)
            {
                if (!tables.Exists(x => x.CommandType == commandType && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    tables.Add(new TableToken(commandType, name));
                }
            }
        }

        /// <summary>
        /// SQL 分析（表名称）。
        /// </summary>
        /// <param name="sql">来源于【<see cref="Analyze(string)"/>】的结果。</param>
        /// <returns></returns>
        public IReadOnlyList<TableToken> AnalyzeTables(string sql)
            => TableCache.GetOrAdd(sql, TableToken.None);

        /// <summary>
        /// SQL 分析（参数）。
        /// </summary>
        /// <param name="sql">来源于【<see cref="Analyze(string)"/>】的结果。</param>
        /// <returns></returns>
        public IReadOnlyList<string> AnalyzeParameters(string sql)
        {
            Match match = PatternParameterToken.Match(sql);

            List<string> parameters = new List<string>();

            while (match.Success)
            {
                string name = match.Groups["name"].Value;

                if (!parameters.Contains(name))
                {
                    parameters.Add(name);
                }

                match = match.NextMatch();
            }
#if NET40
            return parameters.ToReadOnlyList();
#else
            return parameters;
#endif
        }

        /// <summary>
        /// 获取符合条件的条数。
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <example>SELECT * FROM Users WHERE Id > 100 => SELECT COUNT(1) FROM Users WHERE Id > 100</example>
        /// <example>SELECT * FROM Users WHERE Id > 100 ORDER BY Id DESC => SELECT COUNT(1) FROM Users WHERE Id > 100</example>
        /// <returns></returns>
        public string ToCountSQL(string sql) => SqlCountCache.GetOrAdd(sql, Aw_ToCountSQL);

        private string Aw_ToCountSQL(string sql)
        {
            var withAsMt = PatternWithAs.Match(sql);

            if (withAsMt.Success)
            {
                return string.Concat(sql.Substring(0, withAsMt.Index + withAsMt.Length),
                    Environment.NewLine,
                    Aw_ToCountSQL_Simple(sql.Substring(withAsMt.Index + withAsMt.Length)));
            }

            return Aw_ToCountSQL_Simple(sql);
        }

        private static string Aw_ToCountSQL_Simple(string sql)
        {
            IndependentSelectSqlType independentType = IndependentSelectSqlAnalysis(sql);

            if (independentType == IndependentSelectSqlType.None)
            {
                throw new NotSupportedException($"不支持({sql})语句的行数统计!");
            }

            var sb = new StringBuilder();

            if (independentType == IndependentSelectSqlType.HorizontalCombination)
            {
                return sb.Append("SELECT COUNT(1) FROM (")
                    .Append(sql)
                    .Append(") AS xRows")
                    .ToString();
            }

            var colsMt = PatternColumn.Match(sql);

            if (!colsMt.Success)
            {
                return sb.Append("SELECT COUNT(1) FROM (")
                    .Append(sql)
                    .Append(") AS xRows")
                    .ToString();
            }

            var colsGrp = colsMt.Groups["cols"];
            var distinctGrp = colsMt.Groups["distinct"];

            if (distinctGrp.Success)
            {
                if (!TryDistinctField(colsGrp.Value, out string col))
                {
                    return sb.Append("SELECT COUNT(1) FROM (")
                       .Append(sql)
                       .Append(") AS xRows")
                       .ToString();
                }

                sb.Append(sql.Substring(0, distinctGrp.Index))
                    .Append("COUNT(")
                    .Append(distinctGrp.Value)
                    .Append(col)
                    .Append(')');
            }
            else
            {
                sb.Append(sql.Substring(0, colsGrp.Index))
                    .Append("COUNT(1)");
            }

            var orderByMt = PatternOrderBy.Match(sql);

            int subIndex = colsGrp.Index + colsGrp.Length;

            //? 补充 \r\n 换行符处理。
            while (sql[subIndex] == '\n' || sql[subIndex] == '\r')
            {
                subIndex--;
            }

            if (orderByMt.Success)
            {
#if NETSTANDARD2_1_OR_GREATER
                sb.Append(sql[subIndex..orderByMt.Index]);
#else
                sb.Append(sql.Substring(subIndex, orderByMt.Index - subIndex));
#endif
            }
            else
            {
#if NETSTANDARD2_1_OR_GREATER
                sb.Append(sql[subIndex..]);
#else
                sb.Append(sql.Substring(subIndex));
#endif
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成分页SQL。
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="pageIndex">页码（从“0”开始）</param>
        /// <param name="pageSize">分页条数</param>
        /// <example>SELECT * FROM Users WHERE Id > 100 => PAGING(`SELECT * FROM Users WHERE Id > 100`,<paramref name="pageIndex"/>,<paramref name="pageSize"/>)</example>
        /// <example>SELECT * FROM Users WHERE Id > 100 ORDER BY Id DESC => PAGING(`SELECT * FROM Users WHERE Id > 100`,<paramref name="pageIndex"/>,<paramref name="pageSize"/>,`ORDER BY Id DESC`)</example>
        /// <returns></returns>
        public string ToSQL(string sql, int pageIndex, int pageSize)
            => SqlPagedCache.GetOrAdd(sql, Aw_ToSQL)
                .Replace("{=index}", pageIndex.ToString())
                .Replace("{=size}", pageSize.ToString());

        private string Aw_ToSQL(string sql)
        {
            var withAsMt = PatternWithAs.Match(sql);

            if (withAsMt.Success)
            {
                string mainSql = Aw_ToSQL_Simple(sql.Substring(withAsMt.Index + withAsMt.Length));

                return string.Concat("PAGING(`", sql.Substring(0, withAsMt.Index + withAsMt.Length),
                    Environment.NewLine,
                    mainSql);
            }
            else
            {
                string mainSql = Aw_ToSQL_Simple(sql);

                return string.Concat("PAGING(`", mainSql);
            }
        }

        private static string Aw_ToSQL_Simple(string sql)
        {
            if (PatternPaging.IsMatch(sql))
            {
                throw new NotSupportedException("请勿重复分页!");
            }

            IndependentSelectSqlType independentType = IndependentSelectSqlAnalysis(sql);

            if (independentType == IndependentSelectSqlType.None)
            {
                throw new NotSupportedException($"不支持({sql})语句的分页!");
            }

            var sb = new StringBuilder();

            if (independentType == IndependentSelectSqlType.HorizontalCombination)
            {
                return sb.Append("SELECT * FROM (")
                         .Append(sql)
                         .Append(") AS xRows")
                         .Append('`')
                         .Append(',')
                         .Append("{=index}")
                         .Append(',')
                         .Append("{=size}")
                         .Append(')')
                         .ToString();
            }

            var orderByMt = PatternOrderBy.Match(sql);

            if (orderByMt.Success)
            {
                sb.Append(sql.Substring(0, orderByMt.Index))
                    .Append('`')
                    .Append(',')
                    .Append("{=index}")
                    .Append(',')
                    .Append("{=size}")
                    .Append(',')
                    .Append('`')
#if NETSTANDARD2_1_OR_GREATER
                    .Append(sql[orderByMt.Index..])
#else
                    .Append(sql.Substring(orderByMt.Index))
#endif
                    .Append('`');
            }
            else
            {
                sb.Append(sql)
                    .Append('`')
                    .Append(',')
                    .Append("{=index}")
                    .Append(',')
                    .Append("{=size}");
            }

            return sb.Append(')')
                .ToString();
        }

        /// <summary>
        /// SQL 格式化。
        /// </summary>
        /// <param name="sql">语句。</param>
        /// <param name="settings">配置。</param>
        /// <returns></returns>
        public string Format(string sql, ISQLCorrectSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            bool flag = CharacterCache.TryGetValue(sql, out List<string> characters);

            //? 分页。
            sql = PatternPaging.Replace(sql, match =>
            {
                var mainSql = match.Groups["main"].Value;
                var pageIndex = Convert.ToInt32(match.Groups["index"].Value);
                var pageSize = Convert.ToInt32(match.Groups["size"].Value);
                var orderBySql = match.Groups["orderby"].Value;

                return settings.ToSQL(mainSql, pageSize, pageIndex * pageSize, orderBySql);
            });

            //? 检查 IndexOf 函数，参数处理。
            if (settings.IndexOfSwapPlaces)
            {
                Match match = PatternIndexOf.Match(sql);

                while (match.Success)
                {
                    int startIndex = match.Index + match.Length;

                    var tuple = ParameterAnalysis(sql, startIndex);

                    sql = sql.Substring(0, startIndex) +
                     (
                         tuple.Item2.Length > 2 ?
                         string.Concat(tuple.Item2[1], ",", tuple.Item2[0], ",", tuple.Item2[2]) :
                         string.Concat(tuple.Item2[1], ",", tuple.Item2[0])
                     ) +
#if NETSTANDARD2_1_OR_GREATER
                     sql[tuple.Item1..];
#else
                     sql.Substring(tuple.Item1);
#endif

                    match = PatternIndexOf.Match(sql, match.Index + 7);
                }
            }

            //? 检查并处理函数名称。
            sql = PatternConvincedMethod.Replace(sql, item =>
            {
                Group nameGrp = item.Groups["name"];

                string name = nameGrp.Value.ToUpper();

                if (name == "LEN" || name == "LENGTH")
                {
                    return settings.Length;
                }

                if (name == "SUBSTRING" || name == "SUBSTR")
                {
                    return settings.Substring;
                }

                if (name == "INDEXOF")
                {
                    return settings.IndexOf;
                }

                return item.Value;
            });

            //? 名称。
            sql = PatternNameToken.Replace(sql, item => settings.Name(item.Groups["name"].Value));

            //? 参数名称。
            sql = PatternParameterToken.Replace(sql, item => settings.ParamterName(item.Groups["name"].Value));

            foreach (IFormatter formatter in settings.Formatters)
            {
                sql = formatter.RegularExpression.Replace(sql, formatter.Evaluator);
            }

            if (flag)
            {
                int offset = 0;

                //? 还原字符串。
                sql = PatternCharacter.Replace(sql, item =>
                {
                    string value = item.Value;

                    if (value.Length > 2)
                    {
                        var charStr = char.ToString(value[0]);

                        return string.Concat(charStr, characters[offset++], charStr);
                    }

                    return value;
                });
            }

            return sql;
        }
    }
}
