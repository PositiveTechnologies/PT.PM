using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
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
            Utils.RegisterAllLexersParsersAndConverters();
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

            HashSet<Language> allLang = "all".ParseLanguages();
            CollectionAssert.AreEquivalent(LanguageUtils.Languages, allLang);
        }

        [TestCase(Language.CSharp, "Patterns.cs")]
        [TestCase(Language.Java, "Patterns.java")]
        [TestCase(Language.Php, "Patterns.php")]
        [TestCase(Language.PlSql, "plsql_patterns.sql")]
        [TestCase(Language.TSql, "tsql_patterns.sql")]
        [TestCase(Language.MySql, "mysql_patterns.sql")]
        [TestCase(Language.Aspx, "Patterns.aspx")]
        [TestCase(Language.JavaScript, "Patterns.js")]
        [TestCase(Language.Python, "python_patterns.py")]
        public void DetectLanguage_Source_CorrectLanguage(Language expectedLanguage, string fileName)
        {
            var source =
                new TextFile(File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, fileName.NormalizeDirSeparator())));
            source.Name = fileName;
            DetectionResult detectedLanguage = ParserLanguageDetector.Detect(source);
            Assert.AreEqual(expectedLanguage, detectedLanguage.Language);
        }

        [Test]
        public void IgnoreFileDueToAnalyzedLanguages()
        {
            var sourceRepository = new MemorySourceRepository("");
            sourceRepository.Languages = new HashSet<Language> { Language.PlSql, Language.TSql };

            CollectionAssert.IsEmpty(sourceRepository.GetLanguages(Path.Combine(TestUtility.TestsDataPath, "Patterns.php"), true));
        }

        [TestCase(Language.MySql)]
        [TestCase(Language.TSql)]
        [TestCase(Language.PlSql)]
        public void DetectSqlDialects(Language language)
        {
            var testFolder = Path.Combine(TestUtility.GrammarsDirectory, language.ToString().ToLowerInvariant(),
                "examples");
            var directoryCodeRepository = new DirectorySourceRepository(testFolder);
            var fileNames = directoryCodeRepository.GetFileNames();

            int totalFilesCount = 0;
            int ambiguousFilesCount = 0;

            foreach (var fileName in fileNames)
            {
                TextFile textFile = new TextFile(File.ReadAllText(fileName), fileName);
                List<Language> sqls = SqlDialectDetector.Detect(textFile);

                if (sqls.Count > 1)
                {
                    Console.WriteLine($"{fileName} has been recognized as {string.Join(", ", sqls)}");
                    ambiguousFilesCount++;
                }

                totalFilesCount++;

                CollectionAssert.Contains(sqls, language, $"File {fileName} has not been detected as {language}");
            }

            Console.WriteLine($"Ambiguous files count: {ambiguousFilesCount}");
            Console.WriteLine($"Total files count: {totalFilesCount}");
        }
    }
}

