using PT.PM.Common;
using PT.PM.Matching;

namespace PT.PM.Cli
{
    public class WorkflowLoggerHelper : WorkflowLoggerHelperBase<Stage, WorkflowResult, PatternRoot, MatchingResult>
    {
        public WorkflowLoggerHelper(ILogger logger, WorkflowResult workflowResult)
            : base(logger, workflowResult)
        {
        }
    }
}
