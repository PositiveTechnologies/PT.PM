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
        private static List<OptionType> optionTypes;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool CheckTypes { get; set; } = true;

        public bool CheckDuplicates { get; set; } = true;

        static CliParametersNormalizer()
        {
            PropertyInfo[] paramProps = typeof(TParameters).GetProperties();

            optionTypes = new List<OptionType>();
            foreach (PropertyInfo prop in paramProps)
            {
                IEnumerable<OptionAttribute> optionAttrs = prop.GetCustomAttributes<OptionAttribute>(true);
                foreach (OptionAttribute optionAttr in optionAttrs)
                {
                    optionTypes.Add(new OptionType(optionAttr, prop));
                }
            }
        }

        public bool Normalize(string[] args, out string[] outArgs)
        {
            bool error = false;
            var outArgValues = new List<Tuple<string, string>>(args.Length);

            int argInd = 0;
            while (argInd < args.Length)
            {
                if (!args[argInd].StartsWith("-"))
                {
                    error = true;
                    Logger.LogError(new ArgumentException($"Incorrect argument", args[argInd]));

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
                        bool success = CheckDuplicatesAndAdd(outArgValues, outArg, isNullable ? true.ToString().ToLowerInvariant() : null);
                        if (!success)
                        {
                            error = true;
                        }
                    }
                    else
                    {
                        bool success = CheckAndAddIfParsed(outArgValues, outArg, outValue, notNullableType, isNullable);
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
                    Logger.LogError(new ArgumentException($"Unknown argument", args[argInd]));

                    argInd++;
                }
            }

            var outArgsList = new List<string>();
            foreach (var argValue in outArgValues)
            {
                outArgsList.Add(argValue.Item1);

                if (argValue.Item2 != null)
                {
                    outArgsList.Add(argValue.Item2);
                }
            }

            outArgs = outArgsList.ToArray();

            return !error;
        }

        private bool CheckAndAddIfParsed(List<Tuple<string, string>> outArgs, string outArg, string outValue, Type type, bool nullable)
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
                    if (nullable)
                    {
                        return CheckDuplicatesAndAdd(outArgs, outArg, outValue);
                    }
                    else if (b)
                    {
                        return CheckDuplicatesAndAdd(outArgs, outArg, null);
                    }

                    Tuple<string, string> duplicate;
                    if (CheckDuplicates && (duplicate = outArgs.FirstOrDefault(outArg2 => outArg2.Item1 == outArg)) != default)
                    {
                        Logger.LogError(new ArgumentException("Duplicate argument", outArg));
                        outArgs.Remove(duplicate);
                        return false;
                    }
                    return true;
                }
                else
                {
                    Logger.LogError(new FormatException($"Incorrect value `{outValue}` of argument `{outArg}`"));
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

        private bool AddArgValueIfSuccess(bool success, List<Tuple<string, string>> outArgs, string outArg, string outValue)
        {
            if (success || !CheckTypes)
            {
                return CheckDuplicatesAndAdd(outArgs, outArg, outValue);
            }
            else
            {
                Logger.LogError(new FormatException($"Incorrect value `{outValue}` of argument {outArg}"));
                return false;
            }
        }

        private bool CheckDuplicatesAndAdd(List<Tuple<string, string>> outArgs, string arg, string value)
        {
            bool result = true;
            Tuple<string, string> duplicate;

            if (CheckDuplicates && (duplicate = outArgs.FirstOrDefault(outArg => outArg.Item1 == arg)) != default)
            {
                Logger.LogError(new ArgumentException("Duplicate argument", arg));
                outArgs.Remove(duplicate);
                result = false;
            }

            outArgs.Add(new Tuple<string, string>(arg, value));
            return result;
        }
    }
}
