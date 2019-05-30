using PT.PM.Common;
using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using PT.PM.AntlrUtils;
using PT.PM.Common.Files;

namespace PT.PM
{
    public class ParserUnit
    {
        private readonly Thread thread;

        private readonly ParserUnitLogger logger;

        public Language Language { get; }

        public ParseTree ParseTree { get; private set; }

        public int ParseErrorCount => logger.ErrorCount;

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
            logger = new ParserUnitLogger();
            this.thread = thread;
        }

        public void Parse(TextFile sourceFile)
        {
            ILanguageParserBase parser = Language.CreateParser();
            parser.Logger = logger;

            if (parser is AntlrParser antlrParser)
            {
                var lexer = (AntlrLexer)Language.CreateLexer();
                lexer.Logger = logger;
                var tokens = lexer.GetTokens(sourceFile, out TimeSpan _);
                ParseTree = antlrParser.Parse(tokens, out TimeSpan _);
            }
            else
            {
                ParseTree = ((ILanguageParser<TextFile>)parser).Parse(sourceFile, out TimeSpan _);
            }
        }

        public override string ToString()
        {
            return $"{Language}; Errors: {ParseErrorCount}; IsAlive: {IsAlive}";
        }
    }
}
