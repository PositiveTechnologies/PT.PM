using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common.SourceRepository;
using PT.PM.TestUtils;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class PhpMatchingTests
    {
        [Test]
        public void Match_TestPatternsPhp_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.php");
            var sourceRep = new FileSourceRepository(path);

            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceRep, Global.PatternsRepository) {Logger = logger};
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches
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
