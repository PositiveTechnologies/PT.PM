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

            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceCodeRep, Global.PatternsRepository) {Logger = logger};
            workflow.Process();
            var matchResults = logger.Matches.ToDto();
            var patternDtos = Global.PatternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains("CSharp")).ToArray();
            foreach (PatternDto dto in patternDtos)
            {
                Assert.Greater(matchResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
        }

        [Test]
        public void Match_TestPatternsAspx_MatchedExpected()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.aspx");
            var sourceCodeRep = new FileCodeRepository(path);
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceCodeRep, Global.PatternsRepository) {Logger = logger};
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches.ToDto();

            Assert.IsTrue(matchResults.ElementAt(0).MatchedCode.Contains("Password"));
            Assert.IsTrue(matchResults.ElementAt(1).MatchedCode.Contains("Random"));
            Assert.IsTrue(matchResults.ElementAt(2).MatchedCode.Contains("try"));
        }

        [Test]
        public void Match_HardcodedPasswordAspx_WithoutException()
        {
            var hardcodedPassRepository = new DslPatternRepository("<[(?i)password]> = <[\"\\w*\"]>", "CSharp");
            var sourceCodeRep = new FileCodeRepository(Path.Combine(TestUtility.TestsDataPath, "HardcodedPassword.aspx"));
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceCodeRep, hardcodedPassRepository) {Logger = logger};
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches.ToDto();

            string match = matchResults.ElementAt(0).MatchedCode;
            Assert.IsTrue(match.Contains("password") && match.Contains("hardcoded"));

            match = matchResults.ElementAt(1).MatchedCode;
            Assert.IsTrue(match.Contains("PASSWORD") && match.Contains("hardcoded"));
        }
    }
}
