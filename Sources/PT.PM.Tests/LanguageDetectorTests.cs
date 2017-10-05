using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.IO;

namespace PT.PM.Tests
{
    [TestFixture]
    public class LanguageDetectorTests
    {
        [TestCase("CSharp", "Patterns.cs")]
        [TestCase("Java", "Patterns.java")]
        [TestCase("Php", "Patterns.php")]
        [TestCase("PlSql", "PlSql/plsql_patterns.sql")]
        [TestCase("TSql", "TSql/tsql_patterns.sql")]
        [TestCase("Aspx", "Patterns.aspx")]
        [TestCase("JavaScript", "Patterns.js")]
        public void Detect_SourceCode_CorrectLanguage(string expectedLanguage, string fileName)
        {
            string sourceCode = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, fileName.NormDirSeparator()));
            LanguageInfo detectedLanguage = new ParserLanguageDetector().Detect(sourceCode);
            Assert.NotNull(detectedLanguage);
            Assert.AreEqual(expectedLanguage, detectedLanguage.Key);
        }
    }
}
