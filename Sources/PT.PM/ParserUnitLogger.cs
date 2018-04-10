using PT.PM.Common;
using System;
using System.Collections.Generic;

namespace PT.PM
{
    public class ParserUnitLogger : ILogger
    {
        public List<Exception> Errors { get; } = new List<Exception>();

        public List<object> Infos { get; } = new List<object>();

        public List<string> Debugs { get; } = new List<string>();

        public int ErrorCount => Errors.Count;

        public void LogDebug(string message)
        {
            lock (Debugs)
            {
                Debugs.Add(message);
            }
        }

        public void LogError(Exception ex)
        {
            lock (Errors)
            {
                Errors.Add(ex);
            }
        }

        public void LogInfo(string message)
        {
            lock (Infos)
            {
                Infos.Add(message);
            }
        }

        public void LogInfo(object infoObj)
        {
            lock (Infos)
            {
                Infos.Add(infoObj);
            }
        }
    }
}
