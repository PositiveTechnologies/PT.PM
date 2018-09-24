using NUnit.Framework;
using PT.PM.TestUtils;
using System.IO;

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
                JavaScript.Language, Stage.Ust);
        }
    }
}
