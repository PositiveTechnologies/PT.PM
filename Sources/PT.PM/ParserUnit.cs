using PT.PM.Common;
using System;
using System.Threading;

namespace PT.PM
{
    public class ParserUnit
    {
        private Thread thread { get; set; }

        private ILanguageParser parser { get; set; }

        public int ParseErrorCount => parser.Logger.ErrorCount;

        public bool IsAlive => thread.IsAlive;

        public void Abort() => thread.Abort();

        public ParserUnit(ILanguageParser parser, Thread thread)
        {
            this.parser = parser ?? throw new NullReferenceException(nameof(parser));
            this.thread = thread ?? throw new NullReferenceException(nameof(thread));
        }
    }
}
