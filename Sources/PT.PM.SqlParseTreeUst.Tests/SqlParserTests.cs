using NUnit.Framework;
using PT.PM.TestUtils;
using System.IO;

namespace PT.PM.SqlParseTreeUst.Tests
{
    [TestFixture]
    public class SqlParserTests
    {
        [Test]
        public void Parse_PlSqlFiles_WithoutErrors()
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.TestsDataPath, "PlSql"), PlSql.Language, Stage.Parse);
        }

        [Test]
        public void Parse_TSqlSyntax_WithoutErrors()
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.TestsDataPath, "TSql"), TSql.Language, Stage.Parse);
        }
    }
}
