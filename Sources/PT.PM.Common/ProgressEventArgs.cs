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

        public override string ToString()
        {
            string value = Progress >= 1 ? $"{(int)Progress} items" : $"{(int)(Progress * 100):0.00}%";
            return $"Progress: {value}; File: {CurrentFile}";
        }
    }
}
