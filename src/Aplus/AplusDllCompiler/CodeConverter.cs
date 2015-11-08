﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting;

using DLR = System.Linq.Expressions;
using DYN = System.Dynamic;

using AplusCore.Compiler;
using AplusCore.Runtime;
using AplusCore.Types;
using AplusCore.Compiler.AST;
using System.IO;

namespace AplusDllCompiler
{
    public class CodeConverter
    {
        #region fields

        private const string sFilePrefix = "FILE::";

        private LexerMode lexerStartMode;
        private List<string> sourceCodes;

        #endregion

        #region constructor

        public CodeConverter(LexerMode mode)
        {
            this.lexerStartMode = mode;
            this.sourceCodes = new List<string>();
        }

        #endregion

        #region add sources

        public void AddSourceFile(string filePath)
        {
            this.sourceCodes.Add(sFilePrefix + filePath);
        }

        public void AddSource(string source)
        {
            this.sourceCodes.Add(source);
        }

        #endregion

        #region converters

        public List<Node> ParseSources()
        {
            List<Node> trees = new List<Node>();
            FunctionInformation funcionInfo = new FunctionInformation(".");

            foreach (string item in this.sourceCodes)
            {
                if (item.StartsWith(sFilePrefix))
                {
                    string fileName = item.Substring(sFilePrefix.Length);
                    trees.Add(Parse.LoadFile(fileName, lexerStartMode, funcionInfo));
                }
                else
                {
                    trees.Add(Parse.String(item, this.lexerStartMode, funcionInfo));
                }
            }

            return trees;
        }

        public List<DLR.LambdaExpression> GetLambdaMethods(List<Node> nodes)
        {
            List<DLR.LambdaExpression> lambdaMethods = new List<DLR.LambdaExpression>();
            foreach (Node node in nodes)
            {
                List<Node> userMethods = FindUserDefMethods(node);
                List<DLR.LambdaExpression> lambdas = ConvertNodes(userMethods);

                lambdaMethods.AddRange(lambdas);
            }

            return lambdaMethods;
        }


        internal static Node ParseCode(string code)
        {
            FunctionInformation funcionInfo = new FunctionInformation(".");
            return Parse.ASCIIString(code, funcionInfo);
        }

        internal static List<Node> FindUserDefMethods(Node treeTop)
        {
            List<Node> userMethods = new List<Node>();

            // We expect that the top level is an ExpressionList type node
            if (treeTop.NodeType != NodeTypes.ExpressionList)
            {
                return userMethods;
            }

            ExpressionList topExpressions = treeTop as ExpressionList;

            foreach (Node expression in topExpressions.Items)
            {
                if (expression.NodeType == NodeTypes.UserDefFunction)
                {
                    userMethods.Add(expression);
                }
            }

            return userMethods;
        }

        internal static List<DLR.LambdaExpression> ConvertNodes(List<Node> methods)
        {
            Aplus runtime = new Aplus();

            AplusScope scope = new AplusScope(null, "__top__", runtime,
                DLR.Expression.Parameter(typeof(Aplus), "__aplusRuntime__"),
                DLR.Expression.Parameter(typeof(DYN.IDynamicMetaObjectProvider), "__module__")
            );

            List<DLR.LambdaExpression> dlrMethods = new List<DLR.LambdaExpression>();

            foreach (Node userDefMethod in methods)
            {
                DLR.Expression varSet = userDefMethod.Generate(scope);

                DLR.Expression wrappedLambda = ((DLR.MethodCallExpression)((DLR.UnaryExpression)varSet).Operand).Arguments.Last();
                DLR.LambdaExpression lambda = (DLR.LambdaExpression)((DLR.MethodCallExpression)wrappedLambda).Arguments[1];

                List<DLR.ParameterExpression> newParams = new List<DLR.ParameterExpression>();
                newParams.Add(scope.GetRuntimeExpression());
                // We must change the order of the AType arguments to have a correct C# call order
                newParams.AddRange(lambda.Parameters.Skip(1).Reverse());

                DLR.LambdaExpression resultingLambda =
                    DLR.Expression.Lambda(
                        DLR.Expression.Block(
                            new DLR.ParameterExpression[] {
                                scope.GetModuleExpression(),
                                lambda.Parameters.First()
                            },
                            DLR.Expression.Assign(
                                lambda.Parameters.First(),
                                scope.GetRuntimeExpression()
                            ),
                            DLR.Expression.Assign(
                                scope.GetModuleExpression(),
                                DLR.Expression.PropertyOrField(scope.GetRuntimeExpression(), "Context")
                            ),
                            lambda.Body
                        ),
                        lambda.Name,
                        newParams
                    );

                dlrMethods.Add(resultingLambda);
            }

            return dlrMethods;
        }

        #endregion

        #region demo usage

        internal static void Demo()
        {
            string src = @"
                method{c}: { .d:=2; c+1 }
                method2{c; e}: { (c * 0) + e + d }
            ";

            Node tree = ParseCode(src);
            List<Node> methods = FindUserDefMethods(tree);
            List<DLR.LambdaExpression> dlrMethods = ConvertNodes(methods);

            AssemblyCreator asmCreator = new AssemblyCreator("Demo");
            foreach (var item in dlrMethods)
            {
                asmCreator.AddMethod(item.Name.TrimStart('.'), item);
            }

            List<string> names = asmCreator.Build();
            Console.WriteLine("Created '{0}.dll' with the following methods/properties/fields", asmCreator.AssemblyBaseName);
            foreach (string name in names)
            {
                Console.WriteLine("- {0}", name);
            }
            Console.WriteLine("Number of generated entries: {0}", names.Count);

            Console.ReadLine();
        }

        internal static void Demo2()
        {
            string src = @"
                method{c}: { .d:=2; c+1 }
                method2{c; e}: { (c * 0) + e + d }
            ";

            CodeConverter cc = new CodeConverter(LexerMode.ASCII);

            cc.AddSource(src);
            List<Node> nodes = cc.ParseSources();
            List<DLR.LambdaExpression> lambdas = cc.GetLambdaMethods(nodes);

            AssemblyCreator asmCreator = new AssemblyCreator("Demo");
            foreach (DLR.LambdaExpression item in lambdas)
            {
                asmCreator.AddMethod(item.Name.TrimStart('.'), item);
            }

            List<string> names = asmCreator.Build();
            Console.WriteLine("Created '{0}.dll' with the following methods/properties/fields", asmCreator.AssemblyBaseName);
            foreach (string name in names)
            {
                Console.WriteLine("- {0}", name);
            }
            Console.WriteLine("Number of generated entries: {0}", names.Count);

            Console.ReadLine();
        }

        #endregion
    }
}
