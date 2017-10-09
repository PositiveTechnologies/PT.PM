using PT.PM.Common;
using System.Threading.Tasks;

namespace PT.PM
{
    public class ParserUnit
    {
        public ILanguageParser Parser { get; set; }

        public LoggerMessageCounter Logger { get; set; }

        public Task Task { get; set; }

        public int ParseErrorCount => Logger.ErrorCount;

        public ParserUnit(ILanguageParser parser, LoggerMessageCounter logger, Task task)
        {
            Parser = parser;
            Logger = logger;
            Task = task;
        }
    }
}
