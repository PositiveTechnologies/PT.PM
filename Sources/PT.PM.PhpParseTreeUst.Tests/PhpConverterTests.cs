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
            TestHelper.CheckFile(fileName, Language.Php, Stage.Convert);
        }

        [TestCase("WebGoatPHP-6f48c9")]
        // [TestCase("phpBB-3.1.6")] // Too long test duration
        // [TestCase("ZendFramework-2.4.8")] // Too long test duration
        public void Convert_PhpProject_WithoutErrors(string projectKey)
        {
            TestHelper.CheckProject(
                TestProjects.PhpProjects.Single(p => p.Key == projectKey), Language.Php, Stage.Convert);
        }

        [Test]
        public void Convert_PhpPatternsWithErrors_MatchedResultsEqual()
        {
            var patternsLogger = new LoggerMessageCounter();
            TestHelper.CheckFile("Patterns.php", Language.Php, Stage.Match, patternsLogger);

            var patternWithErrorsLogger = new LoggerMessageCounter();
            TestHelper.CheckFile("PatternsWithParseErrors.php", Language.Php, Stage.Match, patternWithErrorsLogger, true);

            Assert.AreEqual(patternsLogger.InfoMessageCount, patternWithErrorsLogger.InfoMessageCount);
        }
    }
}
