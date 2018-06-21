using System.Diagnostics;

namespace PT.PM
{
    public class ProcessUtils
    {
        public static bool IsToolExists(string fileName, string arguments = "")
        {
            try
            {
                ProcessExecutionResult result = SetupHiddenProcessAndStart(fileName, arguments, timeout: 1000);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static ProcessExecutionResult SetupHiddenProcessAndStart(string fileName, string arguments, string workingDirectory = ".", bool waitForExit = true,
            int timeout = 0)
        {
            var result = new ProcessExecutionResult();

            var process = new Process();
            var startInfo = process.StartInfo;
            startInfo.FileName = fileName;
            startInfo.Arguments = arguments;
            if (workingDirectory != null)
            {
                startInfo.WorkingDirectory = workingDirectory;
            }
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    result.Errors.Add(e.Data);
                }
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    result.Output.Add(e.Data);
                }
            };
            process.EnableRaisingEvents = true;
            process.Start();
            process.StandardInput.WriteLine();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            result.ProcessId = process.Id;
            if (waitForExit)
            {
                if (timeout == 0)
                {
                    process.WaitForExit();
                }
                else
                {
                    process.WaitForExit(timeout);
                }
            }

            return result;
        }
    }
}
