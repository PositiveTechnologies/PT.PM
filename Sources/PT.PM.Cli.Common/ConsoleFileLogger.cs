using System;
using NLog;
using PT.PM.Common.Exceptions;

namespace PT.PM.Cli.Common
{
    public class ConsoleFileLogger : FileLogger
    {
        protected Logger NLogConsoleLogger { get; } = LogManager.GetLogger("console");

        public override void LogError(Exception ex)
        {
            base.LogError(ex);
            if (IsLogErrors)
            {
                NLogConsoleLogger.Error(MessagePrinter.Print(ex.GetPrettyErrorMessage(FileNameType.Relative)));
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
                string truncatedMessage = MessagePrinter.Print(message);
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
