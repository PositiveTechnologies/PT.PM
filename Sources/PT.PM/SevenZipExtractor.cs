using PT.PM.Common.Exceptions;
using System;
using System.Diagnostics;
using PT.PM.Common;
using System.IO;
using System.Reflection;
using System.Threading;

namespace PT.PM
{
    public class SevenZipExtractor : IArchiveExtractor
    {
        public string SevenZipPath { get; set; } = "";

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public SevenZipExtractor()
        {
            string executingAssemblyPath = Path.Combine(
               Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            SevenZipPath = CommonUtils.IsRunningOnLinux
               ? "7z"
               : Path.Combine(executingAssemblyPath, "7-Zip", "7z.exe");
        }

        public void Extract(string zipPath, string extractPath)
        {
            if (!File.Exists(SevenZipPath))
            {
                throw new Exception($"7z.exe not found at {SevenZipPath}");
            }

            string errorMessage = null;
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = SevenZipPath,
                    Arguments = $@"x ""{zipPath}"" -o""{extractPath}"" -y"
                };
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    errorMessage = $"Error while extracting {extractPath}.";
                }
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                errorMessage = $"Error while extracting {extractPath}: {ex.Message}";
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.Error.WriteLine(errorMessage);
                throw new ReadException(new CodeFile("") { Name = zipPath }, message: errorMessage);
            }
        }
    }
}
