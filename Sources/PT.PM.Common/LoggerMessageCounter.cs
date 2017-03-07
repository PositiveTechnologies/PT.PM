using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using PT.PM.Common.CodeRepository;

namespace PT.PM.Common
{
    public class LoggerMessageCounter : ILogger
    {
        private List<string> errorMessages = new List<string>();
        private List<string> infoMessages = new List<string>();
        private List<string> debugMessages = new List<string>();

        private HashSet<string> errorFiles = new HashSet<string>();

        public int ErrorCount => errorMessages.Count;

        public double ErrorFilesCount => errorFiles.Count;

        public int InfoMessageCount => infoMessages.Count;

        public bool LogToConsole { get; set; }

        public string ErrorsString => string.Join(", " + Environment.NewLine, errorMessages);

        public ISourceCodeRepository SourceCodeRepository { get; set; }

        public void LogError(Exception ex)
        {
            string message = ex.ToString();
            lock (errorMessages)
            {
                errorMessages.Add(message);
                var parsingException = ex as ParsingException;
                if (parsingException != null)
                {
                    errorFiles.Add(parsingException.FileName);
                }
            }
            message = message + Environment.NewLine;
            LogToConsoleIfNeeded(message);
            Debug.Write(message);
        }

        public void LogError(string message)
        {
            lock (errorMessages)
            {
                errorMessages.Add(message);
            }
            message = message.TrimEnd() + Environment.NewLine;
            LogToConsoleIfNeeded(message);
            Debug.Write(message);
        }

        public void LogError(string message, Exception ex)
        {
            string output = message + ex.ToString();
            lock (errorMessages)
            {
                errorMessages.Add(output);
            }
            output = output.TrimEnd() + Environment.NewLine;
            LogToConsoleIfNeeded(output);
            Debug.Write(output);
        }

        public void LogInfo(object infoObj)
        {
            string message;
            var progressEventArgs = infoObj as ProgressEventArgs;
            if (progressEventArgs != null)
            {
                message = string.Format("Progress: {0}%; File: {1}{2}",
                    (int)(progressEventArgs.Progress * 100), progressEventArgs.CurrentFile,
                    Math.Abs(progressEventArgs.Progress - 1) < 1e-10 ? Environment.NewLine : "");
            }
            else
            {
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

        public void LogInfo(string format, params string[] args)
        {
            var message = string.Format(format, args);
            lock (infoMessages)
            {
                infoMessages.Add(message);
            }
            message = message.TrimEnd() + Environment.NewLine;
            LogToConsoleIfNeeded(message);
        }

        public bool ContainsErrorMessagePart(string errorMessagePart)
        {
            return errorMessages.Any(msg => msg.Contains(errorMessagePart));
        }

        public bool ContainsDebugMessagePart(string debugMessagePart)
        {
            return debugMessages.Any(msg => msg.Contains(debugMessagePart));
        }

        public void LogDebug(string message)
        {
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
