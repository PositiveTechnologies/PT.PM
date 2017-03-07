using PT.PM.Common;
using PT.PM.Common.Tests;
using NUnit.Framework;

namespace PT.PM.JavaScriptAstConversion.Tests
{
    [TestFixture]
    public class JavaScriptConverterTests
    {
        [TestCase("helloworld.js")]
        [TestCase("VeryBig.js")]
        public void Convert_JavaScriptSyntax_WithoutErrors(string fileName)
        {
            TestHelper.CheckFile(fileName, Language.JavaScript, Stage.Convert);
        }
    }
}
