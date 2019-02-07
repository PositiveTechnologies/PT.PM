using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common.SourceRepository;
using PT.PM.TestUtils;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class JavaMatchingTests
    {
        [Test]
        public void Match_TestPatternsJava_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.java");
            var sourceRep = new FileSourceRepository(path);

            var logger = new TestLogger();
            var workflow = new Workflow(sourceRep, Global.PatternsRepository) {Logger = logger};
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches.ToDto().OrderBy(r => r.PatternKey);
            IEnumerable<PatternDto> patternDtos = Global.PatternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains("Java")).ToArray();
            foreach (PatternDto dto in patternDtos)
            {
                Assert.Greater(matchResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }

            var improperValidationEmptyMethodPartial = patternDtos.Single(dto => dto.Description.StartsWith("ImproperValidationEmptyMethodPartial"));
            Assert.AreEqual(1, matchResults.Count(r => r.PatternKey == improperValidationEmptyMethodPartial.Key));

            var improperValidationEmptyMethodFull = patternDtos.Single(dto => dto.Description.StartsWith("ImproperValidationEmptyMethodFull"));
            Assert.AreEqual(3, matchResults.Count(r => r.PatternKey == improperValidationEmptyMethodFull.Key));

            var missingReceiverPermission = patternDtos.Single(dto => dto.Description.StartsWith("MissingReceiverPermission"));
            Assert.AreEqual(1, matchResults.Count(r => r.PatternKey == missingReceiverPermission.Key));

            var missingBroadcasterPermission = patternDtos.Single(dto => dto.Description.StartsWith("MissingBroadcasterPermission"));
            Assert.AreEqual(1, matchResults.Count(r => r.PatternKey == missingBroadcasterPermission.Key));

            var cookieNotSentOverSslDto = patternDtos.Single(dto => dto.Description.StartsWith("CookieNotSentOverSSL"));
            var cookieNotSentOverSslResults =
                matchResults.Where(r => r.PatternKey == cookieNotSentOverSslDto.Key).ToArray();

            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsSimple")));
            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsComplex")));
            Assert.AreEqual(1, cookieNotSentOverSslResults.Count(r => r.MatchedCode.Contains("emailCookieExistsAnotherVarName")));

            var useOfNullPointerException = patternDtos.Single(dto => dto.Description.StartsWith("Use of NullPointerException"));
            Assert.AreEqual(1, matchResults.Count(r => r.PatternKey == useOfNullPointerException.Key));
        }

        [Test]
        public void Match_Issue156_ObjectCreationByFullyQualifiedName()
        {
            var workflow = new Workflow(
                new MemorySourceRepository(@"
                    public class ProcessBuilderExample
                    {
                        public static void main(String[] args)
                        {
                            ProcessBuilder pb = new Java.Lang.ProcessBuilder(""bbb"", ""aaa"");
                            Process process = pb.start();
                        }
                    }",
                    "test.java"),
                new DslPatternRepository("new Java.Lang.ProcessBuilder(...)", "java"));

            WorkflowResult result = workflow.Process();

            Assert.AreEqual(1, result.TotalMatchesCount);
        }
    }
}