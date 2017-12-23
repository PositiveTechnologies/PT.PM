using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;
using System.IO;

namespace PT.PM.PhpParseTreeUst.Tests
{
    [TestFixture]
    public class PhpConverterTests
    {
        [Test]
        public void Convert_PhpFiles_WithoutErrors()
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, "php", "examples"),
                Php.Language, Stage.Ust, searchPredicate: fileName => !fileName.Contains("Error"));
        }

        [Test]
        public void Convert_PhpPatternsWithErrors_MatchedResultsEqual()
        {
            var patternsLogger = new LoggerMessageCounter();
            TestUtility.CheckFile("Patterns.php", Stage.Match, patternsLogger);

            var patternWithErrorsLogger = new LoggerMessageCounter();
            TestUtility.CheckFile("PatternsWithParseErrors.php", Stage.Match, patternWithErrorsLogger, true);

            Assert.AreEqual(patternsLogger.InfoMessageCount, patternWithErrorsLogger.InfoMessageCount);
        }
    }
}
