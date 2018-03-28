using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.IO;
using PT.PM.SqlParseTreeUst;
using PT.PM.JavaParseTreeUst;
using System.Collections.Generic;
using System.Linq;
using PT.PM.CSharpParseTreeUst;

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

            HashSet<Language> notJavaSqlLangs = "!Java|!Sql".ParseLanguages();
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
        public void Detect_SourceCode_CorrectLanguage(string expectedLanguage, string fileName)
        {
            string sourceCode = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, fileName.NormDirSeparator()));
            DetectionResult detectedLanguage = new ParserLanguageDetector().Detect(sourceCode);
            Assert.NotNull(detectedLanguage);
            Assert.AreEqual(expectedLanguage, detectedLanguage.Language.Key);
        }
    }
}
