using System.IO;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;

namespace PT.PM.PhpParseTreeUst.Tests
{
    [TestFixture]
    public class PhpConverterTests
    {
        [Test]
        public void Convert_PhpFiles_WithoutErrors()
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, "php", "examples"),
                Language.Php, Stage.Ust, searchPredicate: fileName => !fileName.Contains("Error"));
        }

        [Test]
        public void Convert_PhpPatternsWithErrors_MatchedResultsEqual()
        {
            var patternsLogger = new TestLogger();
            TestUtility.CheckFile("Patterns.php", Stage.Match, patternsLogger);

            var patternWithErrorsLogger = new TestLogger();
            TestUtility.CheckFile("PatternsWithParseErrors.php", Stage.Match, patternWithErrorsLogger, true);

            Assert.AreEqual(patternsLogger.InfoMessageCount, patternWithErrorsLogger.InfoMessageCount);
        }
    }
}
