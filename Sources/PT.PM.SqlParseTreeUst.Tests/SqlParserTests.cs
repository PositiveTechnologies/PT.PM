using System.IO;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;

namespace PT.PM.SqlParseTreeUst.Tests
{
    [TestFixture]
    public class SqlParserTests
    {
        [TestCase(Language.PlSql)]
        [TestCase(Language.TSql)]
        [TestCase(Language.MySql)]
        public void Parse_SqlFiles_WithoutErrors(Language language)
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, language.ToString().ToLowerInvariant(), "examples"),
                language, Stage.ParseTree);
        }
    }
}
