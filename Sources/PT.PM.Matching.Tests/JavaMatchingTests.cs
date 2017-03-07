using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Tests;
using PT.PM.Patterns.PatternsRepository;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class JavaMatchingTests
    {
        private IPatternsRepository patternsRepository;

        [SetUp]
        public void Init()
        {
            patternsRepository = new DefaultPatternRepository();
        }

        [Test]
        public void Match_TestPatternsJava_MatchedAllDefault()
        {
            var path = Path.Combine(TestHelper.TestsDataPath, "Patterns.java");
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, Language.Java, patternsRepository);

            var matchingResults = workflow.Process().OrderBy(r => r.PatternKey).ToArray();
            var patternDtos = patternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Is(LanguageFlags.Java)).ToArray();
            foreach (var dto in patternDtos)
            {
                Assert.Greater(matchingResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }

            var missingReceiverPermission = patternDtos.Single(dto => dto.Description.StartsWith("MissingReceiverPermission"));
            Assert.AreEqual(1, matchingResults.Count(r => r.PatternKey == missingReceiverPermission.Key));

            var missingBroadcasterPermission = patternDtos.Single(dto => dto.Description.StartsWith("MissingBroadcasterPermission"));
            Assert.AreEqual(1, matchingResults.Count(r => r.PatternKey == missingBroadcasterPermission.Key));

            var cookieNotSentOverSslDto = patternDtos.Single(dto => dto.Description.StartsWith("CookieNotSentOverSSL"));
            var cookieNotSentOverSslResults =
                matchingResults.Where(r => r.PatternKey == cookieNotSentOverSslDto.Key).ToArray();

            Assert.AreEqual(0, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieNotExistsSimple")));
            Assert.AreEqual(0, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieNotExistsComplex")));

            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsTwoPatterns1")));
            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsSimple")));
            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsComplex")));
            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsAnotherVarName")));
        }
    }
}
