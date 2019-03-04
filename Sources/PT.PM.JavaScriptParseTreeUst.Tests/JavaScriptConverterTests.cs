using System.IO;
using System.Linq;
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
            var ignoredFiles = new []
            {
                "EnhancedRegularExpression.js",
                "MapSetAndWeakMapWeakSet.js",
                "Modules.js",
                "Outdated.js"
            };

            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, "javascript", "examples"),
                Language.JavaScript, Stage.Ust, searchPredicate: fileName =>
                    ignoredFiles.All(ignoredFile => !fileName.Contains(ignoredFile)));
        }
    }
}
