using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching.PatternsRepository;
using PT.PM.Patterns.PatternsRepository;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class PhpMatchingTests
    {
        private IPatternsRepository patternsRepository;

        [SetUp]
        public void Init()
        {
            patternsRepository = new DefaultPatternRepository();
        }

        [Test]
        public void Match_TestPatternsPhp_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.php");
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, Language.Php, patternsRepository);
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchingResultDto> matchingResults = workflowResult.MatchingResults
                .ToDto()
                .OrderBy(r => r.PatternKey);
            PatternDto[] patternDtos = patternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains(Language.Php)).ToArray();
            foreach (var dto in patternDtos)
            {
                Assert.Greater(matchingResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
            Assert.AreEqual(1, matchingResults.Count(r =>
                r.MatchedCode.Contains("Configure") &&
                r.MatchedCode.Contains("write") &&
                r.MatchedCode.Contains("3")));
            Assert.AreEqual(0, matchingResults.Count(r => 
                r.MatchedCode.Contains("Configure") &&
                r.MatchedCode.Contains("write") &&
                r.MatchedCode.Contains("50")));
        }
    }
}
