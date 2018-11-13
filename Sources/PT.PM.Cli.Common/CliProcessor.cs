using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Utils;
using PT.PM.Matching;

namespace PT.PM.Cli.Common
{
    public class CliProcessor : CliProcessorBase<Stage, WorkflowResult, PatternRoot, CliParameters, Stage>
    {
        public override string CoreName => "PT.PM";

        protected override WorkflowBase<Stage, WorkflowResult, PatternRoot, Stage> CreateWorkflow(CliParameters parameters)
        {
            return new Workflow();
        }

        protected override WorkflowBase<Stage, WorkflowResult, PatternRoot, Stage>
            InitWorkflow(CliParameters parameters)
        {
            var workflow = (Workflow)base.InitWorkflow(parameters);

            if (parameters.StartStage == null && workflow.SourceCodeRepository.LoadJson)
            {
                workflow.StartStage = Stage.Ust;
            }

            return workflow;
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

            return startStage == Stage.Ust;
        }
    }
}
