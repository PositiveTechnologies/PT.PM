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
            LogEvent?.Invoke(this, message);
        }

        public void LogError(Exception ex)
        {
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
            ParsingException parseException;
            ConversionException conversionException;
            MatchingException matchException;
            bool logError = LogPatternErrors;
            if ((parseException = ex as ParsingException) != null)
            {
                if (!logError)
                {
                    logError = parseException.FileName != "Pattern";
                }
                parseException.FileName = "";
            }
            if ((conversionException = ex as ConversionException) != null)
            {
                if (!logError)
                {
                    logError = conversionException.FileName != "Pattern";
                }
                conversionException.FileName = "";
            }
            if ((matchException = ex as MatchingException) != null)
            {
                if (!logError)
                {
                    logError = matchException.FileName != "Pattern";
                }
                matchException.FileName = "";
            }

            return logError;
        }
    }
}
