using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Tests;
using PT.PM.Patterns.PatternsRepository;
using NUnit.Framework;
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
            var path = Path.Combine(TestHelper.TestsDataPath, "Patterns.php");
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, Language.Php, patternsRepository);

            var matchingResults = workflow.Process().OrderBy(r => r.PatternKey).ToArray();
            var patternDtos = patternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Is(LanguageFlags.Php)).ToArray();
            foreach (var dto in patternDtos)
            {
                Assert.Greater(matchingResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
            Assert.AreEqual(1, matchingResults.Count(r => r.MatchedCode.Contains("Configure::write") && r.MatchedCode.Contains("3")));
            Assert.AreEqual(0, matchingResults.Count(r => r.MatchedCode.Contains("Configure::write") && r.MatchedCode.Contains("50")));
        }
    }
}
