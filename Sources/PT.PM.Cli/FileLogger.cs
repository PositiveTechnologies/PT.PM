using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Exceptions;
using PT.PM.Matching;
using System;
using System.IO;
using System.Threading;

namespace PT.PM.Cli
{
    public class FileLogger : ILogger
    {
        private int errorCount;
        private string logPath;

        public int ErrorCount => errorCount;

        protected NLog.Logger FileInternalLogger => NLog.LogManager.GetLogger("file");

        protected NLog.Logger ErrorsLogger => NLog.LogManager.GetLogger("errors");

        protected NLog.Logger MatchLogger { get; } = NLog.LogManager.GetLogger("match");

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
            FileInternalLogger.Error(message);
            Interlocked.Increment(ref errorCount);
        }

        public virtual void LogError(Exception ex)
        {
            var exString = ex.FormatExceptionMessage();
            ErrorsLogger.Error(exString);
            FileInternalLogger.Error(exString);
            Interlocked.Increment(ref errorCount);
        }

        public virtual void LogInfo(object infoObj)
        {
            string message;
            var progressEventArgs = infoObj as ProgressEventArgs;
            if (progressEventArgs != null)
            {
                string value = progressEventArgs.Progress >= 1
                    ? $"{(int)progressEventArgs.Progress} items"
                    : $"{(int)(progressEventArgs.Progress * 100):0.00}%";
                message = $"Progress: {value}; File: {progressEventArgs.CurrentFile}";
                LogInfo(message);
            }
            else
            {
                var matchingResult = infoObj as MatchingResult;
                if (matchingResult != null)
                {
                    var matchingResultDto = MatchingResultDto.CreateFromMatchingResult(matchingResult, SourceCodeRepository);
                    var json = JsonConvert.SerializeObject(matchingResultDto, Formatting.Indented);
                    MatchLogger.Info(json);
                    LogInfo($"Pattern matched: {Environment.NewLine}{json}{Environment.NewLine}");
                }
            }
        }

        public virtual void LogInfo(string message)
        {
            FileInternalLogger.Info(message);
        }

        public virtual void LogDebug(string message)
        {
            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            if (isDebug || IsLogDebugs)
            {
                FileInternalLogger.Debug(message);
            }
        }
    }
}
