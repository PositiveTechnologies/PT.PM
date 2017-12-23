using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.SqlParseTreeUst;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    public class SqlMatchingTests
    {
        [Test]
        public void Match_TestPatternsPlSql_MatchedAllDefault()
        {
            Match_TestPatternsSql_MatchedAllDefault(PlSql.Language, "plsql_patterns.sql");
        }

        [Test]
        public void Match_TestPatternsTSql_MatchedAllDefault()
        {
            Match_TestPatternsSql_MatchedAllDefault(TSql.Language, "tsql_patterns.sql");
        }

        private void Match_TestPatternsSql_MatchedAllDefault(Language language, string patternsFileName)
        {
            var path = Path.Combine(TestUtility.TestsDataPath, patternsFileName.NormDirSeparator());
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, Global.PatternsRepository);
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchResultDto> matchResults = workflowResult.MatchResults
                .ToDto()
                .OrderBy(r => r.PatternKey);
            IEnumerable<PatternDto> patternDtos = Global.PatternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains(language.Key));

            foreach (var dto in patternDtos)
            {
                Assert.Greater(matchResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
        }
    }
}
