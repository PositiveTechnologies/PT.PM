using PT.PM.Cli.Common;
using PT.PM.TestUtils;

namespace PT.PM.Cli.Tests
{
    public class TestsCliProcessor : CliProcessor
    {
        public override bool StopIfDebuggerAttached => false;

        public TestsCliProcessor()
        {
            Logger = new TestLogger();
        }
    }
}
