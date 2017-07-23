using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.Linq;

namespace PT.PM.JavaParseTreeUst.Tests
{
    [TestFixture]
    public class JavaParserTests
    {
        [Test]
        public void Parse_JavaSyntaxErrorFile_CatchErrors()
        {
            TestHelper.CheckFile("ParseError.java", Language.Java, Stage.Parse, shouldContainsErrors:true);
        }

        [TestCase("WebGoat.Java-05a1f5")]
        public void Parse_JavaProject_WithoutErrors(string projectKey)
        {
            TestHelper.CheckProject(TestProjects.JavaProjects
                .Single(p => p.Key == projectKey), Language.Java, Stage.Parse);
        }

        [TestCase("ManyStringsConcat.java")]
        [TestCase("AllInOne.java")]
        [TestCase("AllInOne8.java")]
        public void Parser_JavaFile_WithoutErrors(string fileName)
        {
            TestHelper.CheckFile(fileName, Language.Java, Stage.Parse);
        }
    }
}
