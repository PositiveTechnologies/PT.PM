using PT.PM.Common;
using PT.PM.Matching;

namespace PT.PM.Cli.Common
{
    public class WorkflowLoggerHelper : WorkflowLoggerHelperBase<Stage, WorkflowResult, PatternRoot, MatchResult>
    {
        public const int Align = -25;

        public WorkflowLoggerHelper(ILogger logger, WorkflowResult workflowResult)
            : base(logger, workflowResult)
        {
        }

        protected override void LogAdvanced()
        {
            if (WorkflowResult.Stage >= Stage.SimplifiedUst)
            {
                LogStageTime(nameof(Stage.SimplifiedUst));
                if (WorkflowResult.Stage >= Stage.Match)
                {
                    LogStageTime(nameof(Stage.Match));
                }
            }
        }

        protected override long GetTicksCount(string stage)
        {
            if (stage == nameof(Stage.SimplifiedUst))
            {
                return WorkflowResult.TotalSimplifyTicks;
            }
            return 0;
        }
    }
}
