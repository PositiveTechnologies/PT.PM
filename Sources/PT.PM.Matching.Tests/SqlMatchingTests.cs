using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Utils;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    public class SqlMatchingTests
    {
        [TestCase("tsql", "tsql_patterns.sql")]
        [TestCase("plsql", "plsql_patterns.sql")]
        [TestCase("mysql", "mysql_patterns.sql")]
        public void Match_TestPatternsSql_MatchedAllDefault(string dialect, string patternsFileName)
        {
            var language = LanguageUtils.ParseLanguages(dialect).ToList()[0];
            var path = Path.Combine(TestUtility.TestsDataPath, patternsFileName.NormalizeDirSeparator());
            var sourceCodeRep = new FileCodeRepository(path);

            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceCodeRep, Global.PatternsRepository) {Logger = logger};
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches
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
