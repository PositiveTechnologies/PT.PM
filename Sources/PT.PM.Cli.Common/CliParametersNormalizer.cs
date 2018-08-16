using CommandLine;
using PT.PM.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PT.PM.Cli.Common
{
    public class CliParametersNormalizer<TParameters> : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool CheckTypes { get; set; } = true;

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

                    argInd++;

                    string outValue = argInd < args.Length ? args[argInd] : null;

                    Type underlyingType = Nullable.GetUnderlyingType(foundOption.Type);

                    bool isNullable;
                    Type notNullableType;
                    if (underlyingType != null)
                    {
                        isNullable = true;
                        notNullableType = underlyingType;
                    }
                    else
                    {
                        isNullable = false;
                        notNullableType = foundOption.Type;
                    }

                    if (notNullableType == typeof(bool) && (argInd == args.Length || outValue.StartsWith("-")))
                    {
                        outArgs.Add(outArg);
                        if (isNullable)
                        {
                            outArgs.Add(true.ToString().ToLowerInvariant());
                        }
                    }
                    else
                    {
                        bool success = CheckAndAddIfParsed(outArgs, outArg, outValue, notNullableType, isNullable);
                        if (!success)
                        {
                            error = true;
                        }
                        argInd++;
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

        private bool CheckAndAddIfParsed(List<string> outArgs, string outArg, string outValue, Type type, bool nullable)
        {
            if (type == typeof(int))
            {
                return AddArgValueIfSuccess(int.TryParse(outValue, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(uint))
            {
                return AddArgValueIfSuccess(uint.TryParse(outValue, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(byte))
            {
                return AddArgValueIfSuccess(byte.TryParse(outValue, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(sbyte))
            {
                return AddArgValueIfSuccess(sbyte.TryParse(outValue, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(short))
            {
                return AddArgValueIfSuccess(short.TryParse(outValue, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(ushort))
            {
                return AddArgValueIfSuccess(ushort.TryParse(outValue, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(long))
            {
                return AddArgValueIfSuccess(long.TryParse(outValue, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(ulong))
            {
                return AddArgValueIfSuccess(ulong.TryParse(outValue, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(float))
            {
                return AddArgValueIfSuccess(float.TryParse(outValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(double))
            {
                return AddArgValueIfSuccess(double.TryParse(outValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(decimal))
            {
                return AddArgValueIfSuccess(decimal.TryParse(outValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var _), outArgs, outArg, outValue);
            }
            else if (type == typeof(bool))
            {
                bool b = true;
                if (!CheckTypes || bool.TryParse(outValue, out b))
                {
                    if (b)
                    {
                        outArgs.Add(outArg);
                        if (nullable)
                        {
                            outArgs.Add(outValue);
                        }
                    }
                    return true;
                }
                else
                {
                    Logger.LogError(new FormatException($"Incorrect value {outValue} of argument {outArg}"));
                    return false;
                }
            }
            else if (type.BaseType == typeof(Enum))
            {
                bool success = true;
                try
                {
                    Enum.Parse(type, outValue, true);
                }
                catch
                {
                    success = false;
                }
                return AddArgValueIfSuccess(success, outArgs, outArg, outValue);
            }

            return AddArgValueIfSuccess(true, outArgs, outArg, outValue);
        }

        private bool AddArgValueIfSuccess(bool success, List<string> outArgs, string outArg, string outValue)
        {
            if (success || !CheckTypes)
            {
                outArgs.Add(outArg);
                outArgs.Add(outValue);
                return true;
            }
            else
            {
                Logger.LogError(new FormatException($"Incorrect value `{outValue}` of argument {outArg}"));
                return false;
            }
        }
    }
}
