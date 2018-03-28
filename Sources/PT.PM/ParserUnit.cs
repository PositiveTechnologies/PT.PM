using PT.PM.Common;
using System;
using System.Threading;

namespace PT.PM
{
    public class ParserUnit
    {
        private Thread thread { get; set; }

        private ILanguageParser parser { get; set; }

        public Language Language { get; }

        public int ParseErrorCount => parser.Logger.ErrorCount;

        public bool IsAlive => thread.IsAlive;

        public void Abort() => thread.Abort();

        public ParserUnit(Language language, ILanguageParser parser, Thread thread)
        {
            Language = language ?? throw new NullReferenceException(nameof(language));
            this.parser = parser ?? throw new NullReferenceException(nameof(parser));
            this.thread = thread ?? throw new NullReferenceException(nameof(thread));
        }

        public override string ToString()
        {
            return $"{Language}; Errors: {ParseErrorCount}; IsAlive: {thread.IsAlive}";
        }
    }
}
