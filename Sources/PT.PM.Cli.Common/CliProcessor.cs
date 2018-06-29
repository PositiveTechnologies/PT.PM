using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Matching;

namespace PT.PM.Cli.Common
{
    public class CliProcessor : CliProcessorBase<RootUst, Stage, WorkflowResult, PatternRoot, MatchResult, CliParameters, Stage>
    {
        public override string CoreName => "PT.PM";

        protected override WorkflowBase<RootUst, Stage, WorkflowResult, PatternRoot, MatchResult, Stage> CreateWorkflow(CliParameters parameters)
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
                startStage = startStageString.ParseEnum(ContinueWithInvalidArgs, startStage, Logger);
            }

            return startStage == Stage.Ust || startStage == Stage.SimplifiedUst;
        }
    }
}
