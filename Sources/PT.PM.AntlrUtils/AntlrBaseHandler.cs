using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Atn;
using PT.PM.Common;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrBaseHandler : ILanguageHandler
    {
        protected static Dictionary<Language, ATN> lexerAtns = new Dictionary<Language, ATN>();

        protected static Dictionary<Language, ATN> parserAtns = new Dictionary<Language, ATN>();

        public static ILogger StaticLogger { get; set; } = DummyLogger.Instance;

        public ILogger Logger { get; set; } = DummyLogger.Instance;
        
        public abstract Language Language { get; }
        public bool UseFastParseStrategyAtFirst { get; set; } = true;

        public static long MemoryConsumptionBytes { get; set; } = 3 * 1024 * 1024 * 1024L;

        public static long ClearCacheFilesBytes { get; set; } = 5 * 1024 * 1024L;

        public static int ClearCacheFilesCount { get; set; } = 50;
        
        /// <summary>
        /// Converts \r to \r\n.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual string PreprocessText(TextFile file)
        {
            var text = file.Data;
            var result = new StringBuilder(text.Length);
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '\r')
                {
                    if (i + 1 >= text.Length)
                    {
                        result.Append('\n');
                    }
                    else if (text[i + 1] != '\n')
                    {
                        result.Append('\n');
                    }
                    else
                    {
                        result.Append(text[i]);
                    }
                }
                else
                {
                    result.Append(text[i]);
                }
                i++;
            }
            return result.ToString();
        }
        
        protected ATN GetOrCreateAtn(bool lexer, string atnText)
        {
            ATN atn;
            Dictionary<Language, ATN> atns = lexer ? lexerAtns : parserAtns;

            lock (atns)
            {
                if (!atns.TryGetValue(Language, out atn))
                {
                    atn = new ATNDeserializer().Deserialize(atnText.ToCharArray());
                    atns.Add(Language, atn);
                    Logger.LogDebug($"New ATN initialized for {Language} {(lexer ? "lexer" : "parser")}.");
                }
            }

            return atn;
        }
    }
}