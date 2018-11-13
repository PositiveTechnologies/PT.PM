using NUnit.Framework;
using PT.PM.TestUtils;
using System.IO;

namespace PT.PM.SqlParseTreeUst.Tests
{
    [TestFixture]
    public class SqlParserTests
    {
        [TestCase("mysql")]
        [TestCase("plsql")]
        [TestCase("tsql")]
        public void Parse_SqlFiles_WithoutErrors(string dialect)
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, dialect, "examples"),
                SqlTestUtils.Languages[dialect], Stage.ParseTree);
        }
    }
}
