using System.IO;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;

namespace PT.PM.SqlParseTreeUst.Tests
{
    [TestFixture]
    public class SqlConverterTests
    {
        [TestCase(Language.PlSql)]
        [TestCase(Language.TSql)]
        [TestCase(Language.MySql)]
        public void Convert_SqlSyntax_WithoutErrors(Language dialect)
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, dialect.ToString().ToLowerInvariant(), "examples"),
                dialect, Stage.Ust);
        }

        [Test]
        public void Convert_PlSqlPattern_WithoutErrors()
        {
            TestUtility.CheckFile(Path.Combine(TestUtility.TestsDataPath, "plsql_patterns.sql"), Stage.Ust,
                language: Language.PlSql);
        }

        [Test]
        public void Convert_TSqlPattern_WithoutErrors()
        {
            TestUtility.CheckFile(Path.Combine(TestUtility.TestsDataPath, "tsql_patterns.sql"), Stage.Ust,
                language: Language.TSql);
        }
    }
}
