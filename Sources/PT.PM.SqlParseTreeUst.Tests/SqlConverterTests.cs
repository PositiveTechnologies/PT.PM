using NUnit.Framework;
using PT.PM.TestUtils;
using System.IO;

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
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, dialect, "examples"),
                SqlTestUtils.Languages[dialect], Stage.Ust);
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
