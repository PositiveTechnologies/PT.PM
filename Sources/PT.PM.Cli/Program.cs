using PT.PM.Cli.Common;

namespace PT.PM.Cli
{
    class Program
    {
        static int Main(string[] args) => new CliProcessor().Process(args) != null ? 0 : 1;
    }
}
