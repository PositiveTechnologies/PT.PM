using PT.PM.Common;
using System;
using System.Diagnostics;

namespace PT.PM.Cli.Common
{
    public static class LoggerUtils
    {
        public const int Align = -25;

        private const int TwoInPower20 = 1 << 20;

        public static void LogSystemInfo(ILogger logger, string coreName)
        {
            logger.LogInfo($"{coreName + " version:",Align} {Utils.GetVersionString()}");
            logger.LogInfo($"{"OS:",Align} {Environment.OSVersion}");
            logger.LogInfo($"{"Config:",Align} {(CommonUtils.IsDebug ? "DEBUG" : "RELEASE")} ({(Debugger.IsAttached ? "+ debugger" : "no debugger")})");

            Process currentProcess = Process.GetCurrentProcess();
            string processBitsString = (Environment.Is64BitProcess ? "64" : "32") + "-bit";
            double peakVirtualSet = currentProcess.PeakVirtualMemorySize64 / TwoInPower20;
            double peakWorkingSet = currentProcess.PeakWorkingSet64 / TwoInPower20;
            logger.LogInfo($"{"Peak virtual/working set:",Align} {peakVirtualSet} / {peakWorkingSet} MB, {processBitsString}");
        }
    }
}
