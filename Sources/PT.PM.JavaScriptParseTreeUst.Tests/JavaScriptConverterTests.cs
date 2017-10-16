using NUnit.Framework;
using PT.PM.TestUtils;

namespace PT.PM.JavaScriptParseTreeUst.Tests
{
    [TestFixture]
    public class JavaScriptConverterTests
    {
        [Test]
        public void Convert_JavaScriptSyntaxFiles_WithoutErrors()
        {
            TestUtility.CheckProject(TestUtility.TestsDataPath, JavaScript.Language, Stage.Ust, "*.js");
        }
    }
}
