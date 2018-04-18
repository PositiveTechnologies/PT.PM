using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;

namespace PT.PM.Cli
{
    public class CliProcessor : CliProcessorBase<RootUst, Stage, WorkflowResult, PatternRoot, MatchResult, CliParameters>
    {
        protected override WorkflowBase<RootUst, Stage, WorkflowResult, PatternRoot, MatchResult> CreateWorkflow(CliParameters parameters, SourceCodeRepository sourceCodeRepository, IPatternsRepository patternsRepository)
        {
            return new Workflow(sourceCodeRepository, patternsRepository);
        }

        protected override void LogStatistics(ILogger logger, WorkflowResult workflowResult)
        {
            var workflowLoggerHelper = new WorkflowLoggerHelper(logger, workflowResult);
            workflowLoggerHelper.LogStatistics();
        }
    }
}
