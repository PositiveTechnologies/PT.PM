using PT.PM.Common.CodeRepository;
using System;

namespace PT.PM.Common
{
    public interface ILogger
    {
        int ErrorCount { get; }

        SourceCodeRepository SourceCodeRepository { get; set; }

        void LogError(Exception ex);

        void LogInfo(string message);

        void LogInfo(object infoObj);

        void LogDebug(string message);
    }
}
