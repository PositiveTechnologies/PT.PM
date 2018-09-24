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
            LogAdvancedInfo();

            if (WorkflowResult.TotalTerminatedFilesCount > 0)
            {
                Logger.LogInfo($"{"Terminated files count:",LoggerUtils.Align} {WorkflowResult.TotalTerminatedFilesCount}");
            }
            if (!WorkflowResult.Stage.Is(Stage.Pattern) || WorkflowResult.TotalProcessedFilesCount > 0)
            {
                Logger.LogInfo($"{"Files count:",LoggerUtils.Align} {WorkflowResult.TotalProcessedFilesCount}");
            }
            if (WorkflowResult.TotalProcessedFilesCount > 0)
            {
                Logger.LogInfo($"{"Lines/chars count:",LoggerUtils.Align} {WorkflowResult.TotalProcessedLinesCount} / {WorkflowResult.TotalProcessedCharsCount}");
            }
            Logger.LogInfo($"{"Patterns count:",LoggerUtils.Align} {WorkflowResult.TotalProcessedPatternsCount}");

            Logger.LogInfo($"{"Time format:",LoggerUtils.Align} {Utils.TimeSpanFormat.Replace("\\", "")}");
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
                            LogAdvancedStageInfo();
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

        protected virtual void LogAdvancedInfo()
        {
        }

        protected abstract void LogAdvancedStageInfo();

        protected void LogStageTime(string stage)
        {
            long ticks = 0;
            switch (stage)
            {
                case nameof(Stage.File):
                    ticks = WorkflowResult.TotalReadTicks;
                    break;
                case nameof(Stage.ParseTree):
                    ticks = WorkflowResult.TotalLexerParserTicks;
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
                    ($"{stage + " time:",LoggerUtils.Align} {timeSpan.Format()} {percent}%");
            }
            if (stage == nameof(Stage.ParseTree))
            {
                LogAdditionalParserInfo();
            }
        }

        protected virtual long GetTicksCount(string stage) => 0;

        protected void LogAdditionalParserInfo()
        {
            bool printLexerParserPercents = WorkflowResult.TotalLexerTicks > 0 && WorkflowResult.TotalLexerParserTicks > 0;
            if (WorkflowResult.TotalLexerTicks > 0)
            {
                TimeSpan lexerTimeSpan = new TimeSpan(WorkflowResult.TotalLexerTicks);
                string totalLexerPercent = CalculateAndFormatPercent(WorkflowResult.TotalLexerTicks, WorkflowResult.TotalTimeTicks);
                string lexerPercent = printLexerParserPercents
                    ? $" ({CalculateAndFormatPercent(WorkflowResult.TotalLexerTicks, WorkflowResult.TotalLexerParserTicks)}%)"
                    : "";
                Logger.LogInfo($"{"Lexing time: ",LoggerUtils.Align} {lexerTimeSpan.Format()} {totalLexerPercent}%{lexerPercent}");
            }
            if (WorkflowResult.TotalLexerParserTicks > 0)
            {
                TimeSpan parserTimeSpan = new TimeSpan(WorkflowResult.TotalParserTicks);
                string totalParserPercent = CalculateAndFormatPercent(WorkflowResult.TotalParserTicks, WorkflowResult.TotalTimeTicks);
                string parserPercent = printLexerParserPercents
                    ? $" ({CalculateAndFormatPercent(WorkflowResult.TotalParserTicks, WorkflowResult.TotalLexerParserTicks)}%)"
                    : "";
                Logger.LogInfo($"{"Parsing time: ",LoggerUtils.Align} {parserTimeSpan.Format()} {totalParserPercent}%{parserPercent}");
            }
        }

        protected string CalculateAndFormatPercent(long part, long whole)
        {
            return (whole == 0 ? 0 : ((double)part / whole * 100.0)).ToString("00.00");
        }
    }
}
