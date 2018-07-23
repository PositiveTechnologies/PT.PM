using PT.PM.Common;
using PT.PM.Matching;
using System;
using System.Linq;

namespace PT.PM.Cli.Common
{
    public abstract class WorkflowLoggerHelperBase<TStage, TWorkflowResult, TPattern, TMatchResult, TRenderStage> : ILoggable
        where TStage : Enum
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchResult, TRenderStage>
        where TMatchResult : MatchResultBase<TPattern>
        where TRenderStage : Enum
    {
        protected const string SimplifiedUstStageName = "SimplifiedUst";

        public ILogger Logger { get; set; }

        public TWorkflowResult WorkflowResult { get; set; }

        public WorkflowLoggerHelperBase(ILogger logger, TWorkflowResult workflowResult)
        {
            Logger = logger;
            WorkflowResult = workflowResult;
        }

        public void LogStatistics()
        {
            if (WorkflowResult.TotalTerminatedFilesCount > 0)
            {
                Logger.LogInfo($"{"Terminated files count:",CliUtils.Align} {WorkflowResult.TotalTerminatedFilesCount}");
            }
            if (!WorkflowResult.Stage.Is(Stage.Pattern) || WorkflowResult.TotalProcessedFilesCount > 0)
            {
                Logger.LogInfo($"{"Files count:",CliUtils.Align} {WorkflowResult.TotalProcessedFilesCount}");
            }
            if (WorkflowResult.TotalProcessedFilesCount > 0)
            {
                Logger.LogInfo($"{"Lines/chars count:",CliUtils.Align} {WorkflowResult.TotalProcessedLinesCount} / {WorkflowResult.TotalProcessedCharsCount}");
            }
            Logger.LogInfo($"{"Patterns count:",CliUtils.Align} {WorkflowResult.TotalProcessedPatternsCount}");
            long totalTimeTicks = WorkflowResult.TotalTimeTicks;
            if (totalTimeTicks > 0)
            {
                if (Convert.ToInt32(WorkflowResult.Stage) >= (int)Stage.File)
                {
                    LogStageTime(nameof(Stage.File));
                    if (Convert.ToInt32(WorkflowResult.Stage) >= (int)Stage.ParseTree)
                    {
                        LogStageTime(nameof(Stage.ParseTree));
                        if (Convert.ToInt32(WorkflowResult.Stage) >= (int)Stage.Ust)
                        {
                            LogStageTime(nameof(Stage.Ust));
                            if (WorkflowResult.IsSimplifyUst)
                            {
                                LogStageTime(SimplifiedUstStageName);
                            }
                            LogAdvanced();
                        }
                    }
                }
                if (Convert.ToInt32(WorkflowResult.Stage) >= (int)Stage.Match ||
                    Convert.ToInt32(WorkflowResult.Stage) == (int)Stage.Pattern)
                {
                    LogStageTime(nameof(Stage.Pattern));
                }
            }
        }

        protected abstract void LogAdvanced();

        protected void LogStageTime(string stage)
        {
            long ticks = 0;
            switch (stage)
            {
                case nameof(Stage.File):
                    ticks = WorkflowResult.TotalReadTicks;
                    break;
                case nameof(Stage.ParseTree):
                    ticks = WorkflowResult.TotalParseTicks;
                    break;
                case nameof(Stage.Ust):
                    ticks = WorkflowResult.TotalConvertTicks;
                    break;
                case SimplifiedUstStageName:
                    ticks = WorkflowResult.TotalSimplifyTicks;
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
                var timeSpan = new TimeSpan(ticks);
                string percent = CalculateAndFormatPercent(ticks, WorkflowResult.TotalTimeTicks);
                Logger.LogInfo
                    ($"{"Total " + stage + " time:",CliUtils.Align} {timeSpan.Format()} {percent}%");
            }
            if (stage == nameof(Stage.ParseTree))
            {
                LogAdditionalParserInfo();
            }
        }

        protected virtual long GetTicksCount(string stage) => 0;

        protected void LogAdditionalParserInfo()
        {
            if (WorkflowResult.AnalyzedLanguages.Any(lang => lang.HaveAntlrParser))
            {
                TimeSpan lexerTimeSpan = new TimeSpan(WorkflowResult.TotalLexerTicks);
                TimeSpan parserTimeSpan = new TimeSpan(WorkflowResult.TotalParserTicks);
                string lexerPercent = CalculateAndFormatPercent(WorkflowResult.TotalLexerTicks, WorkflowResult.TotalParseTicks);
                string parserPercent = CalculateAndFormatPercent(WorkflowResult.TotalParserTicks, WorkflowResult.TotalParseTicks);
                if (WorkflowResult.TotalLexerTicks > 0)
                {
                    Logger.LogInfo($"{"  ANTLR lexing time: ",CliUtils.Align - 2} {lexerTimeSpan.Format()} {lexerPercent}%");
                }
                if (WorkflowResult.TotalParseTicks > 0)
                {
                    Logger.LogInfo($"{"  ANTLR parisng time: ",CliUtils.Align - 2} {parserTimeSpan.Format()} {parserPercent}%");
                }
            }
        }

        protected string CalculateAndFormatPercent(long part, long whole)
        {
            return (whole == 0 ? 0 : ((double)part / whole * 100.0)).ToString("00.00");
        }
    }
}
