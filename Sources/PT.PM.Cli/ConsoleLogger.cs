using System;
using PT.PM.Common;
using PT.PM.Matching;
using Newtonsoft.Json;
using NLog;

namespace PT.PM.Cli
{
    public class ConsoleLogger : AbstractLogger
    {
        protected Logger NLogConsoleLogger { get; } = LogManager.GetLogger("console");

        protected Logger MatchLogger { get; } = LogManager.GetLogger("match");

        public override void LogError(string message)
        {
            base.LogError(message);
            if (IsLogErrors)
            {
                NLogConsoleLogger.Error("Error: " + PrepareForConsole(message));
            }
        }

        public override void LogError(Exception ex)
        {
            base.LogError(ex);
            if (IsLogErrors)
            {
                NLogConsoleLogger.Error("Error: {0}", PrepareForConsole(ex.Message));
            }
        }

        public override void LogInfo(string message)
        {
            base.LogInfo(message);
            NLogConsoleLogger.Info(PrepareForConsole(message));
        }

        public override void LogInfo(object infoObj)
        {
            string message;
            var progressEventArgs = infoObj as ProgressEventArgs;
            if (progressEventArgs != null)
            {
                bool endFile = Math.Abs(progressEventArgs.Progress - 1) < 1e-10;
                message = $"Progress: {(int)(progressEventArgs.Progress * 100)}%; File: {progressEventArgs.CurrentFile}";
                NLogConsoleLogger.Info(PrepareForConsole(message));
                FileLogger.Info(message);
            }
            else
            {
                var matchingResult = infoObj as MatchingResult;
                if (matchingResult != null)
                {
                    LogMatchingResult(matchingResult);
                }
            }
        }

        public override void LogDebug(string message)
        {
            bool debug = false;
#if DEBUG
            debug = true;
#endif
            if (debug || IsLogDebugs)
            {
                base.LogDebug(message);
                NLogConsoleLogger.Debug(PrepareForConsole(message));
            }
        }

        protected override void LogMatchingResult(MatchingResult matchingResult)
        {
            var matchingResultDto = MatchingResultDto.CreateFromMatchingResult(matchingResult, SourceCodeRepository);
            var json = JsonConvert.SerializeObject(matchingResultDto, Formatting.Indented);
            NLogConsoleLogger.Info("Pattern matched:");
            NLogConsoleLogger.Info(json);
            NLogConsoleLogger.Info("");
            FileLogger.Info("Pattern matched:" + Environment.NewLine + json + Environment.NewLine);
            MatchLogger.Info(json);
        }

        protected string PrepareForConsole(string str)
        {
            return str.Replace("\a", "");
        }
    }
}
