using Avalonia.Threading;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.PatternEditor.ViewModels;
using System;
using System.Collections.ObjectModel;
using PT.PM.Matching;

namespace PT.PM.PatternEditor
{
    public class GuiLogger : ILogger
    {
        public int ErrorCount { get; private set; }

        internal event EventHandler<string> LogEvent;

        internal readonly ObservableCollection<ErrorViewModel> ErrorsCollection;

        internal readonly ObservableCollection<MatchResultViewModel> MatchResultsCollection;

        internal bool IsPatternLogger { get; }

        public bool IsLogDebugs { get; set; }

        public string LogsDir { get; set; } = "";

        public static GuiLogger CreateSourceLogger(ObservableCollection<ErrorViewModel> errorsCollection,
            ObservableCollection<MatchResultViewModel> matchResultCollection)
        {
            return new GuiLogger(errorsCollection, false, matchResultCollection);
        }

        public static GuiLogger CreatePatternLogger(ObservableCollection<ErrorViewModel> errorsCollection) =>
            new GuiLogger(errorsCollection, true, null);

        private GuiLogger(ObservableCollection<ErrorViewModel> errorsCollection, bool logPatternErrors,
            ObservableCollection<MatchResultViewModel> matchResultsCollection)
        {
            ErrorsCollection = errorsCollection;
            IsPatternLogger = logPatternErrors;
            MatchResultsCollection = matchResultsCollection;
        }

        public void Clear()
        {
            ErrorCount = 0;
            if (MatchResultsCollection != null)
            {
                Dispatcher.UIThread.InvokeAsync(MatchResultsCollection.Clear);
            }
            Dispatcher.UIThread.InvokeAsync(ErrorsCollection.Clear);
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
            bool logToGui = true;

            if (ex is PMException pmException)
            {
                logToGui = !(IsPatternLogger ^ pmException.File.PatternKey != null);
            }

            if (logToGui)
            {
                ErrorCount++;
                Dispatcher.UIThread.InvokeAsync(() => ErrorsCollection.Add(new ErrorViewModel(ex)));
            }

            string message = ex.ToString();
            if (string.IsNullOrEmpty(message))
            {
                message = ex.InnerException.ToString();
            }
            LogEvent?.Invoke(this, "Error: " + message);
        }
        
        public void LogInfo(object infoObj)
        {
            if (infoObj is MatchResult matchResult && MatchResultsCollection != null)
            {
                var matchResultViewModel = new MatchResultViewModel(new MatchResultDto(matchResult));
                Dispatcher.UIThread.InvokeAsync(() => MatchResultsCollection.Add(matchResultViewModel));
            }

            LogEvent?.Invoke(this, infoObj.ToString());
        }

        public void LogInfo(string message)
        {
            LogEvent?.Invoke(this, message);
        }
    }
}
