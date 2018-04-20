using PT.PM.Common;
using PT.PM.Common.Exceptions;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using PT.PM.Common.CodeRepository;

namespace PT.PM.PatternEditor
{
    public class GuiLogger : ILogger
    {
        private int errorCount;
        public int ErrorCount => errorCount;

        internal event EventHandler<string> LogEvent;

        internal readonly ObservableCollection<object> ErrorsCollection;

        internal bool LogPatternErrors { get; set; }

        public SourceCodeRepository SourceCodeRepository { get; set; }

        public bool IsLogErrors { get; set; }

        public bool IsLogDebugs { get; set; }

        public string LogsDir { get; set; } = "";

        public GuiLogger(ObservableCollection<object> errorsCollection)
        {
            ErrorsCollection = errorsCollection;
        }

        public void Clear()
        {
            errorCount = 0;
        }

        public void LogDebug(string message)
        {
            if (IsLogDebugs)
            {
                LogEvent?.Invoke(this, message);
            }
        }

        public void LogError(Exception ex)
        {
            if (!IsLogErrors)
            {
                return;
            }

            bool logError = WhetherLogError(ex);
            if (logError)
            {
                errorCount++;
                Dispatcher.UIThread.InvokeAsync(() => ErrorsCollection.Add(ex));
                string message = ex.ToString();
                if (string.IsNullOrEmpty(message))
                {
                    message = ex.InnerException.ToString();
                }
                LogEvent?.Invoke(this, "Error: " + message);
            }
        }

        public void LogInfo(object infoObj)
        {
            LogEvent?.Invoke(this, infoObj.ToString());
        }

        public void LogInfo(string message)
        {
            LogEvent?.Invoke(this, message);
        }

        private bool WhetherLogError(Exception ex)
        {
            bool logError = LogPatternErrors;

            if (ex is PMException pmException)
            {
                if (!logError)
                {
                    logError = !pmException.CodeFile.IsPattern;
                }
            }

            return logError;
        }
    }
}
