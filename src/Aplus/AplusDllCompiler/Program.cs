using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using AplusCore.Compiler;
using AplusCore.Compiler.AST;

using DLR = System.Linq.Expressions;

namespace AplusDllCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup the program arguments
            ArgumentParser argParser = new ArgumentParser();
            argParser.AddArgument("class", "Base name of the generated class", true);
            argParser.AddArgument("mode", "Base input mode for the A+ parser", new string[] { "apl", "ascii", "uni" });
            argParser.AddArgument("help", "Display the help (duh..)", false);

            // Parse all args
            ParsedArguments parsed = argParser.Parse(args);

            // Do a bit of preprocess on the argumnets
            if (parsed.Options.ContainsKey("help")
                || (parsed.Options.Count == 0 && parsed.Arguments.Count == 0))
            {
                string appName = System.AppDomain.CurrentDomain.FriendlyName;

                Console.WriteLine("A+ to DLL Compiler v1.0");
                Console.WriteLine();
                Console.WriteLine("This tool converts A+ source scripts into DLLs which can be used from .NET");
                Console.WriteLine();
                Console.WriteLine("Usage: {0} --class=<arg> file1.a+ [file2.a+ ...]", appName);

                argParser.WriteHelp();
                Environment.Exit(1);
                return;
            }

            // Without a class name we can't build any classes now are we? :)
            if (!parsed.Options.ContainsKey("class"))
            {
                Console.Error.WriteLine("No 'class' option specified, please see help");
                Environment.Exit(1);
                return;
            }

            if (parsed.Arguments.Count == 0)
            {
                Console.Error.WriteLine("No file specified to compile to DLL");
                Environment.Exit(1);
                return;
            }

            // By default the 'ascii' mode is set if there is none specified
            if (!parsed.Options.ContainsKey("mode"))
            {
                parsed.Options["mode"] = "ascii";
            }

            int invalidFileCount = 0;
            foreach (string fileName in parsed.Arguments)
            {
                if (!File.Exists(fileName))
                {
                    Console.Error.WriteLine("File does not exists: {0}", fileName);
                    invalidFileCount++;
                }
            }

            if (invalidFileCount > 0)
            {
                Console.Error.WriteLine("Found non existent input file(s), exiting");
                Environment.Exit(invalidFileCount);
                return;
            }

            DateTime start = DateTime.UtcNow;
            // Start the code converting, first to AST,
            // then to LambdaExpressions
            // and for last to Types & DLL
            CodeConverter codeConverter = new CodeConverter(ToLexerMode(parsed.Options["mode"]));

            foreach (string fileName in parsed.Arguments)
            {
                codeConverter.AddSourceFile(fileName);
            }

            List<Node> nodes;
            try
            {
                nodes = codeConverter.ParseSources();
            }
	        catch (ParseException e)
            {
                Console.WriteLine("Parser error:");
                Console.WriteLine(e.Message);
                Environment.Exit(1);
                return;
	        }

            List<DLR.LambdaExpression> lambdas = codeConverter.GetLambdaMethods(nodes);

            // Now we add all of the methods to the assembly builder
            AssemblyCreator asmCreator = new AssemblyCreator(parsed.Options["class"]);
            foreach (DLR.LambdaExpression item in lambdas)
            {
                asmCreator.AddMethod(item.Name.TrimStart('.').Replace('.', '_'), item);
            }

            List<string> signatures = asmCreator.Build();

            // Finally print out some infos
            Console.WriteLine("Created '{0}.dll' assembly and '{1}.dll' module", asmCreator.AssemblyName, asmCreator.ModuleName);
            Console.WriteLine("The following methods/fields/properties were created:");
            foreach (string sig in signatures)
            {
                Console.WriteLine(" {0}", sig);
            }

            double runtime = (DateTime.UtcNow - start).TotalMilliseconds;
            Console.WriteLine("Number of generated elements: {0} in {1:0.0} ms", signatures.Count, runtime);
        }

        private static LexerMode ToLexerMode(string mode)
        {
            switch (mode)
            {
                case "apl": return LexerMode.APL;
                case "ascii": return LexerMode.ASCII;
                case "uni": return LexerMode.UNI;
                default:
                    break;
            }

            return LexerMode.ASCII;
        }
    }
}
