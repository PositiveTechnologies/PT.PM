using System;
using System.Diagnostics;

namespace PT.PM.Cli
{
    class Program
    {
        private static int Main(string[] args)
        {
            var cliProcessor = new CliProcessor();
            int result = cliProcessor.ParseAndConvert(args, "PT.PM");

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press Enter to exit");
                Console.ReadLine();
            }

            return result;
        }
    }
}
