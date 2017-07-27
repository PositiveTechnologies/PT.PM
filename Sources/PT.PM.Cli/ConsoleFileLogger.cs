using System;
using NLog;

namespace PT.PM.Cli
{
    public class ConsoleFileLogger : FileLogger
    {
        protected Logger NLogConsoleLogger { get; } = LogManager.GetLogger("console");

        public override void LogError(Exception ex)
        {
            base.LogError(ex);
            if (IsLogErrors)
            {
                NLogConsoleLogger.Error($"Error: {PrepareForConsole(ex.Message)}");
            }
        }

        public override void LogInfo(string message)
        {
            base.LogInfo(message);
            NLogConsoleLogger.Info(PrepareForConsole(message));
        }

        public override void LogDebug(string message)
        {
            base.LogDebug(message);
            if (IsLogDebugs)
            {
                NLogConsoleLogger.Debug(PrepareForConsole(message));
            }
        }

        protected string PrepareForConsole(string str)
        {
            return str.Replace("\a", "");
        }
    }
}
