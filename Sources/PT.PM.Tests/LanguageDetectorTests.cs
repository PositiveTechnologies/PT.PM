using PT.PM.Common;
using PT.PM.Common.Tests;
using NUnit.Framework;
using System.IO;

namespace PT.PM.Tests
{
    [TestFixture]
    public class LanguageDetectorTests
    {
        [TestCase(Language.CSharp, "Patterns.cs")]
        [TestCase(Language.Java, "Patterns.java")]
        [TestCase(Language.Php, "Patterns.php")]
        [TestCase(Language.PlSql, "PlSql/plsql_patterns.sql")]
        [TestCase(Language.TSql, "TSql/tsql_patterns.sql")]
        [TestCase(Language.Aspx, "Patterns.aspx")]
        [TestCase(Language.JavaScript, "Patterns.js")]
        public void Detect_SourceCode_CorrectLanguage(Language expectedLanguage, string fileName)
        {
            string sourceCode = File.ReadAllText(Path.Combine(TestHelper.TestsDataPath, fileName.NormDirSeparator()));
            Language? detectedLanguage = new ParserLanguageDetector().Detect(sourceCode);
            Assert.NotNull(detectedLanguage);
            Assert.AreEqual(expectedLanguage, detectedLanguage);
        }
    }
}
