﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;

namespace CodeArts.Emit.Expressions
{
    /// <summary>
    /// 捕获异常。
    /// </summary>
    [DebuggerDisplay("try { {body} }")]
    public class TryAst : BlockAst
    {
        private readonly List<CatchAst> catchAsts = new List<CatchAst>();
        private readonly List<FinallyAst> finallyAsts = new List<FinallyAst>();

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="returnType">返回结果。</param>
        public TryAst(Type returnType) : base(returnType)
        {
        }

        /// <summary>
        /// 添加代码。
        /// </summary>
        /// <param name="code">代码。</param>
        /// <returns></returns>
        public override BlockAst Append(AstExpression code)
        {
            if (code is CatchAst catchAst)
            {
                if (ReturnType.IsAssignableFrom(catchAst.ReturnType) || typeof(Exception).IsAssignableFrom(catchAst.ReturnType))
                {
                    catchAsts.Add(catchAst);
                }
                else
                {
                    throw new ArgumentException("捕获器只能返回相同类型或抛出异常!", nameof(code));
                }

                return this;
            }

            if (code is FinallyAst finallyAst)
            {
                finallyAsts.Add(finallyAst);

                return this;
            }

            return base.Append(code);
        }

        /// <summary>
        /// 生成。
        /// </summary>
        /// <param name="ilg">指令。</param>
        public override void Load(ILGenerator ilg)
        {
            if (catchAsts.Count == 0 && finallyAsts.Count == 0)
            {
                throw new AstException("表达式残缺，未设置捕获代码块或最终执行代码块！");
            }

            ilg.BeginExceptionBlock();

            base.Load(ilg);

            if (HasReturn)
            {
                throw new AstException("表达式会将结果推到堆上，不能写返回！");
            }

            if (ReturnType == typeof(void))
            {
                if (catchAsts.Count > 0)
                {
                    foreach (var item in catchAsts)
                    {
                        item.Load(ilg);
                    }
                }

                if (finallyAsts.Count > 0)
                {
                    ilg.BeginFinallyBlock();

                    ilg.Emit(OpCodes.Nop);

                    foreach (var item in finallyAsts)
                    {
                        item.Load(ilg);
                    }

                    ilg.Emit(OpCodes.Nop);
                }

                ilg.EndExceptionBlock();
            }
            else
            {
                var variable = ilg.DeclareLocal(ReturnType);

                ilg.Emit(OpCodes.Stloc, variable);

                if (catchAsts.Count > 0)
                {
                    ilg.Emit(OpCodes.Nop);

                    foreach (var item in catchAsts)
                    {
                        item.Load(ilg);

                        if (ReturnType.IsAssignableFrom(item.ReturnType))
                        {
                            ilg.Emit(OpCodes.Stloc, variable);
                        }
                    }

                    ilg.Emit(OpCodes.Nop);
                }

                if (finallyAsts.Count > 0)
                {
                    ilg.BeginFinallyBlock();

                    ilg.Emit(OpCodes.Nop);

                    foreach (var item in finallyAsts)
                    {
                        item.Load(ilg);
                    }

                    ilg.Emit(OpCodes.Nop);
                }

                ilg.EndExceptionBlock();

                ilg.Emit(OpCodes.Ldloc, variable);
            }
        }
    }
}
