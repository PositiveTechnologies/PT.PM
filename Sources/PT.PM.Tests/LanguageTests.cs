using NUnit.Framework;
using PT.PM.CLangsParseTreeUst;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Utils;
using PT.PM.CSharpParseTreeUst;
using PT.PM.JavaParseTreeUst;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.PhpParseTreeUst;
using PT.PM.SqlParseTreeUst;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PT.PM.Common.Files;

namespace PT.PM.Tests
{
    [TestFixture]
    public class LanguageTests
    {
        [Test]
        public void CollectLanguages_Assemblies()
        {
            var languages = LanguageUtils.Languages.Values;

            CollectionAssert.Contains(languages, C.Language);
            CollectionAssert.Contains(languages, CPlusPlus.Language);
            CollectionAssert.Contains(languages, ObjectiveC.Language);

            CollectionAssert.Contains(languages, Aspx.Language);
            CollectionAssert.Contains(languages, CSharp.Language);

            CollectionAssert.Contains(languages, Java.Language);

            CollectionAssert.Contains(languages, JavaScript.Language);

            CollectionAssert.Contains(languages, Html.Language);
            CollectionAssert.Contains(languages, Php.Language);

            CollectionAssert.Contains(languages, PlSql.Language);
            CollectionAssert.Contains(languages, TSql.Language);
            CollectionAssert.Contains(languages, MySql.Language);

            CollectionAssert.Contains(languages, Uncertain.Language);
        }

        [Test]
        public void Parse_String_CorrectLanguages()
        {
            var sqlLanguages = new Language[] { TSql.Language, PlSql.Language, MySql.Language };
            CollectionAssert.AreEquivalent(sqlLanguages, "TSQL plsql MySql".ParseLanguages());

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
            
            HashSet<Language> javaScriptLang = "js".ParseLanguages();
            Assert.AreEqual(JavaScript.Language, javaScriptLang.First());
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
                new CodeFile(File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, fileName.NormalizeDirSeparator())));
            DetectionResult detectedLanguage = new ParserLanguageDetector().Detect(sourceCode);
            Assert.NotNull(detectedLanguage);
            Assert.AreEqual(expectedLanguage, detectedLanguage.Language.Key);
        }

        [Test]
        public void IgnoreFileDueToAnalyzedLanguages()
        {
            var sourceCodeRepository = new MemoryCodeRepository("");
            sourceCodeRepository.Languages = new HashSet<Language> { PlSql.Language, TSql.Language };
            Assert.IsTrue(sourceCodeRepository.IsFileIgnored(Path.Combine(TestUtility.TestsDataPath, "Patterns.php"), true));
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

