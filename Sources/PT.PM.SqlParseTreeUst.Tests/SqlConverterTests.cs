using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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

        //[TestCase("TSQL Samples")]
        //[Ignore("Add TSQL samples from codebuff repository: https://github.com/antlr/codebuff/tree/master/corpus/sql/training")]
        public void Convert_TSqlFiles_WithoutErrors(string projectKey)
        {
            TestUtility.CheckProject(
                TestProjects.TSqlProjects.Single(p => p.Key == projectKey), TSql.Language, Stage.Ust);
        }

        [Test]
        public void Convert_TSqlSyntax_WithoutErrors()
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.TestsDataPath, "TSql"), TSql.Language, Stage.Ust);
        }
    }
}
