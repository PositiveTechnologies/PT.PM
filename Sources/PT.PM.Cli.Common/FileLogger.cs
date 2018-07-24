using NLog.Targets;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Matching;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace PT.PM.Cli.Common
{
    public class FileLogger : ILogger
    {
        private int errorCount;
        private string logPath;

        public int ErrorCount => errorCount;

        public NLog.Logger FileInternalLogger => NLog.LogManager.GetLogger("file");

        public NLog.Logger ErrorsLogger => NLog.LogManager.GetLogger("errors");

        public NLog.Logger MatchLogger => NLog.LogManager.GetLogger("match");

        protected PrettyPrinter ErrorPrinter { get; } = new PrettyPrinter
        {
            MaxMessageLength = 300,
            CutWords = false
        };

        protected PrettyPrinter MessagePrinter { get; } = new PrettyPrinter
        {
            MaxMessageLength = 300,
        };

        protected PrettyPrinter CodePrinter { get; } = new PrettyPrinter
        {
            ReduceWhitespaces = true
        };

        public string LogsDir
        {
            get => logPath;
            set
            {
                logPath = value;
                if (!string.IsNullOrEmpty(logPath))
                {
                    foreach (Target target in NLog.LogManager.Configuration.AllTargets)
                    {
                        if (target is FileTarget fileTarget)
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

        public virtual void LogError(Exception ex)
        {
            var exString = ErrorPrinter.Print(ex.GetPrettyErrorMessage(FileNameType.Full));
            ErrorsLogger.Error(exString);
            FileInternalLogger.Error(exString);
            Interlocked.Increment(ref errorCount);
        }

        public virtual void LogInfo(object infoObj)
        {
            if (infoObj is ProgressEventArgs progressEventArgs)
            {
                LogInfo(progressEventArgs.ToString());
            }
            else
            {
                if (infoObj is MatchResult matchResult)
                {
                    var matchResultDto = new MatchResultDto(matchResult);
                    string matchedCode = CodePrinter.Print(matchResultDto.MatchedCode);

                    string matchMessage = LoggerUtils.LogMatch(matchedCode, matchResultDto.LineColumnTextSpan,
                        matchResult.SourceCodeFile,
                        matchResultDto.PatternKey, false, false);

                    MatchLogger.Info(matchMessage);
                    LogInfo(matchMessage);
                }
                else if (!(infoObj is MessageEventArgs))
                {
                    var message = new StringBuilder();
                    message.AppendLine("---------------------");
                    message.AppendLine(infoObj.ToString());
                    string result = message.ToString();
                    if (infoObj is IMatchResultBase)
                    {
                        MatchLogger.Info(result);
                    }
                    LogInfo(result);
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
                FileInternalLogger.Debug(MessagePrinter.Print(message));
            }
        }
    }
}
