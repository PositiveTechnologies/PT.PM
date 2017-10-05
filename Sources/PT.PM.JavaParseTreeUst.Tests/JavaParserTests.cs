using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;

namespace PT.PM.JavaParseTreeUst.Tests
{
    [TestFixture]
    public class JavaParserTests
    {
        [Test]
        public void Parse_JavaSyntaxErrorFile_CatchErrors()
        {
            TestUtility.CheckFile("ParseError.java", Java.Language, Stage.Parse, shouldContainsErrors: true);
        }

        [TestCase("ManyStringsConcat.java")]
        [TestCase("AllInOne.java")]
        [TestCase("AllInOne8.java")]
        public void Parser_JavaFile_WithoutErrors(string fileName)
        {
            TestUtility.CheckFile(fileName, Java.Language, Stage.Parse);
        }
    }
}
