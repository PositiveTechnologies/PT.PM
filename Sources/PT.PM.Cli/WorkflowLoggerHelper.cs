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
            Logger.LogInfo($"{"Files count:",-22} {WorkflowResult.TotalProcessedFilesCount}");
            Logger.LogInfo($"{"Chars count:",-22} {WorkflowResult.TotalProcessedCharsCount}");
            Logger.LogInfo($"{"Lines count:",-22} {WorkflowResult.TotalProcessedLinesCount}");
            long totalTimeTicks = WorkflowResult.GetTotalTimeTicks();
            if (totalTimeTicks > 0)
            {
                if (WorkflowResult.Stage >= Stage.File)
                {
                    LogStageTime(Stage.File);
                    if (WorkflowResult.Stage >= Stage.ParseTree)
                    {
                        LogStageTime(Stage.ParseTree);
                        if (WorkflowResult.Stage >= Stage.Ust)
                        {
                            LogStageTime(Stage.Ust);
                            if (WorkflowResult.Stage >= Stage.SimpleParseTree)
                            {
                                LogStageTime(Stage.SimpleParseTree);
                                if (WorkflowResult.Stage >= Stage.Match)
                                {
                                    LogStageTime(Stage.Match);
                                }
                            }
                        }
                    }
                }
                if (WorkflowResult.Stage >= Stage.Match || WorkflowResult.Stage == Stage.Pattern)
                {
                    LogStageTime(Stage.Pattern);
                }
            }
        }

        protected void LogStageTime(Stage stage)
        {
            long totalTimeTicks = WorkflowResult.GetTotalTimeTicks();
            long ticks = 0;
            switch (stage)
            {
                case Stage.File:
                    ticks = WorkflowResult.TotalReadTicks;
                    break;
                case Stage.ParseTree:
                    ticks = WorkflowResult.TotalParseTicks;
                    break;
                case Stage.Ust:
                    ticks = WorkflowResult.TotalConvertTicks;
                    break;
                case Stage.SimpleParseTree:
                    ticks = WorkflowResult.TotalPreprocessTicks;
                    break;
                case Stage.Match:
                    ticks = WorkflowResult.TotalMatchTicks;
                    break;
                case Stage.Pattern:
                    ticks = WorkflowResult.TotalPatternsTicks;
                    break;
            }
            Logger.LogInfo
                ($"{"Total " + stage.ToString().ToLowerInvariant() + " time:",-22} {new TimeSpan(ticks)} {CalculatePercent(ticks, totalTimeTicks):00.00}%");
            if (stage == Stage.ParseTree)
            {
                LogAdditionalParserInfo();
            }
        }

        protected void LogAdditionalParserInfo()
        {
            if (WorkflowResult.AnalyzedLanguages.Any(lang => lang.HaveAntlrParser))
            {
                TimeSpan lexerTimeSpan = new TimeSpan(WorkflowResult.TotalLexerTicks);
                TimeSpan parserTimeSpan = new TimeSpan(WorkflowResult.TotalParserTicks);
                double lexerPercent = CalculatePercent(WorkflowResult.TotalLexerTicks, WorkflowResult.TotalParseTicks);
                double parserPercent = CalculatePercent(WorkflowResult.TotalParserTicks, WorkflowResult.TotalParseTicks);
                Logger.LogInfo($"{"  ANTLR lexing time: ",-22} {lexerTimeSpan} {lexerPercent:00.00}%");
                Logger.LogInfo($"{"  ANTLR parisng time: ",-22} {parserTimeSpan} {parserPercent:00.00}%");
            }
        }

        protected double CalculatePercent(long part, long whole)
        {
            return whole == 0 ? 0 : ((double)part / whole * 100.0);
        }
    }
}
