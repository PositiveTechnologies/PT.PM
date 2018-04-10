﻿using System;

namespace PT.PM.Common
{
    public interface ILogger
    {
        int ErrorCount { get; }

        void LogError(Exception ex);

        void LogInfo(string message);

        void LogInfo(object infoObj);

        void LogDebug(string message);
    }
}
