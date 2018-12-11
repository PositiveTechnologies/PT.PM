using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;
using System.IO;
using System.Linq;

namespace PT.PM.SqlParseTreeUst.Tests
{
    [TestFixture]
    public class SqlConverterTests
    {
        [TestCase("mysql")]
        [TestCase("plsql")]
        [TestCase("tsql")]
        public void Convert_SqlSyntax_WithoutErrors(string dialect)
        {
            Language detectedDialect = LanguageUtils.ParseLanguages(dialect).ToList()[0];

            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, dialect, "examples"),
                detectedDialect, Stage.Ust);
        }

        [Test]
        public void Convert_PlSqlPattern_WithoutErrors()
        {
            TestUtility.CheckFile(Path.Combine(TestUtility.TestsDataPath, "plsql_patterns.sql"), Stage.Ust,
                language: PlSql.Language);
        }

        [Test]
        public void Convert_TSqlPattern_WithoutErrors()
        {
            TestUtility.CheckFile(Path.Combine(TestUtility.TestsDataPath, "tsql_patterns.sql"), Stage.Ust,
                language: TSql.Language);
        }
    }
}
