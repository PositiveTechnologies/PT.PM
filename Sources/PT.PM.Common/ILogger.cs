using System;

namespace PT.PM.Common
{
    public interface ILogger
    {
        int ErrorCount { get; }

        LogLevel LogLevel { get; set; }

        string LogsDir { get; set; }

        void LogError(Exception ex);

        void LogInfo(string message);

        void LogInfo(object infoObj);

        void LogDebug(string message);

        Action<IMatchResultBase> ProcessMatchResultAction { get; set; }
    }
}
