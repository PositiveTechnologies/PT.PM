using System;
using System.Diagnostics;
using System.Text;

namespace PT.PM
{
    public class Processor
    {
        public string ToolName { get; }

        public string Arguments { get; set; } = "";

        public string WorkingDirectory { get; set; }

        public bool WaitForExit { get; set; } = true;

        public int Timeout { get; set; } = 0;

        public Encoding Encoding { get; set; }

        public event EventHandler<string> ErrorDataReceived;

        public event EventHandler<string> OutputDataReceived;

        public Processor(string toolName, string arguments = "", int timeout = 0, Encoding encoding = null)
        {
            ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            Encoding = encoding;
            Timeout = timeout;
        }

        public ProcessExecutionResult Start()
        {
            var result = new ProcessExecutionResult();
            var process = new Process();

            var startInfo = process.StartInfo;
            startInfo.FileName = ToolName;
            startInfo.Arguments = Arguments;

            startInfo.StandardOutputEncoding = Encoding;
            startInfo.StandardErrorEncoding = Encoding;

            if (WorkingDirectory != null)
            {
                startInfo.WorkingDirectory = WorkingDirectory;
            }
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            process.ErrorDataReceived += (sender, e) =>
            {
                EventHandler<string> errorDataReceived = ErrorDataReceived;
                if (!string.IsNullOrEmpty(e.Data) && errorDataReceived != null)
                {
                    errorDataReceived(sender, e.Data);
                }
            };
            process.OutputDataReceived += (sender, e) =>
            {
                EventHandler<string> outputDataReceived = OutputDataReceived;
                if (!string.IsNullOrEmpty(e.Data) && outputDataReceived != null)
                {
                    outputDataReceived(sender, e.Data);
                }
            };

            process.EnableRaisingEvents = true;
            process.Start();
            process.StandardInput.WriteLine();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            result.ProcessId = process.Id;
            if (WaitForExit)
            {
                if (Timeout == 0)
                {
                    process.WaitForExit();
                }
                else
                {
                    process.WaitForExit(Timeout);
                }
            }
            result.ExitCode = process.ExitCode;

            return result;
        }
    }
}
