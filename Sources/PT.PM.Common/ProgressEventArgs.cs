using System;

namespace PT.PM.Common
{
    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; }

        public string CurrentFile { get; }

        public string Message { get; }

        public DateTime Date { get; }

        public ProgressEventArgs(double progress, string fileName, string message = "", DateTime date = default)
        {
            Progress = progress;
            CurrentFile = fileName;
            Message = message;
            Date = date == default ? DateTime.Now : date;
        }

        public override string ToString()
        {
            string value = Progress > 1 ? $"{(int)Progress} items" : $"{Progress*100:0.00}%";
            string messageStr = string.IsNullOrEmpty(Message) ? "" : $"; {Message}";
            string fileStr = string.IsNullOrEmpty(CurrentFile) ? "" : $"; File: {CurrentFile}";
            return $"Progress: {value} at {Date.ToLongTimeString()}{messageStr}{fileStr}";
        }
    }
}
