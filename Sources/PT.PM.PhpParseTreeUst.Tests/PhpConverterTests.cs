using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.Linq;

namespace PT.PM.PhpParseTreeUst.Tests
{
    [TestFixture]
    public class PhpConverterTests
    {
        [TestCase("numericScale.php")]
        public void Convert_PhpSyntax_WithoutErrors(string fileName)
        {
            TestUtility.CheckFile(fileName, Stage.Ust);
        }

        [TestCase("WebGoatPHP-6f48c9")]
        // [TestCase("phpBB-3.1.6")] // Too long test duration
        // [TestCase("ZendFramework-2.4.8")] // Too long test duration
        public void Convert_PhpProject_WithoutErrors(string projectKey)
        {
            TestUtility.CheckProject(
                TestProjects.PhpProjects.Single(p => p.Key == projectKey), Php.Language, Stage.Ust);
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
