using PT.PM.TestUtils;
using AspxParser;
using NUnit.Framework;
using System.IO;
using System.Linq;
using PT.PM.Common.Files;

namespace PT.PM.CSharpParseTreeUst.Tests
{
    [TestFixture]
    public class AspxTests
    {
        [Test]
        public void Convert_AspxLineColumnPosition_Correct()
        {
            string fileName = Path.Combine(TestUtility.TestsDataPath, "TestAspxParser.aspx");
            string text = File.ReadAllText(fileName);
            var aspxParser = new global::AspxParser.AspxParser(fileName, true);
            var source = new AspxSource(fileName, text);
            AspxParseResult result = aspxParser.Parse(source);
            var foundNode = result.RootNode.Descendants<AspxNode.AspxExpressionTag>()
                .FirstOrDefault(node => node.Expression.Contains("Expression text"));

            var sourceCode = new TextFile(source.Text);
            sourceCode.GetLineColumnFromLinear(foundNode.Location.Start, out int line, out int column);
            Assert.AreEqual(15, line);
            Assert.AreEqual(13, column);
            Assert.AreEqual(foundNode.Location.Start, sourceCode.GetLinearFromLineColumn(line, column));

            sourceCode.GetLineColumnFromLinear(foundNode.Location.End, out line, out column);
            Assert.AreEqual(15, line);
            Assert.AreEqual(30, column);
            Assert.AreEqual(foundNode.Location.End, sourceCode.GetLinearFromLineColumn(line, column));
        }

        [TestCase("TestAspxParser.aspx")]
        [TestCase("Patterns.aspx")]
        public void Parse_AspxFile_WithoutErrors(string fileName)
        {
            TestUtility.CheckFile(fileName, Stage.Ust);
        }
    }
}
