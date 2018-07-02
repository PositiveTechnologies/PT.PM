using System;

namespace PT.PM.Common
{
    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; }

        public string CurrentFile { get; }

        public string Message { get; }

        public ProgressEventArgs(double progress, string fileName, string message = "")
        {
            Progress = progress;
            CurrentFile = fileName;
            Message = message;
        }

        public override string ToString()
        {
            string value = Progress > 1 ? $"{(int)Progress} items" : $"{Progress*100:0.00}%";
            string messageStr = string.IsNullOrEmpty(Message) ? "" : $"; {Message}";
            string fileStr = string.IsNullOrEmpty(CurrentFile) ? "" : $"; File: {CurrentFile}";
            return $"Progress: {value}{messageStr}{fileStr}";
        }
    }
}
