using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AplusCore.Compiler;
using AplusCore.Runtime;
using Microsoft.Scripting.Runtime;
using AplusCore.Types;

using DLR = System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting;

namespace DllBuilder
{
    class Program
    {

        static void BuildDll(string name, DLR.Expression<Func<Aplus, AType>> lambda, Aplus runtime)
        {
            Type[] types = lambda.Parameters.Select(param => param.Type).ToArray();

            AssemblyName asmName = new AssemblyName(name + "Generated");
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(name + "Module", name + "Module.dll", true);
            TypeBuilder typeBuilder = moduleBuilder.DefineType("AplusGenerated", TypeAttributes.Public);
            MethodBuilder methodBuilder = typeBuilder.DefineMethod("test", MethodAttributes.Public | MethodAttributes.Static,
                lambda.ReturnType, types);

            lambda.CompileToMethod(methodBuilder);

            Type t = typeBuilder.CreateType();
            asmBuilder.Save(name + "Generated.dll");

            object result = t.GetMethod("test").Invoke(null, new object[] { runtime });

            Console.WriteLine(result);
        }

        static void Main(string[] args)
        {
            Scope scope = new Scope();
            scope.Storage["."] = new ScopeStorage();
            scope.Storage["."]["a"] = AInteger.Create(109);
            Aplus runtime = new Aplus(scope, LexerMode.ASCII);
            runtime.Context = scope;

            string src = @"
                method{c}: c+1

                method{a}";

            DLR.Expression<Func<Aplus, AType>> lambda = AplusScriptCode.ParseToLambda(runtime, src);

            BuildDll("Sum", lambda, runtime);

            Console.ReadLine();
        }
    }
}
