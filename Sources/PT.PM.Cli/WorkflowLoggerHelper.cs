using PT.PM.Common;
using System;
using System.Linq;

namespace PT.PM.Cli
{
    public class WorkflowLoggerHelper: ILoggable
    {
        public ILogger Logger { get; set; }

        public WorkflowResult WorkflowResult { get; set; }

        public WorkflowLoggerHelper(ILogger logger, WorkflowResult workflowResult)
        {
            Logger = logger;
            WorkflowResult = workflowResult;
        }

        public void LogStatistics()
        {
            Logger.LogInfo("{0,-22} {1}", "Files count:", WorkflowResult.TotalProcessedFilesCount.ToString());
            Logger.LogInfo("{0,-22} {1}", "Chars count:", WorkflowResult.TotalProcessedCharsCount.ToString());
            Logger.LogInfo("{0,-22} {1}", "Lines count:", WorkflowResult.TotalProcessedLinesCount.ToString());
            long totalTimeTicks = WorkflowResult.GetTotalTimeTicks();
            if (totalTimeTicks > 0)
            {
                if (WorkflowResult.Stage >= Stage.Read)
                {
                    LogStageTime(Stage.Read);
                    if (WorkflowResult.Stage >= Stage.Parse)
                    {
                        LogStageTime(Stage.Parse);
                        if (WorkflowResult.Stage >= Stage.Convert)
                        {
                            LogStageTime(Stage.Convert);
                            if (WorkflowResult.Stage >= Stage.Preprocess)
                            {
                                LogStageTime(Stage.Preprocess);
                                if (WorkflowResult.Stage >= Stage.Match)
                                {
                                    LogStageTime(Stage.Match);
                                }
                            }
                        }
                    }
                }
                if (WorkflowResult.Stage >= Stage.Match || WorkflowResult.Stage == Stage.Patterns)
                {
                    LogStageTime(Stage.Patterns);
                }
            }
        }

        protected void LogStageTime(Stage stage)
        {
            long totalTimeTicks = WorkflowResult.GetTotalTimeTicks();
            long ticks = 0;
            switch (stage)
            {
                case Stage.Read:
                    ticks = WorkflowResult.TotalReadTicks;
                    break;
                case Stage.Parse:
                    ticks = WorkflowResult.TotalParseTicks;
                    break;
                case Stage.Convert:
                    ticks = WorkflowResult.TotalConvertTicks;
                    break;
                case Stage.Preprocess:
                    ticks = WorkflowResult.TotalPreprocessTicks;
                    break;
                case Stage.Match:
                    ticks = WorkflowResult.TotalMatchTicks;
                    break;
                case Stage.Patterns:
                    ticks = WorkflowResult.TotalPatternsTicks;
                    break;
            }
            Logger.LogInfo("{0,-22} {1} {2}%",
                "Total " + stage.ToString().ToLowerInvariant() + " time:",
                new TimeSpan(ticks).ToString(), CalculatePercent(ticks, totalTimeTicks).ToString("00.00"));

            if (stage == Stage.Parse)
            {
                LogAdditionalParserInfo();
            }
        }

        protected void LogAdditionalParserInfo()
        {
            if (WorkflowResult.Languages.Any(lang => lang.HaveAntlrParser()))
            {
                Logger.LogInfo("{0,-22} {1} {2}%",
                    "  ANTLR lexing time:",
                    new TimeSpan(WorkflowResult.TotalLexerTicks).ToString(),
                        CalculatePercent(WorkflowResult.TotalLexerTicks, WorkflowResult.TotalParseTicks).ToString("00.00"));
                Logger.LogInfo("{0,-22} {1} {2}%",
                    "  ANTLR parsing time:",
                    new TimeSpan(WorkflowResult.TotalParserTicks).ToString(),
                        CalculatePercent(WorkflowResult.TotalParserTicks, WorkflowResult.TotalParseTicks).ToString("00.00"));
            }
        }

        protected double CalculatePercent(long part, long whole)
        {
            return whole == 0 ? 0 : ((double)part / whole * 100.0);
        }
    }
}
