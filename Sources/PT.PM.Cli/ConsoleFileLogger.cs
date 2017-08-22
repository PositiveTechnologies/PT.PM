using System;
using NLog;
using PT.PM.Common.Exceptions;

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
                NLogConsoleLogger.Error(MessageTruncater.Trunc(ex.GetPrettyErrorMessage(FileNameType.Short)));
            }
        }

        public override void LogInfo(string message)
        {
            base.LogInfo(message);
            NLogConsoleLogger.Info(PrepareForConsole(message));
        }

        public override void LogDebug(string message)
        {
            if (IsLogDebugs)
            {
                string truncatedMessage = MessageTruncater.Trunc(message);
                base.LogDebug(truncatedMessage);
                NLogConsoleLogger.Debug(PrepareForConsole(truncatedMessage));
            }
        }

        protected string PrepareForConsole(string str)
        {
            return str.Replace("\a", "");
        }
    }
}
