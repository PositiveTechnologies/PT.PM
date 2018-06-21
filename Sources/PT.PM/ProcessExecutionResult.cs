using System.Collections.Generic;

namespace PT.PM
{
    public class ProcessExecutionResult
    {
        public int ProcessId { get; set; }

        public int ExitCode { get; set; }

        public List<string> Errors { get; set; }

        public List<string> Output { get; set; }

        public ProcessExecutionResult()
        {
            Errors = new List<string>();
            Output = new List<string>();
        }
    }
}
