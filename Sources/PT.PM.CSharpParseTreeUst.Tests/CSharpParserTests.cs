using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;
using System.IO;

namespace PT.PM.CSharpParseTreeUst.Tests
{
    [TestFixture]
    public class CSharpParserTests
    {
        [Test]
        public void Parse_CSharpWithRoslyn()
        {
            TestUtility.CheckFile(Path.Combine(TestUtility.GrammarsDirectory, "csharp", "not-ready-examples", "AllInOne.cs"),
                Stage.ParseTree);
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
