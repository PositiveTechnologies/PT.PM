using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Console.Tests
{
    public class ProcessExecutionResult
    {
        public int ProcessId { get; set; }

        public List<string> Errors { get; set; }

        public List<string> Output { get; set; }

        public ProcessExecutionResult()
        {
            Errors = new List<string>();
            Output = new List<string>();
        }
    }
}
