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
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, "javascript", "examples"),
                JavaScript.Language, Stage.Ust);
        }
    }
}
