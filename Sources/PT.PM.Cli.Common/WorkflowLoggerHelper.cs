using PT.PM.Common;
using PT.PM.Matching;

namespace PT.PM.Cli.Common
{
    public class WorkflowLoggerHelper : WorkflowLoggerHelperBase<Stage, WorkflowResult, PatternRoot, MatchResult, Stage>
    {
        public const int Align = -25;

        public WorkflowLoggerHelper(ILogger logger, WorkflowResult workflowResult)
            : base(logger, workflowResult)
        {
        }

        protected override void LogAdvanced()
        {
            if (WorkflowResult.Stage >= Stage.Match)
            {
                LogStageTime(nameof(Stage.Match));
            }
        }
    }
}
