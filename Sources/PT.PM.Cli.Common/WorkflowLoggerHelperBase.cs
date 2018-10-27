using PT.PM.Common;
using PT.PM.Matching;
using System;

namespace PT.PM.Cli.Common
{
    public abstract class WorkflowLoggerHelperBase<TStage, TWorkflowResult, TPattern, TMatchResult, TRenderStage> : ILoggable
        where TStage : Enum
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchResult, TRenderStage>
        where TMatchResult : MatchResultBase<TPattern>
        where TRenderStage : Enum
    {
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

            string extraTimeInfo = WorkflowResult.ThreadCount == 1 ? "" : $" (average per thread)";
            Logger.LogInfo($"{"Time format:",LoggerUtils.Align} {Utils.TimeSpanFormat.Replace("\\", "")}{extraTimeInfo}");
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
                long avgTicksPerThread = IsMultithreadStage(stage)
                    ? ticks / (WorkflowResult.ThreadCount == 0 ? Environment.ProcessorCount : WorkflowResult.ThreadCount)
                    : ticks;
                var timeSpan = new TimeSpan(avgTicksPerThread);
                string percent = CalculateAndFormatPercent(ticks, WorkflowResult.TotalTimeTicks);

                string extraInfo = "";
                if (stage == nameof(Stage.ParseTree))
                {
                    string lexerPercent = CalculateAndFormatPercent(WorkflowResult.TotalLexerTicks, WorkflowResult.TotalLexerParserTicks);
                    string parserPercent = CalculateAndFormatPercent(WorkflowResult.TotalParserTicks, WorkflowResult.TotalLexerParserTicks);
                    extraInfo = $" (Lexer: {lexerPercent}% + Parser: {parserPercent}%)";
                }

                Logger.LogInfo($"{stage + " time:",LoggerUtils.Align} {timeSpan.Format()} {percent}%{extraInfo}");
            }
        }

        protected virtual long GetTicksCount(string stage) => 0;

        protected virtual bool IsMultithreadStage(string stage)
        {
            return stage != nameof(Stage.Pattern);
        }

        protected string CalculateAndFormatPercent(long part, long whole)
        {
            return (whole == 0 ? 0 : ((double)part / whole * 100.0)).ToString("00.00");
        }
    }
}
