using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Matching;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PT.PM.TestUtils
{
    public class LoggerMessageCounter : ILogger
    {
        private List<string> infoMessages = new List<string>();
        private List<string> debugMessages = new List<string>();

        private HashSet<string> errorFiles = new HashSet<string>();

        private List<IMatchResultBase> matches = new List<IMatchResultBase>();

        public int ErrorCount => Errors.Count;

        public int InfoMessageCount => infoMessages.Count;

        public List<string> Errors { get; } = new List<string>();

        public bool LogToConsole { get; set; }

        public string ErrorsString => string.Join(", " + Environment.NewLine, Errors);

        public bool IsLogDebugs { get; set; } = true;

        public string LogsDir { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public IReadOnlyList<IMatchResultBase> Matches => matches;

        public void LogError(Exception ex)
        {
            string message = ex.ToString();
            lock (Errors)
            {
                Errors.Add(message);
                if (ex is ParsingException parsingException)
                {
                    errorFiles.Add(parsingException.File.RelativeName);
                }
            }
            message = message + Environment.NewLine;
            LogToConsoleIfNeeded(message);
            Debug.Write(message);
        }

        public void LogInfo(object infoObj)
        {
            string message;
            if (infoObj is ProgressEventArgs progressEventArgs)
            {
                message = string.Format("Progress: {0}%; File: {1}{2}",
                    (int)(progressEventArgs.Progress * 100), progressEventArgs.CurrentFile,
                    Math.Abs(progressEventArgs.Progress - 1) < 1e-10 ? Environment.NewLine : "");
            }
            else
            {
                if (infoObj is IMatchResultBase matchResult)
                {
                    lock (matches)
                    {
                        matches.Add(matchResult);
                    }
                }

                message = infoObj.ToString();
            }
            message = message.TrimEnd() + Environment.NewLine;
            lock (infoMessages)
            {
                infoMessages.Add(message);
            }
            LogToConsoleIfNeeded(message);
        }

        public void LogInfo(string message)
        {
            lock (infoMessages)
            {
                infoMessages.Add(message);
            }
            message = message.TrimEnd() + Environment.NewLine;
            LogToConsoleIfNeeded(message);
        }

        public void LogDebug(string message)
        {
            if (!IsLogDebugs)
            {
                return;
            }

            lock (debugMessages)
            {
                debugMessages.Add(message);
            }
            message = message.TrimEnd() + Environment.NewLine;
            LogToConsoleIfNeeded(message);
            Debug.Write(message);
        }

        private void LogToConsoleIfNeeded(string message)
        {
            if (LogToConsole)
            {
                Console.Write(message);
            }
        }
    }
}
