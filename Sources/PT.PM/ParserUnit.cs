using PT.PM.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PT.PM
{
    public class ParserUnit
    {
        private Thread thread;

        private ParserUnitLogger logger;

        private ILanguageParser parser;

        public Language Language { get; }

        public ParseTree ParseTree { get; private set; }

        public int ParseErrorCount => parser.Logger.ErrorCount;

        public List<Exception> Errors => logger.Errors;

        public List<object> Infos => logger.Infos;

        public List<string> Debugs => logger.Debugs;

        public bool IsAlive => thread?.IsAlive ?? false;

        public void Abort() => thread?.Abort();

        public void Wait(TimeSpan timeout) => thread?.Join(timeout);

        public void Wait(int millisecondsTimeout) => thread?.Join(millisecondsTimeout);

        public void Wait() => thread?.Join();

        public ParserUnit(Language language, Thread thread)
        {
            Language = language ?? throw new NullReferenceException(nameof(language));
            parser = language.CreateParser();
            logger = new ParserUnitLogger();
            parser.Logger = logger;
            this.thread = thread;
        }

        public void Parse(CodeFile codeFile)
        {
            ParseTree = parser.Parse(codeFile);
        }

        public override string ToString()
        {
            return $"{Language}; Errors: {ParseErrorCount}; IsAlive: {IsAlive}";
        }
    }
}
