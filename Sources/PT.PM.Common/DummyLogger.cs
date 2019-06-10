using System;

namespace PT.PM.Common
{
    public class DummyLogger : ILogger
    {
        public static readonly DummyLogger Instance = new DummyLogger();

        public int ErrorCount => 0;

        public LogLevel LogLevel
        {
            get => LogLevel.Off;
            set => throw new InvalidOperationException($"{nameof(LogLevel)} property is read-only for {nameof(DummyLogger)}");
        }

        public string LogsDir { get; set; } = "";

        public Action<IMatchResultBase> ProcessMatchResultAction { get; set; }

        public void LogDebug(string message)
        {
        }

        public void LogError(Exception ex)
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
