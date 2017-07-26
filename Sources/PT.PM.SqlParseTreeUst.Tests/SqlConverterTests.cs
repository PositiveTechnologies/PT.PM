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
            TestHelper.CheckProject(Path.Combine(TestHelper.TestsDataPath, "PlSql"), Language.PlSql, Stage.Convert);
        }

        //[TestCase("TSQL Samples")]
        //[Ignore("Add TSQL samples from codebuff repository: https://github.com/antlr/codebuff/tree/master/corpus/sql/training")]
        public void Convert_TSqlFiles_WithoutErrors(string projectKey)
        {
            TestHelper.CheckProject(
                TestProjects.TSqlProjects.Single(p => p.Key == projectKey), Language.TSql, Stage.Convert);
        }

        [Test]
        public void Convert_TSqlSyntax_WithoutErrors()
        {
            TestHelper.CheckProject(Path.Combine(TestHelper.TestsDataPath, "TSql"), Language.TSql, Stage.Convert);
        }
    }
}
