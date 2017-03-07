using System;

namespace PT.PM.Common
{
    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; set; }

        public string CurrentFile { get; set; }

        public ProgressEventArgs(double progress, string fileName)
        {
            Progress = progress;
            CurrentFile = fileName;
        }
    }
}
