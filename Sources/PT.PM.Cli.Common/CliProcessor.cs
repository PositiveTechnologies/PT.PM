using PT.PM.Matching;

namespace PT.PM.Cli.Common
{
    public class CliProcessor : CliProcessorBase<Stage, WorkflowResult, PatternRoot, CliParameters, Stage>
    {
        public override string CoreName => "PT.PM";

        protected override WorkflowBase<Stage, WorkflowResult, PatternRoot, Stage> CreateWorkflow()
        {
            return new Workflow();
        }

        protected override void LogStatistics(WorkflowResult workflowResult)
        {
            var workflowLoggerHelper = new WorkflowLoggerHelper(Logger, workflowResult);
            workflowLoggerHelper.LogStatistics();
        }
    }
}
