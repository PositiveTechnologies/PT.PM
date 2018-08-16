using CommandLine;
using PT.PM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PT.PM.Cli.Common
{
    public class CliParametersNormalizer<TParameters> : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool Normalize(string[] args, out List<string> outArgs)
        {
            bool error = false;
            outArgs = new List<string>(args.Length);
            PropertyInfo[] paramProps = typeof(TParameters).GetProperties();

            var optionTypes = new List<OptionType>();
            foreach (PropertyInfo prop in paramProps)
            {
                IEnumerable<OptionAttribute> optionAttrs = prop.GetCustomAttributes<OptionAttribute>(true);
                foreach (OptionAttribute optionAttr in optionAttrs)
                {
                    optionTypes.Add(new OptionType(optionAttr, prop));
                }
            }

            int argInd = 0;
            while (argInd < args.Length)
            {
                if (!args[argInd].StartsWith("-"))
                {
                    error = true;
                    Logger.LogError(new ArgumentException($"Incorrect argument {args[argInd]}", args[argInd]));

                    argInd++;
                    continue;
                }

                string trimmedArg = args[argInd].TrimStart('-');

                OptionType foundOption = optionTypes.FirstOrDefault(optionType =>
                    optionType.Option.ShortName == trimmedArg || optionType.Option.LongName == trimmedArg ||
                    optionType.ParamName == trimmedArg);

                if (foundOption != null)
                {
                    string outArg = foundOption.Option.ShortName == trimmedArg
                        ? "-" + foundOption.Option.ShortName
                        : foundOption.Option.LongName == trimmedArg
                        ? "--" + foundOption.Option.LongName
                        : "--" + foundOption.ParamName;
                    outArgs.Add(outArg);

                    argInd++;

                    if (foundOption.Type == typeof(bool?) &&
                        (argInd == args.Length || !bool.TryParse(args[argInd], out bool _)))
                    {
                        outArgs.Add(true.ToString().ToLowerInvariant());
                    }
                    else
                    {
                        if (argInd < args.Length)
                        {
                            outArgs.Add(args[argInd]);
                            argInd++;
                        }
                    }
                }
                else
                {
                    error = true;
                    Logger.LogError(new ArgumentException($"Unknown argument {args[argInd]}", args[argInd]));

                    argInd++;
                }
            }

            return !error;
        }
    }
}
