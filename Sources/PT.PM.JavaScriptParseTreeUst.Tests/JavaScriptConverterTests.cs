using System.IO;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;

namespace PT.PM.JavaScriptParseTreeUst.Tests
{
    [TestFixture]
    public class JavaScriptConverterTests
    {
        [Test]
        public void Convert_JavaScriptSyntaxFiles_WithoutErrors()
        {
            Assert.Inconclusive("Waiting for the new version of https://github.com/sebastienros/esprima-dotnet");

            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, "javascript", "examples"),
                Language.JavaScript, Stage.Ust);
        }
    }
}
