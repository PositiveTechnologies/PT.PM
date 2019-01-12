using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;

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
            Language detectedDialect = LanguageUtils.ParseLanguages(dialect).ToList()[0];

            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, dialect, "examples"),
                detectedDialect, Stage.ParseTree);
        }
    }
}
