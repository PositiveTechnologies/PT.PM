using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [TestCase(@"TSql/control_flow.sql")]
        [TestCase(@"TSql/cursors.sql")]
        [TestCase(@"TSql/ddl_create_alter_database.sql")]
        [TestCase(@"TSql/ddl_create_drop_type.sql")]
        [TestCase(@"TSql/dml_delete.sql")]
        [TestCase(@"TSql/dml_insert.sql")]
        [TestCase(@"TSql/dml_openrowset.sql")]
        [TestCase(@"TSql/dml_select.sql")]
        [TestCase(@"TSql/dml_update.sql")]
        [TestCase(@"TSql/expressions.sql")]
        [TestCase(@"TSql/full_width_chars.sql")]
        [TestCase(@"TSql/predicates.sql")]
        [TestCase(@"TSql/procedures.sql")]
        [TestCase(@"TSql/statements.sql")]
        [TestCase(@"TSql/transactions.sql")]
        public void Convert_TSqlSyntax_WithoutErrors(string fileName)
        {
            TestHelper.CheckFile(fileName, Language.TSql, Stage.Convert);
        }
    }
}
