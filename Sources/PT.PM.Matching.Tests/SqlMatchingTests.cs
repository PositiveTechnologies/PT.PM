using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using PT.PM.Patterns.PatternsRepository;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    public class SqlMatchingTests
    {
        private IPatternsRepository patternsRepository;

        [SetUp]
        public void Init()
        {
            patternsRepository = new DefaultPatternRepository();
        }

        [Test]
        public void Match_TestPatternsPlSql_MatchedAllDefault()
        {
            Match_TestPatternsSql_MatchedAllDefault(Language.PlSql, "PlSql/plsql_patterns.sql");
        }

        [Test]
        public void Match_TestPatternsTSql_MatchedAllDefault()
        {
            Match_TestPatternsSql_MatchedAllDefault(Language.TSql, "TSql/tsql_patterns.sql");
        }

        private void Match_TestPatternsSql_MatchedAllDefault(Language language, string patternsFileName)
        {
            var path = Path.Combine(TestHelper.TestsDataPath, patternsFileName.NormDirSeparator());
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, language, patternsRepository);
            WorkflowResult workflowResult = workflow.Process();
            MatchingResultDto[] matchingResults = workflowResult.MatchingResults
                .ToDto(workflow.SourceCodeRepository)
                .OrderBy(r => r.PatternKey).ToArray();
            var patternDtos = patternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Is(language)).ToArray();

            foreach (var dto in patternDtos)
            {
                Assert.Greater(matchingResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
        }
    }
}
