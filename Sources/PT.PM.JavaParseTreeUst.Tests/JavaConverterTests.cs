using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.Linq;

namespace PT.PM.JavaParseTreeUst.Tests
{
    [TestFixture]
    public class JavaConverterTests
    {
        [TestCase("ManyStringsConcat.java")]
        [TestCase("AllInOne.java")]
        public void Convert_Java_WithoutErrors(string fileName)
        {
            TestHelper.CheckFile(fileName, Language.Java, Stage.Convert);
        }

        [Test]
        public void Convert_WebGoatJava_WithoutErrors()
        {
            string projectKey = "WebGoat.Java-05a1f5";
            TestHelper.CheckProject(TestProjects.JavaProjects.Single(p => p.Key == projectKey),
                Language.Java, Stage.Parse);
        }

        [Test]
        public void Convert_JavaPatternsWithErrors_MatchedResultsEqual()
        {
            var patternsLogger = new LoggerMessageCounter();
            TestHelper.CheckFile("Patterns.java", Language.Java, Stage.Match, patternsLogger);

            var patternWithErrorsLogger = new LoggerMessageCounter();
            TestHelper.CheckFile("PatternsWithParseErrors.java", Language.Java, Stage.Match, patternWithErrorsLogger, true);

            Assert.AreEqual(patternsLogger.InfoMessageCount, patternWithErrorsLogger.InfoMessageCount);
        }
    }
}
