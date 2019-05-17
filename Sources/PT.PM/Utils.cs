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
using PT.PM.CSharpParseTreeUst.RoslynUstVisitor;
using PT.PM.JavaParseTreeUst;
using PT.PM.JavaParseTreeUst.Converter;
using PT.PM.PhpParseTreeUst;
using PT.PM.SqlParseTreeUst;
using PT.PM.PythonParseTreeUst;

namespace PT.PM
{
    public static class Utils
    {
        public const int DefaultMaxStackSize = 4 * 1024 * 1024;
        public const string TimeSpanFormat = "mm\\:ss\\.fff";
        public const string FileSerializedMessage = "File serialized.";

        public static void RegisterAllLexersParsersAndConverters()
        {
            LanguageUtils.RegisterParserConverter(Language.CSharp, CSharpRoslynParser.Create, CSharpRoslynParseTreeConverter.Create);
            LanguageUtils.RegisterParserConverter(Language.JavaScript, JavaScriptEsprimaParser.Create, JavaScriptEsprimaParseTreeConverter.Create);
            LanguageUtils.RegisterParserConverter(Language.Aspx, CSharpParseTreeUst.AspxParser.Create, AspxConverter.Create);

            LanguageUtils.RegisterLexerParserConverter(Language.Java, JavaAntlrLexer.Create, JavaAntlrParser.Create, JavaAntlrParseTreeConverter.Create);
            LanguageUtils.RegisterLexerParserConverter(Language.Php, PhpAntlrLexer.Create, PhpAntlrParser.Create, PhpAntlrParseTreeConverter.Create);
            LanguageUtils.RegisterLexerParserConverter(Language.Html, PhpAntlrLexer.Create, PhpAntlrParser.Create, PhpAntlrParseTreeConverter.Create);
            LanguageUtils.RegisterLexerParserConverter(Language.Python, PythonAntlrLexer.Create, PythonAntlrParser.Create, PythonAntlrConverter.Create);

            LanguageUtils.RegisterLexerParserConverter(Language.PlSql, PlSqlAntlrLexer.Create, PlSqlAntlrParser.Create, PlSqlAntlrConverter.Create);
            LanguageUtils.RegisterLexerParserConverter(Language.TSql, TSqlAntlrLexer.Create, TSqlAntlrParser.Create, TSqlAntlrConverter.Create);
            LanguageUtils.RegisterLexerParserConverter(Language.MySql, MySqlAntlrLexer.Create, MySqlAntlrParser.Create, MySqlAntlrConverter.Create);
        }

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

            if (language == Language.CSharp)
            {
                dumper = new RoslynDumper();
            }
            else if (language == Language.JavaScript)
            {
                dumper = new JavaScriptEsprimaParseTreeDumper();
            }
            else
            {
                dumper = new AntlrDumper();
            }

            return dumper;
        }

        public static string Format(this TimeSpan timeSpan) => timeSpan.ToString(TimeSpanFormat, CultureInfo.InvariantCulture);

        public static string GetVersionString()
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            AssemblyName assemblyName = assembly.GetName();
            DateTime buildTime = default;

            string streamName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.Contains("BuildTimeStamp"));
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

            if (buildTime == default)
            {
                buildTime = File.GetLastWriteTime(assembly.Location.NormalizeFilePath());
            }

            return $"{assemblyName?.Version} ({buildTime.ToString(CultureInfo.InvariantCulture)})";
        }

        public static string GetElapsedString(this Stopwatch stopwatch) => GetElapsedString(stopwatch.Elapsed);

        public static string GetElapsedString(this TimeSpan timeSpan) => $"(Elapsed: {timeSpan.Format()})";
    }
}
