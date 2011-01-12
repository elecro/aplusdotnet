﻿using System;
using System.Text;
using System.Collections.Generic;
using DLR = System.Linq.Expressions;
using AplusCore.Runtime;
using AplusCore.Types;
using AplusCore.Compiler.Grammar;

namespace AplusCore.Compiler.AST
{
    public class DyadicDo : Node
    {
        #region Variables

        private Node expression;
        private Node codeblock;

        #endregion

        #region Constructor

        public DyadicDo(Node expression, Node codeblock)
        {
            this.expression = expression;
            this.codeblock = codeblock;
        }

        #endregion

        #region DLR Generator

        public override DLR.Expression Generate(AplusScope scope)
        {
            Aplus runtime = scope.GetRuntime();

            LinkedList<DLR.Expression> result = new LinkedList<DLR.Expression>();

            DLR.ParameterExpression scalar = DLR.Expression.Parameter(typeof(AType), "_ScalarResult_");
            DLR.ParameterExpression counter = DLR.Expression.Parameter(typeof(int), "COUNTER");
            DLR.ParameterExpression exitValue = DLR.Expression.Parameter(typeof(int), "EXITVALUE");
            
            DLR.LabelTarget exitLabel = DLR.Expression.Label(typeof(AType), "EXIT");
            DLR.ParameterExpression returnValue = DLR.Expression.Parameter(typeof(AType), "RETURN");

            bool incrementMode = true;
            if (this.expression is MonadicFunction &&
                ((MonadicFunction)this.expression).TokenType == Tokens.EXPONENTIAL)
            {
                // Change the counter's 'way'
                incrementMode = false;
                // Remove the Exponential function
                this.expression = ((MonadicFunction)this.expression).Expression;
            }

            if (this.expression is Assign && ((Assign)this.expression).Target is Identifier)
            {
                result.AddFirst(this.expression.Generate(scope));
                // Remove the assignment and leave the identifier only
                this.expression = ((Assign)this.expression).Target;
            }

            // Save the previous return target
            DLR.LabelTarget oldTarget = scope.ReturnTarget;

            // Add an return target for the scope
            // this will allow the usage of the Result monadic function
            scope.ReturnTarget = exitLabel;

            if (this.expression is Identifier)
            {
                // Found a case like: VAR do { ... }

                Identifier variable = (Identifier)this.expression;
                // Generate a .Dynamic.Get DLR tree (used multiple times so this saves time)
                DLR.Expression variableGenerated = variable.Generate(scope);
                DLR.Expression variableAsFloat = DLR.Expression.Property(scalar, "asFloat");

                result.AddLast(DLR.Expression.Block(
                    new DLR.ParameterExpression[] { counter, exitValue, returnValue, scalar },
                    // Test if the constant is an integer
                    DomainTest(variableGenerated, scalar),

                    DLR.Expression.Assign(exitValue,
                        (incrementMode ?
                    // EXITVALUE = round(variable.asFloat)
                        (DLR.Expression)FloatRounding(variableAsFloat) :
                    // EXITVALUE = 0
                        (DLR.Expression)DLR.Expression.Constant(0, typeof(int))
                        )
                    ),
                    DLR.Expression.Assign(
                        counter,
                        (incrementMode ?
                        (DLR.Expression)DLR.Expression.Constant(0, typeof(int)) :
                        (DLR.Expression)DLR.Expression.Decrement(FloatRounding(variableAsFloat))
                        )
                    ),

                    // Start the loop
                    DLR.Expression.Loop(
                        DLR.Expression.Block(

                    // set the variable to the counter     
                            AST.Assign.GenerateIdentifierAssign(
                                scope,
                                variable,
                                DLR.Expression.Call(
                                    typeof(AInteger).GetMethod("Create", new Type[] { typeof(int) }),
                                    counter
                                )
                            ),

                            DLR.Expression.IfThen(
                                (incrementMode ?
                    // Check if  variable >= EXITVALUE  is true
                                    DLR.Expression.GreaterThanOrEqual(
                                        counter,
                    //DLR.Expression.Property(variableGenerated, "asInteger"),
                                        exitValue
                                    ) :
                    // Check if  EXITVALUE(0) > variable  is true
                                    DLR.Expression.GreaterThan(
                                        exitValue,
                    //DLR.Expression.Property(variableGenerated, "asInteger")
                                        counter
                                    )
                                ),
                    // The expression was true, exit from the loop with the last value of the expression block
                                DLR.Expression.Break(exitLabel, returnValue)
                            ),
                    
                    // Otherwise run the inner codeblock
                            DLR.Expression.Assign(returnValue, this.codeblock.Generate(scope)),

                    // Update counter
                            (incrementMode ?
                    // ++counter
                                DLR.Expression.PreIncrementAssign(counter) :
                    // --counter
                                DLR.Expression.PreDecrementAssign(counter)
                            )

                        ),
                        exitLabel
                    )
                ));

            }
            else
            {

                // Simple Iteration
                DLR.ParameterExpression temp = DLR.Expression.Parameter(typeof(AType), "TMP");

                result.AddLast(DLR.Expression.Block(
                    new DLR.ParameterExpression[] { temp, counter, exitValue, returnValue, scalar },
                    // Save the iteration count into a temporaly variable
                    DLR.Expression.Assign(temp, this.expression.Generate(scope)),
                    // Test if the constant is an integer
                    DomainTest(temp, scalar),
                    // MAXVALUE = temp.asInteger
                    DLR.Expression.Assign(exitValue, FloatRounding(DLR.Expression.Property(scalar, "asFloat"))),
                    // counter = 0
                    DLR.Expression.Assign(counter, DLR.Expression.Constant(0, typeof(int))),
                    // Start the loop
                    DLR.Expression.Loop(
                        DLR.Expression.Block(
                    // Check if  counter >= MAXVALUE  is true
                            DLR.Expression.IfThen(
                                DLR.Expression.GreaterThanOrEqual(counter, exitValue),
                    // The expression was true, exit from the loop with the last calculated value
                                DLR.Expression.Break(exitLabel, returnValue)
                            ),
                    // Otherwise run the inner codeblock, save the block's result
                            DLR.Expression.Assign(returnValue, this.codeblock.Generate(scope)),
                    // Increment the counter
                            DLR.Expression.PreIncrementAssign(counter)
                        ),
                        exitLabel
                    )
                ));
            }

            // Restore the return target
            scope.ReturnTarget = oldTarget;

            return DLR.Expression.Block(result);
        }

        private DLR.Expression FloatRounding(DLR.Expression number)
        {
            return DLR.Expression.Convert(
                    DLR.Expression.Call(
                        typeof(Math).GetMethod("Round", new Type[] { typeof(double) }),
                        number
                    ),
                    typeof(int)
            );
        }

        private DLR.Expression DomainTest(DLR.Expression value, DLR.ParameterExpression scalar)
        {
            DLR.Expression test =
                DLR.Expression.Call(value, typeof(AType).GetMethod("TryFirstScalar"),
                    scalar, DLR.Expression.Constant(true)
                );

            DLR.Expression block = 
                DLR.Expression.IfThen(
                    DLR.Expression.OrElse(
                        // Test and get item from the array
                        DLR.Expression.IsFalse(test),
                        // Test if the scalar is a tolerably whole number
                        DLR.Expression.Not(DLR.Expression.Property(scalar, "IsTolerablyWholeNumber"))
                    ),
                    DLR.Expression.Throw(
                        DLR.Expression.New(
                            typeof(Error.Domain).GetConstructor(new Type[] { typeof(string) }),
                            DLR.Expression.Constant("DO", typeof(string))
                        ),
                        typeof(Error.Domain)
                    )
            );

            return block;
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return String.Format("Do({0} {1})", this.expression, this.codeblock);
        }

        public override bool Equals(object obj)
        {
            if (obj is DyadicDo)
            {
                DyadicDo other = (DyadicDo)obj;
                return (this.expression == other.expression) && (this.codeblock == other.codeblock);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.expression.GetHashCode() ^ this.codeblock.GetHashCode();
        }

        #endregion

        #region GraphViz output (Only in DEBUG)
#if DEBUG
        private static int counter = 0;
        internal override string ToDot(string parent, StringBuilder textBuilder)
        {
            string name = String.Format("DyadicDo{0}", counter++);
            textBuilder.AppendFormat("  subgraph cluster_{0}_cond {{ style=dotted; color=blue; label=\"Condition\";\n", name);
            string exprName = this.expression.ToDot(name, textBuilder);
            textBuilder.AppendFormat("  }}\n");

            textBuilder.AppendFormat("  subgraph cluster_{0}_code {{ style=dotted; color=black; label=\"Code Block\";\n", name);
            string codeBlockName = this.codeblock.ToDot(name, textBuilder);
            textBuilder.AppendFormat("  }}\n");

            textBuilder.AppendFormat("  {0} [label=\"DO\"];\n", name);
            textBuilder.AppendFormat("  {0} -> {1};\n", name, exprName);
            textBuilder.AppendFormat("  {0} -> {1};\n", name, codeBlockName);


            return name;
        }
#endif
        #endregion

    }

    #region Construction helper

    public partial class Node
    {
        public static DyadicDo DyadicDo(Node expression, Node codeblock)
        {
            return new DyadicDo(expression, codeblock);
        }
    }

    #endregion
}