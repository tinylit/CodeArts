﻿using CodeArts.ORM.Exceptions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
#if NET40
using System.Collections.ObjectModel;
#endif
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeArts.ORM
{
    /// <summary>
    /// SQL 默认语法：
    ///     表名称：[yep_users]
    ///     名称：[name]
    ///     参数名称:{name}
    ///     条件移除：DROP TABLE IF EXIXSTS [yep_users];
    ///     条件创建：CREATE TABLE IF NOT EXIXSTS [yep_users] ([Id] int not null,[name] varchar(100));
    /// 说明：会自动去除代码注解和多余的换行符压缩语句。
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public class SQL : ISQL
    {
        private readonly StringBuilder sb;

        private static readonly ConcurrentDictionary<string, string> SqlCache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 暂存数据
        /// </summary>
        private static readonly Regex PatternTemporaryStorage = new Regex("--#(?<index>\\d+)", RegexOptions.Compiled);

        /// <summary>
        /// 注解
        /// </summary>
        private static readonly Regex PatternAnnotate = new Regex(@"--.*|/\*[\s\S]*?\*/", RegexOptions.Compiled);

        /// <summary>
        /// 提取字符串
        /// </summary>
        private static readonly Regex PatternCharacter = new Regex("(['\"])(?:\\\\.|[^\\\\])*?\\1", RegexOptions.Compiled);

        /// <summary>
        /// 多余的换行符。
        /// </summary>
        private static readonly Regex PatternLineBreak = new Regex(@"(^[\r\n][\x20\t\r\n\f]*[\r\n]|(?<=[\r\n]{2})[\x20\t\r\n\f]*[\r\n]|(?<=[\r\n]{2})[\r\n]{2,})", RegexOptions.Compiled);

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
        private static readonly Regex PatternForm = new Regex(@"\b(from|join)[\x20\t\r\n\f]+([\w+\[\]]\.)*(?<table>(?<name>\w+)|\[(?<name>\w+)\])[\x20\t\r\n\f]+(as[\x20\t\r\n\f]+(?<alias>\w+)|(?<alias>(?!\b(where|on|join|group|order|having|select|into|limit|(left|right|inner|outer)[\x20\t\r\n\f]+join)\b)\w+))?(?<follow>((?!\b(where|on|join|order|group|having|select|insert|update|delete|create|drop|alter|truncate|use|set|(left|right|inner|outer|full)[\x20\t\r\n\f]+join)\b)[^;])+)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 连表命令。
        /// </summary>
        private static readonly Regex PatternFormFollow = new Regex(@",[\x20\t\r\n\f]*(?<table>(?<name>\w+)|\[(?<name>\w+)\])([\x20\t\r\n\f]+(as[\x20\t\r\n\f]+)?(?<alias>\w+))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
        /// 别名字段
        /// </summary>
        private static readonly Regex PatternAliasField = new Regex(@"(?<alias>[\w-]+)\.(?<name>(?!\d+)[\w-]+)(?=[^\w\.\]\}\(]|$)", RegexOptions.Compiled);

        /// <summary>
        /// 独立参数字段
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
        /// 声明别量
        /// </summary>
        private static readonly Regex PatternDeclare = new Regex(@"\bdeclare[\x20\t\r\n\f]+((?!\b(select|insert|update|delete|create|drop|alter|truncate|use|set|declare|exec|execute|sp_executesql)\b)[^;])+;?", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

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
        /// 表令牌。
        /// </summary>
        private static readonly Regex PatternTableToken = new Regex(@"\{(?<type>[A-Z]+)#(?<name>\w+)\}", RegexOptions.Compiled);

        /// <summary>
        /// 字段令牌。
        /// </summary>
        private static readonly Regex PatternFieldToken = new Regex(@"\[(?<name>\w+)\]", RegexOptions.Compiled);

        /// <summary>
        /// 参数令牌。
        /// </summary>
        private static readonly Regex PatternParameterToken = new Regex(@"\{(?<name>[\p{L}\p{N}@_]+)\}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// 静态构造函数。
        /// </summary>
        static SQL()
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
                            .Append("+(top")
                                .Append(whitespace)
                                .Append(@"+\d+")
                                .Append(whitespace)
                                .Append("+")
                            .Append(")?")
                        .Append(")?")
                    .Append(@"(?<name>(?!\d+)(?!\b(not|then|top)\b)\w+)")
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
                    .Append("+(")
                        .Append("top")
                        .Append(whitespace)
                        .Append(@"+\d+")
                        .Append(whitespace)
                        .Append("+")
                    .Append(")?")
                    .Append(@"(?<name>(?!\d+)(?!\btop\b)\w+)")
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
                            .Append("+(top")
                                .Append(whitespace)
                                .Append(@"+\d+")
                                .Append(whitespace)
                                .Append("+")
                            .Append(")?")
                        .Append(")?")
                    .Append(@"(?<name>(?!\d+)(?!\b(not|then|top)\b)\w+)")
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
                .Append(@"|\(")
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

            sb.Clear();

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

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sql">原始SQL语句</param>
        public SQL(string sql) => sb = new StringBuilder(SqlCache.GetOrAdd(sql, MakeSQL));

        /// <summary>
        /// 制定SQL。
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns></returns>
        private static string MakeSQL(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("语句不能为空或空字符串!");

            //? 注解
            sql = PatternAnnotate.Replace(sql, string.Empty);

            if (string.IsNullOrWhiteSpace(sql))
                throw new DSyntaxErrorException("为检测到可执行的语句!");

            List<string> temporaryStorages = new List<string>();

            //? 提取字符串
            sql = PatternCharacter.Replace(sql, item =>
            {
                string value = item.Value;

                if (value.Length > 2)
                {
                    var sb = new StringBuilder();

                    sb.Append(value[0])
                        .Append("--#")
                        .Append(temporaryStorages.Count)
                        .Append(value[value.Length - 1]);

                    temporaryStorages.Add(value.Substring(1, item.Value.Length - 2));

                    return sb.ToString();
                }

                return value;
            });

            //? 去除多余的换行。
            sql = PatternLineBreak.Replace(sql, string.Empty);

            //? Trim
            sql = PatternTrim.Replace(sql, item =>
            {
                Group nameGrp = item.Groups["name"];

                string name = nameGrp.Value;

                return string.Concat("L", name, "(R", name, item.Value.Substring(nameGrp.Length), ")");
            });

            //? 创建表指令。
            sql = PatternCreate.Replace(sql, item =>
            {
                var nameGrp = item.Groups["name"];
                var tableGrp = item.Groups["table"];

                var sb = new StringBuilder();

                return sb.Append(item.Value.Substring(0, tableGrp.Index - item.Index))
                 .Append("{CREATE#")
                 .Append(nameGrp.Value)
                 .Append("}")
                 .Append(PatternFieldCreate.Replace(item.Value.Substring(tableGrp.Index - item.Index + tableGrp.Length), match =>
                 {
                     Group nameGrp2 = match.Groups["name"];

                     string value2 = string.Concat("[", nameGrp2.Value, "]");

                     if (nameGrp2.Index == item.Index && nameGrp2.Length == match.Length)
                         return value2;

                     return string.Concat(match.Value.Substring(0, nameGrp2.Index - match.Index), value2, match.Value.Substring(nameGrp2.Index - match.Index + nameGrp2.Length));
                 }))
                 .ToString();
            });

            //? 删除表指令。
            sql = PatternDrop.Replace(sql, item =>
            {
                var nameGrp = item.Groups["name"];
                var tableGrp = item.Groups["table"];

                var sb = new StringBuilder();

                return sb.Append(item.Value.Substring(0, tableGrp.Index - item.Index))
                 .Append("{DROP#")
                 .Append(nameGrp.Value)
                 .Append("}")
                 .Append(item.Value.Substring(tableGrp.Index - item.Index + tableGrp.Length))
                 .ToString();
            });

            //? 修改表指令。
            sql = PatternAlter.Replace(sql, item =>
            {
                var nameGrp = item.Groups["name"];

                return string.Concat(item.Value.Substring(0, nameGrp.Index - item.Index), "{ALTER#", nameGrp.Value, "}");
            });

            //? 插入表指令。
            sql = PatternInsert.Replace(sql, item =>
            {
                var nameGrp = item.Groups["name"];

                return string.Concat(item.Value.Substring(0, nameGrp.Index - item.Index), "{INSERT#", nameGrp.Value, "}");
            });

            //? 复杂结构表名称处理
            sql = PatternChangeComplex.Replace(sql, item =>
            {
                var value = item.Value;

                var typeGrp = item.Groups["type"];
                var nameGrp = item.Groups["name"];
                var aliasGrp = item.Groups["alias"];

                var type = typeGrp.Value.ToUpper();

                var sb = new StringBuilder();

                sb.Append(value.Substring(0, aliasGrp.Index))
                .Append("[")
                .Append(aliasGrp.Value)
                .Append("]")
                .Append(value.Substring(aliasGrp.Index + aliasGrp.Length, nameGrp.Index - aliasGrp.Index))
                .Append("{")
                .Append(type)
                .Append("#")
                .Append(nameGrp.Value)
                .Append("}");

                value = value.Substring(nameGrp.Index - item.Index + nameGrp.Length);

                var indexOf = value.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);

                return sb.Append(PatternForm.Replace(value, match => Form(match, type), indexOf > -1 ? indexOf : value.Length)).ToString();
            });

            //? 表名称处理(表别名作为名称处理)
            sql = PatternChange.Replace(sql, item =>
            {
                var value = item.Value;

                var typeGrp = item.Groups["type"];
                var nameGrp = item.Groups["name"];

                var type = typeGrp.Value.ToUpper();

                var sb = new StringBuilder();

                sb.Append(value.Substring(0, nameGrp.Index - item.Index))
                .Append("{")
                .Append(type)
                .Append("#")
                .Append(nameGrp.Value)
                .Append("}");

                value = value.Substring(nameGrp.Index - item.Index + nameGrp.Length);

                var indexOf = value.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);

                return sb.Append(PatternForm.Replace(value, match => Form(match, type), indexOf > -1 ? indexOf : value.Length)).ToString();
            });

            string Form(Match item, string type)
            {
                var value = item.Value;

                var nameGrp = item.Groups["name"];
                var tableGrp = item.Groups["table"];
                var aliasGrp = item.Groups["alias"];
                var followGrp = item.Groups["follow"];

                var sb = new StringBuilder();

                sb.Append(value.Substring(0, tableGrp.Index - item.Index))
                    .Append("{")
                    .Append(type)
                    .Append("#")
                    .Append(nameGrp.Value)
                    .Append("}");

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
                    sb.Append(value.Substring(tableGrp.Index + tableGrp.Length - item.Index));
                }

                if (followGrp.Success)
                {
                    sb.Append(PatternFormFollow.Replace(value.Substring(followGrp.Index - item.Index), match => Form(match, type)));
                }

                return sb.ToString();
            }

            //? 查询语句。
            sql = PatternForm.Replace(sql, item => Form(item, "SELECT"));

            //? 参数处理。
            sql = PatternParameter.Replace(sql, item => string.Concat("{", item.Groups["name"].Value, "}"));

            //? 声明变量
            sql = PatternDeclare.Replace(sql, item =>
            {
                string value = string.Concat("--#", temporaryStorages.Count.ToString());

                temporaryStorages.Add(item.Value);

                return value;
            });

            //? 字段和别名处理。
            sql = PatternAliasField.Replace(sql, item =>
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
            sql = PatternSingleArgField.Replace(sql, item =>
            {
                var sb = new StringBuilder();
                Group nameGrp = item.Groups["name"];

                return sb
                    .Append(item.Value.Substring(0, nameGrp.Index - item.Index))
                    .Append("[")
                    .Append(nameGrp.Value)
                    .Append("]")
                    .Append(item.Value.Substring(nameGrp.Index - item.Index + nameGrp.Length))
                    .ToString();
            });

            //? 字段处理。
            sql = PatternField.Replace(sql, item =>
            {
                var sb = new StringBuilder();

                Group nameGrp = item.Groups["name"];

                return sb.Append(item.Value.Substring(0, nameGrp.Index - item.Index))
                        .Append("[")
                        .Append(nameGrp.Value)
                        .Append("]")
                        .Append(item.Value.Substring(nameGrp.Index - item.Index + nameGrp.Length))
                        .ToString();
            });

            //? 字段处理。
            sql = PatternFieldEmbody.Replace(sql, item =>
            {
                Group nameGrp = item.Groups["name"];

                string value = string.Concat("[", nameGrp.Value, "]");

                if (nameGrp.Index == item.Index && nameGrp.Length == item.Length)
                    return value;

                return item.Value.Substring(0, nameGrp.Index - item.Index) + value + item.Value.Substring(nameGrp.Index - item.Index + nameGrp.Length);
            });

            //? 字段别名。
            sql = PatternAsField.Replace(sql, item =>
            {
                Group nameGrp = item.Groups["name"];

                return string.Concat(item.Value.Substring(0, nameGrp.Index - item.Index), "[", nameGrp.Value, "]");
            });

            //? 还原字符
            if (temporaryStorages.Count > 0)
            {
                int execCount = 0;
                int dataCount = temporaryStorages.Count;
                do
                {
                    bool flag = true;
                    sql = PatternTemporaryStorage.Replace(sql, item =>
                    {
                        Group indexGrp = item.Groups["index"];

                        if (int.TryParse(indexGrp.Value, out int index) && dataCount > index)
                        {
                            execCount++;

                            flag = false;

                            return temporaryStorages[index];
                        }

                        return item.Value;
                    });

                    if (flag)
                    {
                        break;
                    }

                } while (execCount < dataCount || PatternTemporaryStorage.IsMatch(sql));
            }

            temporaryStorages = null;

            return sql;
        }

        /// <summary>
        /// 添加语句
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns></returns>
        public SQL Add(string sql) => Add(new SQL(SqlCache.GetOrAdd(sql, MakeSQL)));

        /// <summary>
        /// 添加语句
        /// </summary>
        /// <param name="sql">SQL</param>
        public SQL Add(SQL sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            sb.Append(";")
                .AppendLine()
                .Append(sql.ToString());

            return this;
        }

        /// <summary>
        /// 获取所有表令牌。
        /// </summary>
        /// <param name="match">匹配</param>
        /// <returns></returns>
        private static List<TableToken> GetTableTokens(Match match)
        {
            List<TableToken> tables = new List<TableToken>();

            while (match.Success)
            {
                if (!tables.Exists(token => token.Token == match.Value))
                {
                    tables.Add(new TableToken(match.Value, match.Groups["type"].Value, match.Groups["name"].Value));
                }

                match = match.NextMatch();
            }

            return tables;
        }

        /// <summary>
        /// 获取所有参数令牌。
        /// </summary>
        /// <param name="match">匹配</param>
        /// <returns></returns>
        private static List<ParameterToken> GetParameterTokens(Match match)
        {
            List<ParameterToken> parameters = new List<ParameterToken>();

            while (match.Success)
            {
                if (!parameters.Exists(token => token.Token == match.Value))
                {
                    parameters.Add(new ParameterToken(match.Value, match.Groups["name"].Value));
                }

                match = match.NextMatch();
            }

            return parameters;
        }

#if NET40

        private static readonly ConcurrentDictionary<string, ReadOnlyCollection<TableToken>> TableCache = new ConcurrentDictionary<string, ReadOnlyCollection<TableToken>>();

        private static readonly ConcurrentDictionary<string, ReadOnlyCollection<ParameterToken>> ParameterCache = new ConcurrentDictionary<string, ReadOnlyCollection<ParameterToken>>();

        /// <summary>
        /// 操作的表
        /// </summary>
        public ReadOnlyCollection<TableToken> Tables => TableCache.GetOrAdd(sb.ToString(), sql =>
        {
            Match match = PatternTableToken.Match(sql);

            return match.Success ? new ReadOnlyCollection<TableToken>(GetTableTokens(match)) : TableToken.None;
        });
        /// <summary>
        /// 参数
        /// </summary>
        public ReadOnlyCollection<ParameterToken> Parameters => ParameterCache.GetOrAdd(sb.ToString(), sql =>
        {
            Match match = PatternParameterToken.Match(sql);

            return match.Success ? new ReadOnlyCollection<ParameterToken>(GetParameterTokens(match)) : ParameterToken.None;
        });
#else
        private static readonly ConcurrentDictionary<string, IReadOnlyCollection<TableToken>> TableCache = new ConcurrentDictionary<string, IReadOnlyCollection<TableToken>>();

        private static readonly ConcurrentDictionary<string, IReadOnlyCollection<ParameterToken>> ParameterCache = new ConcurrentDictionary<string, IReadOnlyCollection<ParameterToken>>();

        /// <summary>
        /// 操作的表
        /// </summary>
        public IReadOnlyCollection<TableToken> Tables => TableCache.GetOrAdd(sb.ToString(), sql =>
        {
            Match match = PatternTableToken.Match(sql);

            return match.Success ? GetTableTokens(match) : TableToken.None;
        });

        /// <summary>
        /// 参数
        /// </summary>
        public IReadOnlyCollection<ParameterToken> Parameters => ParameterCache.GetOrAdd(sb.ToString(), sql =>
        {
            Match match = PatternParameterToken.Match(sql);

            return match.Success ? GetParameterTokens(match) : ParameterToken.None;
        });
#endif

        #region Private
        /// <summary>
        /// 分析参数
        /// </summary>
        /// <param name="value">内容</param>
        /// <param name="startIndex">开始位置</param>
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
                            list.Add(value.Substring(dataIndex, commaIndex - dataIndex));

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

            list.Add(value.Substring(dataIndex, rightIndex - dataIndex));

            return Tuple.Create(rightIndex, list.ToArray());
        }

        #endregion

        /// <summary>
        /// 转为实际数据库的SQL语句
        /// </summary>
        /// <param name="settings">SQL修正配置</param>
        /// <returns></returns>
        public string ToString(ISQLCorrectSimSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            string sql = sb.ToString();

            //? 检查 IndexOf 函数，参数处理
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
                     ) + sql.Substring(tuple.Item1);

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

            //? 表名称。
            sql = PatternTableToken.Replace(sql, item => settings.Name(item.Groups["name"].Value));

            //? 字段名称。
            sql = PatternFieldToken.Replace(sql, item => settings.Name(item.Groups["name"].Value));

            //? 参数名称。
            sql = PatternParameterToken.Replace(sql, item => settings.ParamterName(item.Groups["name"].Value));

            foreach (IFormatter formatter in settings.Formatters)
            {
                sql = formatter.RegularExpression.Replace(sql, formatter.Evaluator);
            }

            return sql;
        }

        /// <summary>
        /// 返回分析的SQL结果。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //? 表名称。
            return PatternFieldToken.Replace(PatternTableToken.Replace(sb.ToString(), item => string.Concat("[", item.Groups["name"].Value, "]")), item => string.Concat("[", item.Groups["name"].Value, "]"));
        }

        /// <summary>
        /// 追加sql
        /// </summary>
        /// <param name="left">原SQL</param>
        /// <param name="right">需要追加的SQL</param>
        /// <returns></returns>
        public static SQL operator +(SQL left, SQL right)
        {
            if (left is null || right is null)
            {
                return right ?? left;
            }

            return left.Add(right);
        }

        /// <summary>
        /// 运算符
        /// </summary>
        /// <param name="sql">SQL</param>
        public static implicit operator SQL(string sql) => new SQL(sql);
    }
}
