using PT.PM.Common;
using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common.Files;

namespace PT.PM
{
    public class ParserUnit
    {
        private Thread thread;

        private ParserUnitLogger logger;

        private IBaseLanguageParser parser;

        public Language Language { get; }

        public ParseTree ParseTree { get; private set; }

        public int ParseErrorCount => parser.Logger.ErrorCount;

        public List<Exception> Errors => logger.Errors;

        public List<object> Infos => logger.Infos;

        public List<string> Debugs => logger.Debugs;

        public bool IsAlive => thread?.IsAlive ?? false;

        public void AbortIfPossibly()
        {
            if (!CommonUtils.IsCoreApp)
            {
                thread?.Abort();
            }
            else
            {
                logger.LogInfo("Thread.Abort is not supported in .NET Core");
            }
        }

        public void Wait(TimeSpan timeout) => thread?.Join(timeout);

        public void Wait(int millisecondsTimeout) => thread?.Join(millisecondsTimeout);

        public void Wait() => thread?.Join();

        public ParserUnit(Language language, Thread thread)
        {
            Language = language;
            parser = language.CreateParser();
            logger = new ParserUnitLogger();
            parser.Logger = logger;
            this.thread = thread;
        }

        public void Parse(TextFile sourceFile)
        {
            if (parser is AntlrParser antlrParser)
            {
                antlrParser.SourceFile = sourceFile;
                var lexer = antlrParser.InitAntlrLexer();
                antlrParser.ErrorListener = lexer.ErrorListener;
                ParseTree = antlrParser.Parse(lexer?.GetTokens(sourceFile));
            }
            else
            {
                ParseTree = ((ILanguageParser<TextFile>)parser).Parse(sourceFile);
            }
        }

        public override string ToString()
        {
            return $"{Language}; Errors: {ParseErrorCount}; IsAlive: {IsAlive}";
        }
    }
}
