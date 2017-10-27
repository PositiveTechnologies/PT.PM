using NUnit.Framework;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class CSharpMatchingTests
    {
        [Test]
        public void Match_TestPatternsCSharp_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.cs");
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, Global.PatternsRepository);
            IEnumerable<MatchingResultDto> matchingResults = workflow.Process().MatchingResults.ToDto();
            var patternDtos = Global.PatternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains("CSharp")).ToArray();
            foreach (PatternDto dto in patternDtos)
            {
                Assert.Greater(matchingResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
        }

        [Test]
        public void Match_TestPatternsAspx_MatchedExpected()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.aspx");
            var sourceCodeRep = new FileCodeRepository(path);
            var workflow = new Workflow(sourceCodeRep, Global.PatternsRepository);
            IEnumerable<MatchingResultDto> matchingResults = workflow.Process().MatchingResults.ToDto();

            Assert.IsTrue(matchingResults.ElementAt(0).MatchedCode.Contains("Password"));
            Assert.IsTrue(matchingResults.ElementAt(1).MatchedCode.Contains("Random"));
            Assert.IsTrue(matchingResults.ElementAt(2).MatchedCode.Contains("try"));
        }

        [Test]
        public void Match_HardcodedPasswordAspx_WithoutException()
        {
            var hardcodedPassRepository = new DslPatternRepository("<[(?i)password]> = <[\"\\w*\"]>", "CSharp");
            var sourceCodeRep = new FileCodeRepository(Path.Combine(TestUtility.TestsDataPath, "HardcodedPassword.aspx"));
            var workflow = new Workflow(sourceCodeRep, hardcodedPassRepository);
            IEnumerable<MatchingResultDto> matchingResults = workflow.Process().MatchingResults.ToDto();

            string matching = matchingResults.ElementAt(0).MatchedCode;
            Assert.IsTrue(matching.Contains("password") && matching.Contains("hardcoded"));

            matching = matchingResults.ElementAt(1).MatchedCode;
            Assert.IsTrue(matching.Contains("PASSWORD") && matching.Contains("hardcoded"));
        }
    }
}
