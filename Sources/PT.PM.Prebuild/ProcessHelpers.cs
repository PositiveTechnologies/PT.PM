using System.Diagnostics;

namespace PT.PM.Prebuild
{
    public class ProcessHelpers
    {
        public static Process SetupHiddenProcessAndStart(string fileName, string arguments, string workingDirectory,
            DataReceivedEventHandler errorDataReceived, DataReceivedEventHandler outputDataReceived)
        {
            var process = new Process();
            var startInfo = process.StartInfo;
            startInfo.FileName = fileName;
            startInfo.Arguments = arguments;
            if (workingDirectory != null)
            {
                startInfo.WorkingDirectory = workingDirectory;
            }
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.ErrorDataReceived += errorDataReceived;
            process.OutputDataReceived += outputDataReceived;
            process.EnableRaisingEvents = true;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process;
        }

        public static bool IsProcessCanBeExecuted(string fileName, string arguments = "")
        {
            try
            {
                SetupHiddenProcessStartAndWait(fileName, arguments);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static Process SetupHiddenProcessStartAndWait(string fileName, string arguments, string workingDirectory = null)
        {
            var process = new Process();
            var startInfo = process.StartInfo;
            startInfo.FileName = fileName;
            startInfo.Arguments = arguments;
            if (workingDirectory != null)
            {
                startInfo.WorkingDirectory = workingDirectory;
            }
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            return process;
        }
    }
}
