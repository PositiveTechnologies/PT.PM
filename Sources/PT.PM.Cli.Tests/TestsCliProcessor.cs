using PT.PM.Cli.Common;
using PT.PM.TestUtils;

namespace PT.PM.Cli.Tests
{
    public class TestsCliProcessor : CliProcessor
    {
        public TestsCliProcessor()
        {
            Logger = new TestLogger();
        }
    }
}
