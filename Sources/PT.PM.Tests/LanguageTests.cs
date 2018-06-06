using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.IO;
using PT.PM.SqlParseTreeUst;
using PT.PM.JavaParseTreeUst;
using System.Collections.Generic;
using System.Linq;
using PT.PM.CSharpParseTreeUst;
using PT.PM.Common.CodeRepository;

namespace PT.PM.Tests
{
    [TestFixture]
    public class LanguageTests
    {
        [Test]
        public void Parse_String_CorrectLanguages()
        {
            var sqlLanguages = new Language[] { TSql.Language, PlSql.Language };
            CollectionAssert.AreEquivalent(sqlLanguages, "TSQL plsql".ParseLanguages());

            CollectionAssert.AreEquivalent(sqlLanguages, "Sql".ParseLanguages());

            HashSet<Language> notJavaLangs = "~Java".ParseLanguages();
            CollectionAssert.IsSupersetOf(LanguageUtils.Languages.Values, notJavaLangs);
            CollectionAssert.DoesNotContain(notJavaLangs, Java.Language);

            HashSet<Language> notJavaSqlLangs = "!Java,!Sql".ParseLanguages();
            CollectionAssert.IsSupersetOf(LanguageUtils.Languages.Values, notJavaSqlLangs);
            CollectionAssert.DoesNotContain(notJavaSqlLangs, Java.Language);
            CollectionAssert.DoesNotContain(notJavaSqlLangs, TSql.Language);
            CollectionAssert.DoesNotContain(notJavaSqlLangs, PlSql.Language);

            HashSet<Language> cSharpLang = "c#".ParseLanguages();
            Assert.AreEqual(CSharp.Language, cSharpLang.First());
        }

        [TestCase("CSharp", "Patterns.cs")]
        [TestCase("Java", "Patterns.java")]
        [TestCase("Php", "Patterns.php")]
        [TestCase("PlSql", "plsql_patterns.sql")]
        [TestCase("TSql", "tsql_patterns.sql")]
        [TestCase("Aspx", "Patterns.aspx")]
        [TestCase("JavaScript", "Patterns.js")]
        public void DetectLanguage_SourceCode_CorrectLanguage(string expectedLanguage, string fileName)
        {
            var sourceCode =
                new CodeFile(File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, fileName.NormDirSeparator())));
            DetectionResult detectedLanguage = new ParserLanguageDetector().Detect(sourceCode);
            Assert.NotNull(detectedLanguage);
            Assert.AreEqual(expectedLanguage, detectedLanguage.Language.Key);
        }

        [Test]
        public void IgnoreFileDueToAnalyzedLanguages()
        {
            var sourceCodeRepository = new MemoryCodeRepository("");
            sourceCodeRepository.Languages = new HashSet<Language> { PlSql.Language, TSql.Language };
            Assert.IsTrue(sourceCodeRepository.IsFileIgnored(Path.Combine(TestUtility.TestsDataPath, "Patterns.php")));
        }

        [Test]
        public void DetectLanguageIfRequired_SourceCode_CorrectLanguage()
        {
            var languageDetector = new ParserLanguageDetector();
            DetectionResult detectionResult;

            var plSqlFile = new CodeFile(File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "plsql_patterns.sql")));
            detectionResult = languageDetector.DetectIfRequired(plSqlFile, new HashSet<Language> { PlSql.Language, TSql.Language });

            Assert.NotNull(detectionResult.ParseTree);
            Assert.AreEqual(PlSql.Language, detectionResult.Language);

            // Force parse file with specified language.
            detectionResult = languageDetector.DetectIfRequired(plSqlFile, new HashSet<Language> { TSql.Language });
            Assert.AreEqual(TSql.Language, detectionResult.Language);
        }
    }
}
