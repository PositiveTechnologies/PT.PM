using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PT.PM.Cli.Common
{
    public class CliParametersParser<TParameters>
        where TParameters : new()
    {
        private static readonly List<OptionProperty> optionsProperties;

        private HashSet<OptionProperty> visitedArgs;

        public bool CheckDuplicates { get; set; } = true;

        public bool IgnoreCase { get; set; } = true;

        static CliParametersParser()
        {
            PropertyInfo[] paramProps = typeof(TParameters).GetProperties();

            optionsProperties = new List<OptionProperty>();
            foreach (PropertyInfo prop in paramProps)
            {
                IEnumerable<OptionAttribute> optionAttrs = prop.GetCustomAttributes<OptionAttribute>(true);
                foreach (OptionAttribute optionAttr in optionAttrs)
                {
                    optionsProperties.Add(new OptionProperty(optionAttr, prop));
                }
            }
        }

        public CliParseResult<TParameters> Parse(string[] args)
        {
            var result = new TParameters();
            var errors = new List<Exception>();
            bool showHelp = false;
            bool showVersion = false;

            visitedArgs = new HashSet<OptionProperty>();

            int argInd = 0;
            while (argInd < args.Length)
            {
                string arg = args[argInd];

                if (!arg.StartsWith("-"))
                {
                    errors.Add(new ArgumentException("Incorrect argument", arg));

                    argInd++;
                    continue;
                }

                string trimmedArg = arg.TrimStart('-');

                if (trimmedArg == "help")
                {
                    showHelp = true;
                    argInd++;
                    continue;
                }

                if (trimmedArg == "version")
                {
                    showVersion = true;
                    argInd++;
                    continue;
                }

                OptionProperty foundOption = optionsProperties.FirstOrDefault(optionType =>
                    optionType.Option.ShortName == trimmedArg || optionType.LongName == trimmedArg);

                if (foundOption != null)
                {
                    argInd++;

                    string outValue = argInd < args.Length ? args[argInd] : null;

                    Type underlyingType = Nullable.GetUnderlyingType(foundOption.PropertyInfo.PropertyType);
                    var notNullableType = underlyingType != null ? underlyingType : foundOption.PropertyInfo.PropertyType;

                    if (notNullableType == typeof(bool) && (argInd == args.Length || outValue.StartsWith("-")))
                    {
                        CheckAndSetIfParsed(result, foundOption, arg, true.ToString().ToLowerInvariant(), notNullableType, errors);
                    }
                    else
                    {
                        CheckAndSetIfParsed(result, foundOption, arg, outValue, notNullableType, errors);
                        argInd++;
                    }
                }
                else
                {
                    errors.Add(new ArgumentException("Unknown argument", args[argInd]));
                    argInd++;
                }
            }

            return new CliParseResult<TParameters>(result, showHelp, showVersion, errors);
        }

        public static string[] GenerateHelpText()
        {
            var result = new List<string>();

            result.Add("Available cli parameters:");
            result.Add("");

            foreach (OptionProperty optionProperty in optionsProperties)
            {
                var line = new StringBuilder();

                if (!string.IsNullOrEmpty(optionProperty.Option.ShortName))
                {
                    line.Append('-');
                    line.Append(optionProperty.Option.ShortName);
                    line.Append(", ");
                }

                line.Append("--");
                line.Append(optionProperty.LongName);

                result.Add(line.ToString());

                result.Add("");

                if (!string.IsNullOrEmpty(optionProperty.Option.HelpText))
                {
                    result.Add("    " + optionProperty.Option.HelpText);
                    result.Add("");
                }
            }

            return result.ToArray();
        }

        private bool CheckAndSetIfParsed(TParameters parameters, OptionProperty optionProperty, string outArg, string outValue, Type type, List<Exception> errors)
        {
            if (CheckDuplicates && visitedArgs.Contains(optionProperty))
            {
                errors.Add(new ArgumentException("Duplicate argument", outArg));
                return false;
            }

            visitedArgs.Add(optionProperty);

            if (type == typeof(string))
            {
                optionProperty.PropertyInfo.SetValue(parameters, outValue);
                return true;
            }

            if (type == typeof(int))
            {
                if (int.TryParse(outValue, out int value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(uint))
            {
                if (uint.TryParse(outValue, out uint value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(byte))
            {
                if (byte.TryParse(outValue, out byte value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(sbyte))
            {
                if (sbyte.TryParse(outValue, out sbyte value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(short))
            {
                if (short.TryParse(outValue, out short value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(ushort))
            {
                if (ushort.TryParse(outValue, out ushort value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(long))
            {
                if (long.TryParse(outValue, out long value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(ulong))
            {
                if (ulong.TryParse(outValue, out ulong value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(float))
            {
                if (float.TryParse(outValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(double))
            {
                if (double.TryParse(outValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(decimal))
            {
                if (decimal.TryParse(outValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, value);
                    return true;
                }

                AddIncorrectValueException(errors, type, outArg, outValue);
                return false;
            }

            if (type == typeof(bool))
            {
                if (bool.TryParse(outValue, out bool b))
                {
                    optionProperty.PropertyInfo.SetValue(parameters, b);
                    return true;
                }

                errors.Add(new FormatException($"Incorrect value `{outValue}` of argument `{outArg}`"));
                return false;
            }

            if (type.BaseType == typeof(Enum))
            {
                try
                {
                    optionProperty.PropertyInfo.SetValue(parameters, Enum.Parse(type, outValue, IgnoreCase));
                    return true;
                }
                catch
                {
                    AddIncorrectValueException(errors, type, outArg, outValue);
                    return false;
                }
            }

            if (type == typeof(string[]))
            {
                string[] values = outValue.Split(optionProperty.Option.Separator);
                var result = (Array) Activator.CreateInstance(type, values.Length);

                for (int i = 0; i < values.Length; i++)
                {
                    result.SetValue(values[i], i);
                }

                optionProperty.PropertyInfo.SetValue(parameters, result);
                return true;
            }

            errors.Add(new NotSupportedException($"Mapping of Type {type} of property {optionProperty.PropertyInfo.Name} is not supported"));
            return false;
        }

        private static void AddIncorrectValueException(List<Exception> errors, Type type, string outArg, string outValue)
        {
            errors.Add(new FormatException($"Incorrect value `{outValue}` of argument {outArg} with type {type}"));
        }
    }
}
