using PT.PM.Common;
using PT.PM.Common.CodeRepository;
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

        public string LogPath
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
                            fileTarget.FileName = Path.Combine(logPath, fileTarget.FileName.ToString().Replace("'", ""));
                        }
                    }
                }
            }
        }

        public bool LogErrors { get; set; } = false;

        public bool LogDebugs { get; set; } = false;

        public ISourceCodeRepository SourceCodeRepository { get; set; }

        public virtual void LogError(string message)
        {
            ErrorsLogger.Error(message);
            FileLogger.Error(message);
            Interlocked.Increment(ref errorCount);
        }

        public virtual void LogError(string message, Exception ex)
        {
            var exString = message + ": " + GetExceptionMessage(ex);
            ErrorsLogger.Error(exString);
            FileLogger.Error(exString);
            Interlocked.Increment(ref errorCount);
        }

        public virtual void LogError(Exception ex)
        {
            var exString = GetExceptionMessage(ex);
            ErrorsLogger.Error(exString);
            FileLogger.Error(exString);
            Interlocked.Increment(ref errorCount);
        }

        public void LogInfo(string format, params string[] args)
        {
            var str = string.Format(format, args);
            LogInfo(str);
        }

        public virtual void LogInfo(string message)
        {
            FileLogger.Info(message);
        }

        public string GetExceptionMessage(Exception ex)
        {
            string pathString = null;
            int line = 0;
            try
            {
                var index1 = ex.StackTrace.IndexOf(":\\");
                var index2 = ex.StackTrace.IndexOf(":", index1 + 1);
                index1--;
                pathString = ex.StackTrace.Substring(index1, index2 - index1);
                pathString = Path.GetFileName(pathString);

                while (index2 < ex.StackTrace.Length && !char.IsDigit(ex.StackTrace[index2]))
                {
                    index2++;
                }
                int digitIndex = index2;
                while (index2 < ex.StackTrace.Length && char.IsDigit(ex.StackTrace[index2]))
                {
                    index2++;
                }
                line = int.Parse(ex.StackTrace.Substring(digitIndex, index2 - digitIndex));
            }
            catch
            {
            }

            string result;
            if (pathString != null)
            {
                result = string.Format("{0} at \"{1}\" (line: {2})", ex.Message, pathString, line);
            }
            else
            {
                result = ex.ToString();
            }

            return result;
        }

        public abstract void LogInfo(object infoObj);

        public virtual void LogDebug(string message)
        {
            bool debug = false;
#if DEBUG
            debug = true;
#endif
            if (debug || LogDebugs)
            {
                FileLogger.Debug(message);
            }
        }

        protected abstract void LogMatchingResult(MatchingResult matchingResult);
    }
}
