using PT.PM.Common;
using PT.PM.Matching;
using System;
using System.Linq;
using static PT.PM.Cli.WorkflowLoggerHelper;

namespace PT.PM.Cli
{
    public abstract class WorkflowLoggerHelperBase<TStage, TWorkflowResult, TPattern, TMatchResult> : ILoggable
        where TStage : struct, IConvertible
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchResult>
        where TMatchResult : MatchResultBase<TPattern>
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
            if (WorkflowResult.TotalTerminatedFilesCount > 0)
            {
                Logger.LogInfo($"{"Terminated files count:",Align} {WorkflowResult.TotalTerminatedFilesCount}");
            }
            Logger.LogInfo($"{"Files count:",Align} {WorkflowResult.TotalProcessedFilesCount}");
            Logger.LogInfo($"{"Chars count:",Align} {WorkflowResult.TotalProcessedCharsCount}");
            Logger.LogInfo($"{"Lines count:",Align} {WorkflowResult.TotalProcessedLinesCount}");
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
                case nameof(Stage.SimplifiedUst):
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
            Logger.LogInfo
                ($"{"Total " + stage.ToString().ToLowerInvariant() + " time:",Align} {new TimeSpan(ticks)} {CalculatePercent(ticks, WorkflowResult.TotalTimeTicks):00.00}%");
            if (stage == nameof(Stage.ParseTree))
            {
                LogAdditionalParserInfo();
            }
        }

        protected abstract long GetTicksCount(string stage);

        protected void LogAdditionalParserInfo()
        {
            if (WorkflowResult.AnalyzedLanguages.Any(lang => lang.HaveAntlrParser))
            {
                TimeSpan lexerTimeSpan = new TimeSpan(WorkflowResult.TotalLexerTicks);
                TimeSpan parserTimeSpan = new TimeSpan(WorkflowResult.TotalParserTicks);
                double lexerPercent = CalculatePercent(WorkflowResult.TotalLexerTicks, WorkflowResult.TotalParseTicks);
                double parserPercent = CalculatePercent(WorkflowResult.TotalParserTicks, WorkflowResult.TotalParseTicks);
                Logger.LogInfo($"{"  ANTLR lexing time: ",Align} {lexerTimeSpan} {lexerPercent:00.00}%");
                Logger.LogInfo($"{"  ANTLR parisng time: ",Align} {parserTimeSpan} {parserPercent:00.00}%");
            }
        }

        protected double CalculatePercent(long part, long whole)
        {
            return whole == 0 ? 0 : ((double)part / whole * 100.0);
        }
    }
}
