using NUnit.Framework;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class JavaMatchingTests
    {
        [Test]
        public void Match_TestPatternsJava_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.java");
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, Global.PatternsRepository);
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchingResultDto> matchingResults = workflowResult.MatchingResults
                .ToDto()
                .OrderBy(r => r.PatternKey);
            IEnumerable<PatternDto> patternDtos = Global.PatternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains("Java")).ToArray();
            foreach (PatternDto dto in patternDtos)
            {
                Assert.Greater(matchingResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }

            var improperValidationEmptyMethodPartial = patternDtos.Single(dto => dto.Description.StartsWith("ImproperValidationEmptyMethodPartial"));
            Assert.AreEqual(1, matchingResults.Count(r => r.PatternKey == improperValidationEmptyMethodPartial.Key));

            var improperValidationEmptyMethodFull = patternDtos.Single(dto => dto.Description.StartsWith("ImproperValidationEmptyMethodFull"));
            Assert.AreEqual(3, matchingResults.Count(r => r.PatternKey == improperValidationEmptyMethodFull.Key));

            var missingReceiverPermission = patternDtos.Single(dto => dto.Description.StartsWith("MissingReceiverPermission"));
            Assert.AreEqual(1, matchingResults.Count(r => r.PatternKey == missingReceiverPermission.Key));

            var missingBroadcasterPermission = patternDtos.Single(dto => dto.Description.StartsWith("MissingBroadcasterPermission"));
            Assert.AreEqual(1, matchingResults.Count(r => r.PatternKey == missingBroadcasterPermission.Key));

            var cookieNotSentOverSslDto = patternDtos.Single(dto => dto.Description.StartsWith("CookieNotSentOverSSL"));
            var cookieNotSentOverSslResults =
                matchingResults.Where(r => r.PatternKey == cookieNotSentOverSslDto.Key).ToArray();

            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsSimple")));
            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsComplex")));
            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsAnotherVarName")));

            var useOfNullPointerException = patternDtos.Single(dto => dto.Description.StartsWith("Use of NullPointerException"));
            Assert.AreEqual(1, matchingResults.Count(r => r.PatternKey == useOfNullPointerException.Key));
        }
    }
}