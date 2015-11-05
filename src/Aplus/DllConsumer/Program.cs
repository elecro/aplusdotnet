using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AplusCore.Runtime;
using Microsoft.Scripting.Runtime;
using AplusCore.Compiler;
using AplusCore.Types;
using Microsoft.Scripting;

namespace DllConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Scope scope = new Scope();
            scope.Storage["."] = new ScopeStorage();
            scope.Storage["."]["a"] = AInteger.Create(103);

            Aplus runtime = new Aplus(scope, LexerMode.ASCII);
            runtime.Context = scope;
            AType result = AplusGenerated.test(runtime);

            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
