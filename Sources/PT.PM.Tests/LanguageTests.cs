using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Files;
using PT.PM.Common.Utils;
using PT.PM.TestUtils;

namespace PT.PM.Tests
{
    [TestFixture]
    public class LanguageTests
    {
        [SetUp]
        public void Init()
        {
            Utils.RegisterAllParsersAndCovnerters();
        }

        [Test]
        public void AllLanguageEnumsHaveCorrespondingInfos()
        {
            var enumValues = (Language[])Enum.GetValues(typeof(Language));

            foreach (Language enumValue in enumValues)
            {
                Assert.IsTrue(LanguageUtils.LanguageInfos.ContainsKey(enumValue));
            }
        }

        [Test]
        public void Parse_String_CorrectLanguages()
        {
            var sqlLanguages = new Language[] { Language.TSql, Language.PlSql, Language.MySql };
            CollectionAssert.AreEquivalent(sqlLanguages, "TSQL plsql MySql".ParseLanguages());

            CollectionAssert.AreEquivalent(sqlLanguages, "Sql".ParseLanguages());

            HashSet<Language> notJavaLangs = "~Java".ParseLanguages();
            CollectionAssert.IsSupersetOf(LanguageUtils.Languages, notJavaLangs);
            CollectionAssert.DoesNotContain(notJavaLangs, Language.Java);

            HashSet<Language> notJavaSqlLangs = "!Java,!Sql".ParseLanguages();
            CollectionAssert.IsSupersetOf(LanguageUtils.Languages, notJavaSqlLangs);
            CollectionAssert.DoesNotContain(notJavaSqlLangs, Language.Java);
            CollectionAssert.DoesNotContain(notJavaSqlLangs, Language.TSql);
            CollectionAssert.DoesNotContain(notJavaSqlLangs, Language.PlSql);

            HashSet<Language> cSharpLang = "c#".ParseLanguages();
            Assert.AreEqual(Language.CSharp, cSharpLang.First());
            
            HashSet<Language> javaScriptLang = "js".ParseLanguages();
            Assert.AreEqual(Language.JavaScript, javaScriptLang.First());
        }

        [TestCase(Language.CSharp, "Patterns.cs")]
        [TestCase(Language.Java, "Patterns.java")]
        [TestCase(Language.Php, "Patterns.php")]
        [TestCase(Language.PlSql, "plsql_patterns.sql")]
        [TestCase(Language.TSql, "tsql_patterns.sql")]
        [TestCase(Language.Aspx, "Patterns.aspx")]
        [TestCase(Language.JavaScript, "Patterns.js")]
        public void DetectLanguage_SourceCode_CorrectLanguage(Language expectedLanguage, string fileName)
        {
            var sourceCode =
                new TextFile(File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, fileName.NormalizeDirSeparator())));
            DetectionResult detectedLanguage = new ParserLanguageDetector().Detect(sourceCode);
            Assert.AreEqual(expectedLanguage, detectedLanguage.Language);
        }

        [Test]
        public void IgnoreFileDueToAnalyzedLanguages()
        {
            var sourceCodeRepository = new MemoryCodeRepository("");
            sourceCodeRepository.Languages = new HashSet<Language> { Language.PlSql, Language.TSql };
            Assert.IsTrue(sourceCodeRepository.IsFileIgnored(Path.Combine(TestUtility.TestsDataPath, "Patterns.php"), true));
        }

        [Test]
        public void DetectLanguageIfRequired_SourceCode_CorrectLanguage()
        {
            var languageDetector = new ParserLanguageDetector();
            DetectionResult detectionResult;

            var plSqlFile = new TextFile(File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "plsql_patterns.sql")));
            detectionResult = languageDetector.DetectIfRequired(plSqlFile, new HashSet<Language> { Language.PlSql, Language.TSql });

            Assert.NotNull(detectionResult.ParseTree);
            Assert.AreEqual(Language.PlSql, detectionResult.Language);

            // Force parse file with specified language.
            detectionResult = languageDetector.DetectIfRequired(plSqlFile, new HashSet<Language> { Language.TSql });
            Assert.AreEqual(Language.TSql, detectionResult.Language);
        }
    }
}

