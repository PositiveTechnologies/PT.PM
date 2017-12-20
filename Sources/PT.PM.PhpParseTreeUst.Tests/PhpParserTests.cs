using PT.PM.Common;
using PT.PM.Common.Nodes;
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
        public void Parse_PhpSyntax_WithoutErrors()
        {
            TestUtility.CheckProject(TestUtility.TestsDataPath, Php.Language, Stage.ParseTree,
               searchPredicate: fileName => !fileName.Contains("Error"));
        }

        [Test]
        public void Parse_NewLine_CorrectLineColumn()
        {
            string fileText = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "newLine -r-n.php"));
            var lineEnds = new string[] { "\r", "\n", "\r\n" };

            foreach (var lineEnd in lineEnds)
            {
                var phpParser = new PhpAntlrParser();
                string code = fileText.Replace("\r\n", lineEnd);
                var sourceCodeFile = new CodeFile(code)
                {
                    Name = "newLine.php",
                };
                var parseTree = (PhpAntlrParseTree)phpParser.Parse(sourceCodeFile);
                var converter = new PhpAntlrParseTreeConverter();
                RootUst ust = converter.Convert(parseTree);

                Ust intNode = ust.WhereDescendants(
                    node => node is IntLiteral intLiteral && intLiteral.Value == 42).First();

                LineColumnTextSpan intNodeSpan = intNode.LineColumnTextSpan;
                Assert.AreEqual(1, intNodeSpan.BeginLine);
                Assert.AreEqual(12, intNodeSpan.BeginColumn);
                Assert.AreEqual(14, intNodeSpan.EndColumn);

                Ust heredocNode = ust.WhereDescendants(
                    node => node is StringLiteral stringLiteral &&
                    stringLiteral.Text.StartsWith("Heredoc text")).First();

                LineColumnTextSpan heredocNodeSpan = heredocNode.LineColumnTextSpan;
                Assert.AreEqual(3, heredocNodeSpan.BeginLine);
                Assert.AreEqual(6, heredocNodeSpan.EndLine);

                Ust serverAddressNode = ust.WhereDescendants(
                    node => node is StringLiteral stringLiteral &&
                    stringLiteral.Text.Contains("http://127.0.0.1")).First();

                LineColumnTextSpan serverAddressNodeSpan = serverAddressNode.LineColumnTextSpan;
                Assert.AreEqual(8, serverAddressNodeSpan.BeginLine);
                Assert.AreEqual(15, serverAddressNodeSpan.BeginColumn);
            }
        }
    }
}
