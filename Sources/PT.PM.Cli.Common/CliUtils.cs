using PT.PM.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PT.PM.Cli.Common
{
    public static class CliUtils
    {
        public const int Align = -25;

        private const int TwoInPower20 = 1 << 20;

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
            lastArgInQuotes = false;

            return result.ToArray();
        }

        public static void LogSystemInfo(ILogger logger, string coreName)
        {
            Process currentProcess = Process.GetCurrentProcess();

            logger.LogInfo($"{coreName + " version:",Align} {GetVersionString(coreName)}");
            logger.LogInfo($"{"Finish date:",Align} {DateTime.Now}");
            logger.LogInfo($"{"OS:",Align} {Environment.OSVersion}");
            logger.LogInfo($"{"Config:",Align} {(CommonUtils.IsDebug ? "DEBUG" : "RELEASE")} ({(Debugger.IsAttached ? "+ debugger" : "no debugger")})");

            string processBitsString = (Environment.Is64BitProcess ? "64" : "32") + "-bit";
            double peakVirtualSet = currentProcess.PeakVirtualMemorySize64 / TwoInPower20;
            double peakWorkingSet = currentProcess.PeakWorkingSet64 / TwoInPower20;
            logger.LogInfo($"{"Peak virtual/working set:",Align} {peakVirtualSet} / {peakWorkingSet} MB, {processBitsString}");
        }

        public static string GetVersionString(string coreName)
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            AssemblyName assemblyName = assembly.GetName();
            string buildTime = "";

            Stream stream = assembly.GetManifestResourceStream($"{coreName}.Cli.BuildTimeStamp");
            if (stream != null)
            {
                using (var reader = new StreamReader(stream))
                {
                    buildTime = reader.ReadToEnd().Trim();
                }
                buildTime = $" (build: {buildTime})";
            }

            return $"{assemblyName.Version}{buildTime}";
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
