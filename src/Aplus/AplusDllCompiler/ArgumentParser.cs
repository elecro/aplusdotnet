using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AplusDllCompiler
{
    public class ArgumentParser
    {
        #region internal option class

        class Option
        {
            internal string Name { get; private set; }
            internal bool HasArg { get; private set; }
            internal string Help { get; private set; }
            internal string[] Choices { get; private set; }
            internal bool HasChoices { get { return this.Choices != null; } }

            internal Option(string name, bool hasArg, string help)
            {
                this.Name = name;
                this.HasArg = hasArg;
                this.Help = help;
            }

            internal Option(string name, bool hasArg, string help, string[] choices)
                : this(name, hasArg, help)
            {
                this.Choices = choices;
            }
        }

        #endregion

        #region fields

        private Dictionary<string, Option> options;

        #endregion

        #region constructor

        public ArgumentParser()
        {
            options = new Dictionary<string, Option>();
        }

        #endregion

        #region argument handling

        public void AddArgument(string name, string help, bool hasArg)
        {
            options[name] = new Option(name, hasArg, help);
        }

        public void AddArgument(string name, string help, string[] choices)
        {
            options[name] = new Option(name, true, help, choices);
        }

        public void WriteHelp(TextWriter output)
        {
            output.WriteLine("Allowed options:");
            foreach (KeyValuePair<string, Option> item in this.options)
            {
                string optionName = item.Value.Name;

                if (item.Value.HasArg)
                {
                    optionName += " <arg>";
                }

                output.WriteLine(" --{0,-15} {1,-30}", optionName, item.Value.Help);
            }
        }

        public void WriteHelp()
        {
            WriteHelp(Console.Out);
        }

        #endregion

        #region argument parsing

        public ParsedArguments Parse(string[] args)
        {
            ParsedArguments parsed = new ParsedArguments();
            
            for (int i = 0; i < args.Length; i++)
            {
                string optionName;
                string optionValue;

                if (!TryOptionConvert(args[i], out optionName, out optionValue))
                {
                    // We weren't able to use the argument as an option
                    // Place it into the leftover list.
                    parsed.Arguments.Add(args[i]);
                    continue;
                }

                Option option;
                if (!this.options.TryGetValue(optionName, out option))
                {
                    // We don't have an option named like that
                    // Place it into the leftover list.
                    // TODO: Maybe we should throw an exception instead?
                    parsed.Arguments.Add(args[i]);
                    continue;
                }

                if (option.HasArg)
                {
                    if (parsed.Options.ContainsKey(option.Name))
                    {
                        Console.Error.WriteLine("Option '{0}' already specified with value '{1}'", option.Name, parsed.Options[option.Name]);
                        Environment.Exit(2);
                    }

                    if (optionValue.Length == 0)
                    {
                        // We have a normal '--optionName arg' case, this needs a bit of work
                        if (i + 1 >= args.Length)
                        {
                            Console.Error.WriteLine("Missing argument for option '{0}'", option.Name);
                            WriteHelp();
                            Environment.Exit(1);
                        }

                        optionValue = args[i + 1];
                        i++;
                    }

                    if (option.HasChoices && !option.Choices.Contains(optionValue))
                    {
                        Console.Error.WriteLine("Incorrect argument for '{0}' option", option.Name);
                        Console.Error.WriteLine("The allowed ones are: {0}", string.Join(", ", option.Choices));
                        Environment.Exit(3);
                    }

                    parsed.Options.Add(option.Name, optionValue);
                }
                else
                {
                    parsed.Options.Add(option.Name, "");
                }
            }

            return parsed;
        }

        private static bool TryOptionConvert(string arg, out string optionName, out string optionValue)
        {
            if (arg.StartsWith("-"))
            {
                string entry = arg.Remove(0, 1);
                if (entry.StartsWith("-"))
                {
                    entry = entry.Remove(0, 1);
                }

                string[] parts = entry.Split(new char[] { '=' }, 2);
                optionName = parts[0];

                if (parts.Length > 1)
                {
                    optionValue = parts[1];
                }
                else
                {
                    optionValue = "";
                }

                return true;
            }

            optionName = "";
            optionValue = "";
            return false;
        }

        #endregion
    }

    public class ParsedArguments
    {
        #region properties

        public Dictionary<string, string> Options { get; private set; }
        public List<string> Arguments { get; private set; }

        #endregion

        #region constructor

        public ParsedArguments()
        {
            this.Options = new Dictionary<string, string>();
            this.Arguments = new List<string>();
        }

        #endregion
    }
}
