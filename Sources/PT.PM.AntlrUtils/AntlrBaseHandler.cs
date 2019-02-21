using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Atn;
using PT.PM.Common;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrBaseHandler
    {
        public static ILogger StaticLogger { get; set; } = DummyLogger.Instance;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public abstract Language Language { get; }
        public bool UseFastParseStrategyAtFirst { get; set; } = true;

        public static long MemoryConsumptionBytes { get; set; } = 3 * 1024 * 1024 * 1024L;

        public static long ClearCacheFilesBytes { get; set; } = 5 * 1024 * 1024L;

        public static int ClearCacheFilesCount { get; set; } = 50;
    }
}