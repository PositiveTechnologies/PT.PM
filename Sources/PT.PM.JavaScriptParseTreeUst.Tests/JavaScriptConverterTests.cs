using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;

namespace PT.PM.JavaScriptParseTreeUst.Tests
{
    [TestFixture]
    public class JavaScriptConverterTests
    {
        [Test]
        public void Convert_JavaScriptSyntaxFiles_WithoutErrors()
        {
            TestUtility.CheckProject(TestUtility.TestsDataPath, JavaScript.Language, Stage.Convert);
        }
    }
}
