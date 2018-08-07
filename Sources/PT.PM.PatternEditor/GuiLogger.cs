using Avalonia.Threading;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.ObjectModel;

namespace PT.PM.PatternEditor
{
    public class GuiLogger : ILogger
    {
        public int ErrorCount { get; private set; }

        internal event EventHandler<string> LogEvent;

        internal readonly ObservableCollection<object> ErrorsCollection;

        internal bool LogPatternErrors { get; set; }

        public bool IsLogDebugs { get; set; }

        public string LogsDir { get; set; } = "";

        public GuiLogger(ObservableCollection<object> errorsCollection)
        {
            ErrorsCollection = errorsCollection;
        }

        public void Clear()
        {
            ErrorCount = 0;
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
            bool logError = WhetherLogError(ex);
            if (logError)
            {
                ErrorCount++;
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
                    logError = !string.IsNullOrEmpty(pmException.CodeFile.PatternKey);
                }
            }

            return logError;
        }
    }
}
