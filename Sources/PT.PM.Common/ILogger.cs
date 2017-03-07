using PT.PM.Common.CodeRepository;
using System;

namespace PT.PM.Common
{
    public interface ILogger
    {
        int ErrorCount { get; }

        ISourceCodeRepository SourceCodeRepository { get; set; }

        void LogError(string message);

        void LogError(string message, Exception ex);

        void LogError(Exception ex);

        void LogInfo(string message);

        void LogInfo(string format, params string[] args);

        void LogInfo(object infoObj);

        void LogDebug(string message);
    }
}
