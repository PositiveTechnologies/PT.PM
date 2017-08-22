using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.IO;

namespace PT.PM.SqlParseTreeUst.Tests
{
    [TestFixture]
    public class SqlParserTests
    {
        [Test]
        public void Parse_PlSqlFiles_WithoutErrors()
        {
            TestHelper.CheckProject(Path.Combine(TestHelper.TestsDataPath, "PlSql"), Language.PlSql, Stage.Parse);
        }

        [Test]
        public void Parse_TSqlSyntax_WithoutErrors()
        {
            TestHelper.CheckProject(Path.Combine(TestHelper.TestsDataPath, "TSql"), Language.TSql, Stage.Parse);
        }
    }
}
