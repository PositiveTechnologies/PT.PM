using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Utils;
using PT.PM.CSharpParseTreeUst;
using PT.PM.JavaScriptParseTreeUst;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PT.PM
{
    public static class Utils
    {
        public const int DefaultMaxStackSize = 4 * 1024 * 1024;
        public const string TimeSpanFormat = "mm\\:ss\\.ff";

        public static bool Is<TStage>(this TStage stage, Stage pmStage)
            where TStage : Enum
        {
            return Convert.ToInt32(stage) == (int)pmStage;
        }

        public static bool IsGreaterOrEqual<TStage>(this TStage stage, Stage pmStage)
            where TStage : Enum
        {
            return Convert.ToInt32(stage) >= (int)pmStage;
        }

        public static bool IsLess<TStage>(this TStage stage, Stage pmStage)
            where TStage : Enum
        {
            return Convert.ToInt32(stage) < (int)pmStage;
        }

        public static ParseTreeDumper CreateParseTreeDumper(Language language)
        {
            ParseTreeDumper dumper;

            if (language == CSharp.Language)
            {
                dumper = new RoslynDumper();
            }
            else if (language == JavaScript.Language)
            {
                dumper = new JavaScriptEsprimaParseTreeDumper();
            }
            else
            {
                dumper = new AntlrDumper();
            }

            return dumper;
        }

        public static string Format(this TimeSpan timeSpan) => timeSpan.ToString(TimeSpanFormat);

        public static string GetVersionString()
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            AssemblyName assemblyName = assembly?.GetName();
            DateTime buildTime = default;

            string streamName = assembly?.GetManifestResourceNames().FirstOrDefault(name => name.Contains("BuildTimeStamp")) ?? null;
            Stream stream = !string.IsNullOrEmpty(streamName)
                ? assembly.GetManifestResourceStream(streamName)
                : null;
            if (stream != null)
            {
                using (var reader = new StreamReader(stream))
                {
                    string dateString = reader.ReadToEnd().Trim();
                    DateTime.TryParse(dateString, out buildTime);
                }
            }

            if (buildTime == default && assembly != null)
            {
                buildTime = File.GetLastWriteTime(assembly.Location.NormalizeFilePath());
            }

            return $"{assemblyName?.Version} ({buildTime.ToString(CultureInfo.InvariantCulture)})";
        }

        public static string GetElapsedString(this Stopwatch stopwatch) => $"(Elapsed: {stopwatch.Elapsed.Format()})";
    }
}
