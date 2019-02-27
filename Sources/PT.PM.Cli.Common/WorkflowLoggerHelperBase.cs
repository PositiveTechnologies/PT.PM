using PT.PM.Common;
using System;
using System.Globalization;

namespace PT.PM.Cli.Common
{
    public abstract class
        WorkflowLoggerHelperBase<TStage, TWorkflowResult, TPattern, TRenderStage> : ILoggable
        where TStage : Enum
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TRenderStage>
        where TRenderStage : Enum
    {
        public ILogger Logger { get; set; }

        public TWorkflowResult WorkflowResult { get; }

        public WorkflowLoggerHelperBase(ILogger logger, TWorkflowResult workflowResult)
        {
            Logger = logger;
            WorkflowResult = workflowResult ?? throw new ArgumentNullException(nameof(workflowResult));
        }

        public void LogStatistics()
        {
            LogAdvancedInfo();

            if (WorkflowResult.TotalTerminatedFilesCount > 0)
            {
                Logger.LogInfo(
                    $"{"Terminated files count:",LoggerUtils.Align} {WorkflowResult.TotalTerminatedFilesCount}");
            }

            if (!WorkflowResult.Stage.Is(Stage.Pattern) || WorkflowResult.TotalProcessedFilesCount > 0)
            {
                Logger.LogInfo($"{"Files count:",LoggerUtils.Align} {WorkflowResult.TotalProcessedFilesCount}");
            }

            if (WorkflowResult.TotalProcessedFilesCount > 0)
            {
                Logger.LogInfo(
                    $"{"Lines/chars count:",LoggerUtils.Align} {WorkflowResult.TotalProcessedLinesCount} / {WorkflowResult.TotalProcessedCharsCount}");
            }

            Logger.LogInfo($"{"Patterns count:",LoggerUtils.Align} {WorkflowResult.TotalProcessedPatternsCount}");

            long totalTimeTicks = WorkflowResult.TotalTimeTicks;
            if (totalTimeTicks > 0)
            {
                int stageInt = Convert.ToInt32(WorkflowResult.Stage);

                if (stageInt >= (int) Stage.File)
                {
                    LogStageTime(nameof(Stage.File));
                    if (stageInt >= (int) Stage.Language)
                    {
                        LogStageTime(nameof(Stage.Language));
                        if (stageInt >= (int) Stage.Tokens)
                        {
                            LogStageTime(nameof(Stage.Tokens));
                            if (stageInt >= (int) Stage.ParseTree)
                            {
                                LogStageTime(nameof(Stage.ParseTree));
                                if (stageInt >= (int) Stage.Ust)
                                {
                                    LogStageTime(nameof(Stage.Ust));
                                    LogAdvancedStageInfo();
                                }
                            }
                        }
                    }
                }

                LogStageTime(nameof(Stage.Pattern));
            }
        }

        protected virtual void LogAdvancedInfo()
        {
        }

        protected abstract void LogAdvancedStageInfo();

        protected void LogStageTime(string stage)
        {
            long ticks;
            switch (stage)
            {
                case nameof(Stage.File):
                    ticks = WorkflowResult.TotalReadTicks;
                    break;
                case nameof(Stage.Language):
                    ticks = WorkflowResult.TotalDetectTicks;
                    break;
                case nameof(Stage.Tokens):
                    ticks = WorkflowResult.TotalLexerTicks;
                    break;
                case nameof(Stage.ParseTree):
                    ticks = WorkflowResult.TotalLexerParserTicks;
                    break;
                case nameof(Stage.Ust):
                    ticks = WorkflowResult.TotalConvertTicks;
                    break;
                case nameof(Stage.Match):
                    ticks = WorkflowResult.TotalMatchTicks;
                    break;
                case nameof(Stage.Pattern):
                    ticks = WorkflowResult.TotalPatternsTicks;
                    break;
                default:
                    ticks = GetTicksCount(stage);
                    break;
            }

            if (ticks > 0)
            {
                string percent = CalculateAndFormatPercent(ticks, WorkflowResult.TotalTimeTicks);
                Logger.LogInfo($"{stage + " time ratio:",LoggerUtils.Align} {percent}%");
            }
        }

        protected virtual long GetTicksCount(string stage) => 0;

        protected string CalculateAndFormatPercent(long part, long whole)
        {
            return (whole == 0 ? 0 : (double) part / whole * 100.0).ToString("00.00", CultureInfo.InvariantCulture);
        }
    }
}
