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

            if (parameters.StartStage == null && workflow.SourceCodeRepository.Format != null)
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
    }
}
