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
            TestUtility.CheckFile(fileName, CSharp.Language, Stage.Parse);
        }

        [Test]
        public void Parse_SyntaxErrorFileCSharp_CatchErrors()
        {
            var logger = new LoggerMessageCounter();
            TestUtility.CheckFile("ParseError.cs", CSharp.Language, Stage.Parse, logger, true);

            Assert.AreEqual(7, logger.ErrorCount);
        }
    }
}
