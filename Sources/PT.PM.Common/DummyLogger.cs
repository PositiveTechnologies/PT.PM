using System;

namespace PT.PM.Common
{
    public class DummyLogger : ILogger
    {
        public static DummyLogger Instance = new DummyLogger();

        public int ErrorCount => 0;

        public bool IsLogDebugs { get; set; }

        public string LogsDir { get; set; } = "";

        public void LogDebug(string message)
        {
        }

        public void LogError(Exception ex)
        {
        }

        public void LogError(string message)
        {
        }

        public void LogInfo(object infoObj)
        {
        }

        public void LogInfo(string message)
        {
        }
    }
}
