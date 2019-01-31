using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM
{
    public class SevenZipExtractor : IArchiveExtractor
    {
        private const string ToolName = "7z";

        private static bool? is7zInstalled = null;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public static bool Is7zInstalled
        {
            get
            {
                if (!is7zInstalled.HasValue)
                {
                    try
                    {
                        new Processor(ToolName, "", 1000).Start();
                        is7zInstalled = true;
                    }
                    catch
                    {
                        is7zInstalled = false;
                    }
                }

                return is7zInstalled.Value;
            }
        }

        public void Extract(string zipPath, string extractPath)
        {
            if (!Is7zInstalled)
            {
                throw new Exception($"{ToolName} tool is not installed");
            }

            string errorMessage = null;
            try
            {
                var processor = new Processor(ToolName, $@"x ""{zipPath}"" -o""{extractPath}"" -y");
                processor.ErrorDataReceived += (sender, error) =>
                {
                    Logger.LogInfo($"{ToolName}: error: {error}");
                };
                processor.OutputDataReceived += (sender, message) =>
                {
                    Logger.LogInfo($"{ToolName}: {message}");
                };
                ProcessExecutionResult execResult = processor.Start();

                if (execResult.ExitCode != 0)
                {
                    errorMessage = $"{ToolName} error while extracting {extractPath}. Exit code: {execResult.ExitCode}";
                }
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                errorMessage = $"{ToolName} error while extracting {extractPath}: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new ReadException(new TextFile("") { Name = zipPath }, message: errorMessage);
            }
        }
    }
}
