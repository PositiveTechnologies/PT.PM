using NUnit.Framework;
using PT.PM.TestUtils;
using System.IO;
using System.Linq;

namespace PT.PM.SqlParseTreeUst.Tests
{
    [TestFixture]
    public class SqlConverterTests
    {
        [Test]
        public void Convert_PlSqlFiles_WithoutErrors()
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, "plsql", "examples"),
                PlSql.Language, Stage.Ust);
        }

        [Test]
        public void Convert_TSqlSyntax_WithoutErrors()
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, "tsql", "examples"),
                TSql.Language, Stage.Ust);
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
