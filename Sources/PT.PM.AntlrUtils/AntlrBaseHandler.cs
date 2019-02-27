using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Antlr4.Runtime.Atn;
using PT.PM.Common;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrBaseHandler
    {
        private long processedFilesCount;
        private long processedBytesCount;
        private long checkNumber;
        private volatile bool excessMemory;

        public AntlrMemoryErrorListener ErrorListener { get; set; }

        public static ILogger StaticLogger { get; set; } = DummyLogger.Instance;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public TextFile SourceFile { get; set; }

        public abstract Language Language { get; }

        public bool UseFastParseStrategyAtFirst { get; set; } = true;

        public static long MemoryConsumptionBytes { get; set; } = 3 * 1024 * 1024 * 1024L;

        public static long ClearCacheFilesBytes { get; set; } = 5 * 1024 * 1024L;

        public static int ClearCacheFilesCount { get; set; } = 50;

        protected void HandleMemoryConsumption(Dictionary<Language, ATN> atns)
        {
            long localProcessedFilesCount = Interlocked.Increment(ref processedFilesCount);
            long localProcessedBytesCount = Interlocked.Add(ref processedBytesCount, SourceFile.Data.Length);

            long divideResult = localProcessedBytesCount / ClearCacheFilesBytes;
            bool exceededProcessedBytes = divideResult > Thread.VolatileRead(ref checkNumber);
            checkNumber = divideResult;

            if (Process.GetCurrentProcess().PrivateMemorySize64 > MemoryConsumptionBytes)
            {
                bool prevExcessMemory = excessMemory;
                excessMemory = true;

                if (!prevExcessMemory ||
                    exceededProcessedBytes ||
                    localProcessedFilesCount % ClearCacheFilesCount == 0)
                {
                    lock (atns)
                    {
                        atns.Remove(Language);
                    }

                    Logger.LogInfo(
                        $"Memory cleared due to big memory consumption during {SourceFile.RelativeName} parsing.");
                }
            }
            else
            {
                excessMemory = false;
            }
        }
    }
}