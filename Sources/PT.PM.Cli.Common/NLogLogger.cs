using NLog;
using NLog.Targets;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Utils;
using PT.PM.Matching;
using System;
using System.IO;
using System.Text;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.Cli.Common
{
    public class NLogLogger : PM.Common.ILogger
    {
        private int errorCount;
        private string logPath;

        public int ErrorCount => errorCount;

        public Logger FileLogger { get; } = LogManager.GetLogger("file");

        public Logger ErrorsLogger { get; } = LogManager.GetLogger("errors");

        public Logger MatchLogger { get; } = LogManager.GetLogger("match");

        protected Logger ConsoleLogger { get; } = LogManager.GetLogger("console");

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
            TrimIndent = true,
            ReduceWhitespaces = true,
            MaxMessageLength = 300
        };

        public string LogsDir
        {
            get => logPath;
            set
            {
                logPath = value;
                if (!string.IsNullOrEmpty(logPath))
                {
                    foreach (Target target in LogManager.Configuration.AllTargets)
                    {
                        if (target is FileTarget fileTarget)
                        {
                            string fullFileName = fileTarget.FileName.ToString().Replace("'", "");
                            fileTarget.FileName = Path.Combine(logPath, Path.GetFileName(fullFileName)).NormalizeFilePath();
                        }
                    }
                }
            }
        }

        public bool IsLogDebugs { get; set; } = false;

        public bool IsLogToFile { get; set; } = true;

        public bool IsLogToConsole { get; set; } = true;

        public virtual void LogError(Exception ex)
        {
            var exString = ErrorPrinter.Print(ex.GetPrettyErrorMessage(FileNameType.Full));
            ErrorsLogger.Error(exString);

            if (IsLogToFile)
            {
                FileLogger.Error(exString);
            }

            if (IsLogToConsole)
            {
                ConsoleLogger.Error(MessagePrinter.Print(ex.GetPrettyErrorMessage(FileNameType.Relative)));
            }

            Interlocked.Increment(ref errorCount);
        }

        public virtual void LogInfo(object infoObj)
        {
            try
            {
                switch (infoObj)
                {
                    case ProgressEventArgs progressEventArgs:
                        LogInfo(progressEventArgs.ToString());
                        ProcessProgressEventArgs(progressEventArgs);
                        break;

                    case MessageEventArgs messageEventArgs:
                        ProcessMessageEventArgs(messageEventArgs);
                        break;

                    case MatchResult matchResult:
                        ProcessMatchResult(matchResult.TextSpan, matchResult.SourceCodeFile,
                            matchResult.Pattern.Key, matchResult.Suppressed);
                        break;

                    default:
                        LogInfo($"Unknown object {infoObj}");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public virtual void LogInfo(string message)
        {
            if (IsLogToFile)
            {
                FileLogger.Info(message);
            }

            if (IsLogToConsole)
            {
                ConsoleLogger.Info(PrepareForConsole(message));
            }
        }

        public virtual void LogDebug(string message)
        {
            if (IsLogDebugs)
            {
                string truncatedMessage = MessagePrinter.Print(message);

                if (IsLogToFile)
                {
                    FileLogger.Debug(truncatedMessage);
                }

                if (IsLogToConsole)
                {
                    ConsoleLogger.Debug(PrepareForConsole(truncatedMessage));
                }
            }
        }

        protected virtual void ProcessProgressEventArgs(ProgressEventArgs progressEventArgs)
        {
        }

        protected virtual void ProcessMessageEventArgs(MessageEventArgs messageEventArgs)
        {
        }

        protected virtual void ProcessMatchResult(TextSpan textSpan, CodeFile sourceFile, string patternKey, bool isSuppressed)
        {
            ExtractLogInfo(textSpan, sourceFile,
                out LineColumnTextSpan lineColumnTextSpan, out string code);

            LogMatch(code, lineColumnTextSpan, patternKey, false, isSuppressed);
        }

        protected void ExtractLogInfo(TextSpan textSpan, CodeFile sourceFile, out LineColumnTextSpan lineColumnTextSpan, out string code)
        {
            lineColumnTextSpan = sourceFile.GetLineColumnTextSpan(textSpan);
            code = sourceFile.Data.Substring(textSpan);
            code = CodePrinter.Print(code);
        }

        protected string PrepareForConsole(string str)
        {
            return str.Replace("\a", "");
        }

        protected void LogMatch(string matchedCode, LineColumnTextSpan textSpan, string patternKey,
            bool taint, bool isSuppressed)
        {
            var message = new StringBuilder();
            message.AppendLine($"-- Match ----------------");
            if (taint || isSuppressed)
            {
                message.AppendLine($"Type      : {(taint ? "Taint; " : "")}{(isSuppressed ? "Suppressed" : "")}");
            }
            message.AppendLine($"Code      : {matchedCode}");
            message.AppendLine($"Location  : {textSpan}");
            message.AppendLine($"Pattern   : {patternKey}");
            string text = message.ToString();

            MatchLogger.Info(text);
            LogInfo(text);
        }
    }
}
