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

        protected NLog.Logger MatchLogger => NLog.LogManager.GetLogger("match");

        protected TextTruncater ErrorTruncater { get; } = new TextTruncater
        {
            MaxMessageLength = 300,
            CutWords = false
        };

        protected TextTruncater MessageTruncater { get; } = new TextTruncater();

        protected TextTruncater CodeTruncater { get; } = new TextTruncater
        {
            ReduceWhitespaces = true
        };

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

        public SourceCodeRepository SourceCodeRepository { get; set; }

        public virtual void LogError(Exception ex)
        {
            var exString = ErrorTruncater.Trunc(ex.GetPrettyErrorMessage(FileNameType.Full));
            ErrorsLogger.Error(exString);
            FileInternalLogger.Error(exString);
            Interlocked.Increment(ref errorCount);
        }

        public virtual void LogInfo(object infoObj)
        {
            var progressEventArgs = infoObj as ProgressEventArgs;
            if (progressEventArgs != null)
            {
                LogInfo(progressEventArgs.ToString());
            }
            else
            {
                var matchingResult = infoObj as MatchingResult;
                if (matchingResult != null)
                {
                    var matchingResultDto = new MatchingResultDto(matchingResult);
                    matchingResultDto.MatchedCode = CodeTruncater.Trunc(matchingResultDto.MatchedCode);
                    var json = JsonConvert.SerializeObject(matchingResultDto, Formatting.Indented);
                    MatchLogger.Info(json);
                    LogInfo($"Pattern matched: {Environment.NewLine}{json}{Environment.NewLine}");
                }
                else
                {
                    string match = infoObj.ToString();
                    LogInfo(match);
                }
            }
        }

        public virtual void LogInfo(string message)
        {
            FileInternalLogger.Info(message);
        }

        public virtual void LogDebug(string message)
        {
            if (IsLogDebugs)
            {
                FileInternalLogger.Debug(MessageTruncater.Trunc(message));
            }
        }
    }
}
