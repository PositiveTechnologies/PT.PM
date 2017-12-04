using NUnit.Framework;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class PhpMatchingTests
    {
        [Test]
        public void Match_TestPatternsPhp_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.php");
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, Global.PatternsRepository);
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchResultDto> matchResults = workflowResult.MatchResults
                .ToDto()
                .OrderBy(r => r.PatternKey);
            IEnumerable<PatternDto> patternDtos = Global.PatternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains("Php"));
            foreach (PatternDto dto in patternDtos)
            {
                Assert.Greater(matchResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
            Assert.AreEqual(1, matchResults.Count(r =>
                r.MatchedCode.Contains("Configure") &&
                r.MatchedCode.Contains("write") &&
                r.MatchedCode.Contains("3")));
            Assert.AreEqual(0, matchResults.Count(r => 
                r.MatchedCode.Contains("Configure") &&
                r.MatchedCode.Contains("write") &&
                r.MatchedCode.Contains("50")));
        }
    }
}
