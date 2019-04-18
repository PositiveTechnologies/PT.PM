using PT.PM.Common;
using System.Collections.Generic;

namespace PT.PM.Cli.Common
{
    public static class CliUtils
    {
        public static string[] SplitArguments(this string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                return ArrayUtils<string>.EmptyArray;
            }

            var argChars = args.ToCharArray();
            var inQuote = false;
            var result = new List<string>(argChars.Length);
            var lastArg = new List<char>();
            bool lastArgInQuotes = false;
            int index = 0;

            while (index < args.Length)
            {
                if (args[index] == '"')
                {
                    if (inQuote)
                    {
                        if (index + 1 < args.Length && args[index + 1] == '"')
                        {
                            lastArg.Add('"');
                            index++;
                        }
                        else
                        {
                            lastArgInQuotes = true;
                            inQuote = false;
                        }
                    }
                    else
                    {
                        inQuote = true;
                    }
                }
                else
                {
                    if (!inQuote && args[index] == ' ')
                    {
                        AppendIfNotEmptyOrQuoted(result, lastArg, lastArgInQuotes);
                        lastArgInQuotes = false;
                    }
                    else
                    {
                        lastArg.Add(args[index]);
                    }
                }
                index++;
            }

            AppendIfNotEmptyOrQuoted(result, lastArg, lastArgInQuotes);

            return result.ToArray();
        }

        private static void AppendIfNotEmptyOrQuoted(List<string> result, List<char> lastArg, bool lastArgInQuotes)
        {
            if (lastArg.Count > 0 || lastArgInQuotes)
            {
                result.Add(new string(lastArg.ToArray()));
                lastArg.Clear();
            }
        }
    }
}
