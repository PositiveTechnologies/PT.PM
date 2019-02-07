using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Common.Utils;
using PT.PM.TestUtils;

namespace PT.PM.Matching.Tests
{
    public class SqlMatchingTests
    {
        [TestCase(Language.TSql, "tsql_patterns.sql")]
        [TestCase(Language.PlSql, "plsql_patterns.sql")]
        [TestCase(Language.MySql, "mysql_patterns.sql")]
        public void Match_TestPatternsSql_MatchedAllDefault(Language sqlDialect, string patternsFileName)
        {
            var path = Path.Combine(TestUtility.TestsDataPath, patternsFileName.NormalizeDirSeparator());
            var sourceRep = new FileSourceRepository(path);

            var logger = new TestLogger();
            var workflow = new Workflow(sourceRep, Global.PatternsRepository) {Logger = logger};
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches
                .ToDto()
                .OrderBy(r => r.PatternKey);
            string sqlDialectString = sqlDialect.ToString();
            IEnumerable<PatternDto> patternDtos = Global.PatternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains(sqlDialectString));

            foreach (var dto in patternDtos)
            {
                Assert.Greater(matchResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
        }
    }
}
