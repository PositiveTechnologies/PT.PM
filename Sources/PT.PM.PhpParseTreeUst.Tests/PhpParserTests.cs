using PT.PM.ParseTreeUst;
using PT.PM.UstParsing;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.IO;
using System.Linq;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.PhpParseTreeUst.Tests
{
    [TestFixture]
    public class PhpParserTests
    {
        [TestCase("priorityTest.php")]
        [TestCase("alternativeSyntax.php")]
        [TestCase("heredoc.php")]
        [TestCase("scriptInHtml.php")]
        [TestCase("styleInHtml.php")]
        [TestCase("shebang.php")]
        [TestCase("aspTags.php")]
        [TestCase("deepConcatanation.php")]
        [TestCase("strings.php")]
        public void Parse_PhpSyntax_WithoutErrors(string fileName)
        {
            TestHelper.CheckFile(fileName, Language.Php, Stage.Parse);
        }

        [Test]
        public void Parse_NewLine_CorrectLineColumn()
        {
            string fileText = File.ReadAllText(Path.Combine(TestHelper.TestsDataPath, "newLine -r-n.php"));
            var lineEnds = new string[] { "\r", "\n", "\r\n" };

            foreach (var lineEnd in lineEnds)
            {
                var phpParser = new PhpAntlrParser();
                string code = fileText.Replace("\r\n", lineEnd);
                var sourceCodeFile = new SourceCodeFile
                {
                    Name = "newLine.php",
                    Code = code
                };
                var parseTree = (PhpAntlrParseTree)phpParser.Parse(sourceCodeFile);
                var converter = new PhpAntlrParseTreeConverter();
                Ust ust = converter.Convert(parseTree);
                
                UstNode intNode = ust.Root.GetAllDescendants(
                    node => node.NodeType == NodeType.IntLiteral &&
                    ((IntLiteral)node).Value == 42).First();

                int beginLine, beginColumn, endLine, endColumn;
                TextHelper.ToLineColumn(intNode.TextSpan, code, out beginLine, out beginColumn, out endLine, out endColumn);
                Assert.AreEqual(1, beginLine);
                Assert.AreEqual(12, beginColumn);
                Assert.AreEqual(14, endColumn);

                UstNode heredocNode = ust.Root.GetAllDescendants(
                    node => node.NodeType == NodeType.StringLiteral &&
                    ((StringLiteral)node).Text.StartsWith("Heredoc text")).First();

                TextHelper.ToLineColumn(heredocNode.TextSpan, code, out beginLine, out beginColumn, out endLine, out endColumn);
                Assert.AreEqual(3, beginLine);
                Assert.AreEqual(6, endLine);

                UstNode serverAddressNode = ust.Root.GetAllDescendants(
                    node => node.NodeType == NodeType.StringLiteral &&
                    ((StringLiteral)node).Text.Contains("http://127.0.0.1")).First();

                TextHelper.ToLineColumn(serverAddressNode.TextSpan, code, out beginLine, out beginColumn, out endLine, out endColumn);
                Assert.AreEqual(8, beginLine);
                Assert.AreEqual(15, beginColumn);
            }
        }
    }
}
