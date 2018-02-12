using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;

namespace PT.PM.Cli
{
    public class CliProcessor : CliProcessorBase<Stage, WorkflowResult, PatternRoot, MatchResult, CliParameters>
    {
        protected override WorkflowResult InitWorkflowAndProcess(CliParameters parameters, ILogger logger, SourceCodeRepository sourceCodeRepository, IPatternsRepository patternsRepository)
        {
            Stage stage = string.IsNullOrEmpty(parameters.InputFileNameOrDirectory)
                ? Stage.Pattern
                : parameters.Stage.ParseEnum(Stage.Match);

            var workflow = new Workflow(sourceCodeRepository, patternsRepository, stage)
            {
                Logger = logger,
                ThreadCount = parameters.ThreadCount,
                MemoryConsumptionMb = parameters.Memory,
                IsIncludePreprocessing = parameters.IsPreprocessUst,
                LogsDir = parameters.LogsDir,
                DumpDir = parameters.LogsDir,
                StartStage = parameters.StartStage.ParseEnum(Stage.File),
                DumpStages = new HashSet<Stage>(parameters.DumpStages.ParseCollection<Stage>()),
                IndentedDump = parameters.IndentedDump,
                DumpWithTextSpans = parameters.IncludeTextSpansInDump,
                LineColumnTextSpans = parameters.LineColumnTextSpans,
                RenderStages = new HashSet<Stage>(parameters.RenderStages.ParseCollection<Stage>()),
                RenderFormat = parameters.RenderFormat.ParseEnum<GraphvizOutputFormat>(),
                RenderDirection = parameters.RenderDirection.ParseEnum<GraphvizDirection>(),
            };

            WorkflowResult workflowResult = workflow.Process();

            return workflowResult;
        }

        protected override void LogStatistics(ILogger logger, WorkflowResult workflowResult)
        {
            var workflowLoggerHelper = new WorkflowLoggerHelper(logger, workflowResult);
            workflowLoggerHelper.LogStatistics();
        }
    }
}
