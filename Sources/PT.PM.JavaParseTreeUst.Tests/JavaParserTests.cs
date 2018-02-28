using NUnit.Framework;
using PT.PM.TestUtils;
using System.IO;

namespace PT.PM.JavaParseTreeUst.Tests
{
    [TestFixture]
    public class JavaParserTests
    {
        [Test]
        public void Parse_JavaSyntaxErrorFile_CatchErrors()
        {
            TestUtility.CheckFile("ParseError.java", Stage.ParseTree, shouldContainsErrors: true);
        }

        [TestCase("ManyStringsConcat.java")]
        [TestCase("AllInOne7.java")]
        [TestCase("AllInOne8.java")]
        public void Parser_JavaFile_WithoutErrors(string fileName)
        {
            TestUtility.CheckFile(Path.Combine(TestUtility.GrammarsDirectory, "java", "examples", fileName), Stage.ParseTree);
        }
    }
}
