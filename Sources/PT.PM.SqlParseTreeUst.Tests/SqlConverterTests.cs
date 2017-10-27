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
            TestUtility.CheckProject(Path.Combine(TestUtility.TestsDataPath, "PlSql"), PlSql.Language, Stage.Ust);
        }

        [Test]
        public void Convert_TSqlSyntax_WithoutErrors()
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.TestsDataPath, "TSql"), TSql.Language, Stage.Ust);
        }
    }
}
