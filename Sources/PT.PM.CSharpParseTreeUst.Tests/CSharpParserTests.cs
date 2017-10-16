using System.Linq;
using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;

namespace PT.PM.CSharpParseTreeUst.Tests
{
    [TestFixture]
    public class CSharpParserTests
    {
        [TestCase("AllInOne.cs")]
        public void Parse_CSharpWithRoslyn(string fileName)
        {
            TestUtility.CheckFile(fileName, Stage.ParseTree);
        }

        [Test]
        public void Parse_SyntaxErrorFileCSharp_CatchErrors()
        {
            var logger = new LoggerMessageCounter();
            TestUtility.CheckFile("ParseError.cs", Stage.ParseTree, logger, true);

            Assert.AreEqual(7, logger.ErrorCount);
        }
    }
}
