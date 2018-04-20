using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Matching;

namespace PT.PM.Cli.Common
{
    public class CliProcessor : CliProcessorBase<RootUst, Stage, WorkflowResult, PatternRoot, MatchResult, CliParameters>
    {
        public override string CoreName => "PT.PM";

        protected override WorkflowBase<RootUst, Stage, WorkflowResult, PatternRoot, MatchResult> CreateWorkflow(CliParameters parameters)
        {
            return new Workflow();
        }

        protected override void LogStatistics(WorkflowResult workflowResult)
        {
            var workflowLoggerHelper = new WorkflowLoggerHelper(Logger, workflowResult);
            workflowLoggerHelper.LogStatistics();
        }

        protected override bool IsLoadJson(string startStageString)
        {
            Stage startStage = Stage.File;
            if (startStageString != null)
            {
                startStage = startStageString.ParseEnum<Stage>();
            }

            return startStage == Stage.Ust || startStage == Stage.SimplifiedUst;
        }
    }
}
