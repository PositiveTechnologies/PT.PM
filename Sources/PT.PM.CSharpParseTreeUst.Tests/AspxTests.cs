using PT.PM.Common;
using PT.PM.TestUtils;
using AspxParser;
using NUnit.Framework;
using System.IO;
using System.Linq;

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

            foundNode.Location.Start.ToLineColumn(source.Text, out int line, out int column);
            Assert.AreEqual(15, line);
            Assert.AreEqual(13, column);
            Assert.AreEqual(foundNode.Location.Start, TextUtils.LineColumnToLinear(source.Text, line, column));

            foundNode.Location.End.ToLineColumn(source.Text, out line, out column);
            Assert.AreEqual(15, line);
            Assert.AreEqual(30, column);
            Assert.AreEqual(foundNode.Location.End, TextUtils.LineColumnToLinear(source.Text, line, column));
        }

        [TestCase("TestAspxParser.aspx")]
        [TestCase("Patterns.aspx")]
        public void Parse_AspxFile_WithoutErrors(string fileName)
        {
            TestUtility.CheckFile(fileName, Stage.Ust);
        }
    }
}
