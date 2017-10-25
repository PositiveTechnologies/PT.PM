using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.IO;
using PT.PM.SqlParseTreeUst;
using PT.PM.JavaParseTreeUst;
using System.Collections.Generic;

namespace PT.PM.Tests
{
    [TestFixture]
    public class LanguageTests
    {
        [Test]
        public void Parse_String_CorrectLanguages()
        {
            var sqlLanguages = new Language[] { TSql.Language, PlSql.Language };
            CollectionAssert.AreEquivalent(sqlLanguages, "TSQL plsql".ToLanguages());

            CollectionAssert.AreEquivalent(sqlLanguages, "Sql".ToLanguages());

            HashSet<Language> notJavaLangs = "~Java".ToLanguages();
            CollectionAssert.IsSupersetOf(LanguageUtils.Languages.Values, notJavaLangs);
            CollectionAssert.DoesNotContain(notJavaLangs, Java.Language);

            HashSet<Language> notJavaSqlLangs = "!Java|!Sql".ToLanguages();
            CollectionAssert.IsSupersetOf(LanguageUtils.Languages.Values, notJavaSqlLangs);
            CollectionAssert.DoesNotContain(notJavaSqlLangs, Java.Language);
            CollectionAssert.DoesNotContain(notJavaSqlLangs, TSql.Language);
            CollectionAssert.DoesNotContain(notJavaSqlLangs, PlSql.Language);
        }

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
            Language detectedLanguage = new ParserLanguageDetector().Detect(sourceCode);
            Assert.NotNull(detectedLanguage);
            Assert.AreEqual(expectedLanguage, detectedLanguage.Key);
        }
    }
}
