﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AplusCore.Compiler.Grammar;
using DLR = System.Linq.Expressions;
using AplusCore.Runtime;
using AplusCore.Types;
using System.Dynamic;


namespace AplusCore.Compiler.AST
{
    public class MonadicDo : Node
    {
        #region Variables

        private Node codeblock;

        #endregion

        #region Constructor

        public MonadicDo(Node codeblock)
        {
            this.codeblock = codeblock;
        }

        #endregion

        #region DLR Generator

        public override DLR.Expression Generate(AplusScope scope)
        {
            // TODO: Add usage of Protected Execute Flag

            // Save the previous return target
            DLR.LabelTarget oldTarget = scope.ReturnTarget;
            scope.ReturnTarget = DLR.Expression.Label(typeof(AType), "EXIT");

            DLR.Expression protectedCode = DLR.Expression.Label(
                scope.ReturnTarget,
                this.codeblock.Generate(scope)
            );

            // Restore the return target;
            scope.ReturnTarget = oldTarget;

            // Code block contining the strandard execution's result
            // wrapped in a strand
            DLR.Expression block =
                DLR.Expression.Call(
                    typeof(Runtime.Helpers).GetMethod("BuildStrand"),
                    DLR.Expression.NewArrayInit(
                        typeof(AType),
                // We need to pass in reverse order
                        protectedCode,
                        DLR.Expression.Constant(AInteger.Create(0), typeof(AType))
                    )
                );

            // Parameter for Catch block
            DLR.ParameterExpression errorVariable = DLR.Expression.Parameter(typeof(Error), "error");

            // Catch block, returns the ([errorcode]; [errortext]) strand
            DLR.CatchBlock catchBlock = DLR.Expression.Catch(
                errorVariable,
                DLR.Expression.Call(
                    typeof(Runtime.Helpers).GetMethod("BuildStrand"),
                    DLR.Expression.NewArrayInit(
                        typeof(AType),
                // We need to pass in reverse order
                // Error Text
                        DLR.Expression.Call(
                            typeof(Runtime.Helpers).GetMethod("BuildString"),
                            DLR.Expression.Property(errorVariable, "ErrorText")
                        ),
                // Error Code
                        DLR.Expression.Call(
                            typeof(AInteger).GetMethod("Create", new Type[] { typeof(int) }),
                            DLR.Expression.Convert(
                                DLR.Expression.Property(errorVariable, "ErrorType"),
                                typeof(int)
                            )
                        )
                    )
                )
            );

            DLR.Expression result = DLR.Expression.TryCatch(
                block,
                catchBlock 
            );

            return result;
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return String.Format("Do({0})", this.codeblock);
        }

        public override bool Equals(object obj)
        {
            if (obj is MonadicDo)
            {
                MonadicDo other = (MonadicDo)obj;
                return this.codeblock == other.codeblock;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.codeblock.GetHashCode();
        }

        #endregion

        #region GraphViz output (Only in DEBUG)
#if DEBUG
        private static int counter = 0;
        internal override string ToDot(string parent, StringBuilder textBuilder)
        {
            string name = String.Format("MonadicDo{0}", counter++);
            textBuilder.AppendFormat("  subgraph cluster_{0}_block {{ style=dotted; color=black; label=\"Protected Block\";\n", name);
            string codeBlockName = this.codeblock.ToDot(name, textBuilder);
            textBuilder.AppendFormat("  }}\n");

            textBuilder.AppendFormat("  {0} [label=\"DO\"];\n", name);
            textBuilder.AppendFormat("  {0} -> {1};\n", name, codeBlockName);


            return name;
        }
#endif
        #endregion

    }

    #region Construction helper

    public partial class Node
    {
        public static MonadicDo MonadicDo(Node codeblock)
        {
            return new MonadicDo(codeblock);
        }
    }

    #endregion
}
