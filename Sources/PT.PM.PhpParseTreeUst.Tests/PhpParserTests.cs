using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.TestUtils;

namespace PT.PM.PhpParseTreeUst.Tests
{
    [TestFixture]
    public class PhpParserTests
    {
        public void Parse_PhpSyntax_WithoutErrors()
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, "php", "examples"),
                Language.Php, Stage.ParseTree, searchPredicate: fileName => !fileName.Contains("Error"));
        }

        [Test]
        public void Parse_NewLine_CorrectLineColumn()
        {
            string fileText = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "newLine -r-n.php"));
            var lineEnds = new [] { "\r", "\n", "\r\n" };

            foreach (var lineEnd in lineEnds)
            {
                var phpParser = new PhpAntlrParser();
                string code = fileText.Replace("\r\n", lineEnd);
                var sourceFile = new TextFile(code)
                {
                    Name = "newLine.php"
                };
                var tokens = new PhpAntlrLexer().GetTokens(sourceFile, out TimeSpan _);
                var parseTree = (PhpAntlrParseTree)phpParser.Parse(tokens, out TimeSpan _);
                var converter = new PhpAntlrParseTreeConverter();
                RootUst ust = converter.Convert(parseTree);

                Ust intNode = ust.WhereDescendantsOrSelf(
                    node => node is IntLiteral intLiteral && intLiteral.Value == 42).First();

                LineColumnTextSpan intNodeSpan = intNode.LineColumnTextSpan;
                Assert.AreEqual(1, intNodeSpan.BeginLine);
                Assert.AreEqual(12, intNodeSpan.BeginColumn);
                Assert.AreEqual(14, intNodeSpan.EndColumn);

                Ust heredocNode = ust.WhereDescendantsOrSelf(
                    node => node is StringLiteral stringLiteral &&
                    stringLiteral.TextValue.StartsWith("Heredoc text")).First();

                LineColumnTextSpan heredocNodeSpan = heredocNode.LineColumnTextSpan;
                Assert.AreEqual(3, heredocNodeSpan.BeginLine);
                Assert.AreEqual(6, heredocNodeSpan.EndLine);

                Ust serverAddressNode = ust.WhereDescendantsOrSelf(
                    node => node is StringLiteral stringLiteral &&
                    stringLiteral.TextValue.Contains("http://127.0.0.1")).First();

                LineColumnTextSpan serverAddressNodeSpan = serverAddressNode.LineColumnTextSpan;
                Assert.AreEqual(8, serverAddressNodeSpan.BeginLine);
                Assert.AreEqual(16, serverAddressNodeSpan.BeginColumn);
            }
        }
    }
}
