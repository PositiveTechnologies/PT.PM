using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Exceptions;
using PT.PM.Matching;
using System;
using System.IO;
using System.Threading;

namespace PT.PM.Cli
{
    public abstract class AbstractLogger : ILogger
    {
        private int errorCount;
        private string logPath;

        public int ErrorCount => errorCount;

        protected virtual NLog.Logger FileLogger => NLog.LogManager.GetLogger("file");

        protected NLog.Logger ErrorsLogger => NLog.LogManager.GetLogger("errors");

        public string LogsDir
        {
            get { return logPath; }
            set
            {
                logPath = value;
                if (!string.IsNullOrEmpty(logPath))
                {
                    foreach (var target in NLog.LogManager.Configuration.AllTargets)
                    {
                        var fileTarget = target as NLog.Targets.FileTarget;
                        if (fileTarget != null)
                        {
                            string fullFileName = fileTarget.FileName.ToString().Replace("'", "");
                            fileTarget.FileName = Path.Combine(logPath, Path.GetFileName(fullFileName));
                        }
                    }
                }
            }
        }

        public bool IsLogErrors { get; set; } = false;

        public bool IsLogDebugs { get; set; } = false;

        public ISourceCodeRepository SourceCodeRepository { get; set; }

        public virtual void LogError(string message)
        {
            ErrorsLogger.Error(message);
            FileLogger.Error(message);
            Interlocked.Increment(ref errorCount);
        }

        public virtual void LogError(Exception ex)
        {
            var exString = ex.FormatExceptionMessage();
            ErrorsLogger.Error(exString);
            FileLogger.Error(exString);
            Interlocked.Increment(ref errorCount);
        }

        public virtual void LogInfo(string message)
        {
            FileLogger.Info(message);
        }

        public abstract void LogInfo(object infoObj);

        public virtual void LogDebug(string message)
        {
            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            if (isDebug || IsLogDebugs)
            {
                FileLogger.Debug(message);
            }
        }

        protected abstract void LogMatchingResult(MatchingResult matchingResult);
    }
}
