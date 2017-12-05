using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Exceptions;
using PT.PM.Matching;
using System;
using System.IO;
using System.Text;
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

        protected PrettyPrinter ErrorPrinter { get; } = new PrettyPrinter
        {
            MaxMessageLength = 300,
            CutWords = false
        };

        protected PrettyPrinter MessagePrinter { get; } = new PrettyPrinter();

        protected PrettyPrinter CodePrinter { get; } = new PrettyPrinter
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
                        if (target is NLog.Targets.FileTarget fileTarget)
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

                    var message = new StringBuilder();
                    message.AppendLine("---------------------");
                    message.AppendLine($"{nameof(MatchResultDto.MatchedCode)}: {matchedCode}");
                    message.AppendLine($"Location: [{matchResultDto.BeginLine};{matchResultDto.BeginColumn}] - [{matchResultDto.EndLine};{matchResultDto.EndColumn}]");
                    message.AppendLine($"{nameof(MatchResultDto.PatternKey)}: {matchResultDto.PatternKey}");
                    message.AppendLine($"{nameof(MatchResultDto.SourceFile)}: {matchResultDto.SourceFile}");
                    string result = message.ToString();

                    MatchLogger.Info(result);
                    LogInfo(result);
                }
                else if (!(infoObj is MessageEventArgs))
                {
                    string message = infoObj.ToString();
                    if (infoObj is IMatchResultBase)
                    {
                        MatchLogger.Info(message);
                    }
                    LogInfo(message);
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
