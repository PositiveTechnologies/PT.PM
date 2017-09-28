using System.IO;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using PT.PM.Patterns;
using PT.PM.Patterns.PatternsRepository;
using NUnit.Framework;
using PT.PM.Matching.PatternsRepository;
using System.Collections.Generic;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class CSharpMatchingTests
    {
        private IPatternsRepository patternsRepository;

        [SetUp]
        public void Init()
        {
            patternsRepository = new DefaultPatternRepository();
        }

        [Test]
        public void Match_WebGoatNET_Matched()
        {
            string projectKey = "WebGoat.NET-1c6cab";
            WorkflowResult workflowResult = TestUtility.CheckProject(
                TestProjects.CSharpProjects.Single(p => p.Key == projectKey), Language.CSharp, Stage.Match);

            Assert.AreEqual(13, workflowResult.MatchingResults.Count);
            Assert.AreEqual(workflowResult.MatchingResults.Count(r =>
                r.TextSpan.Start == 400 && r.SourceCodeFile.FullPath.EndsWith(@"WebGoat\AddNewUser.aspx.cs".NormDirSeparator())), 1);
            Assert.AreEqual(workflowResult.MatchingResults.Count(r =>
                r.TextSpan.Start == 70174 && r.SourceCodeFile.FullPath.EndsWith(@"WebGoat\Code\SQLiteMembershipProvider.cs".NormDirSeparator())), 1);
            Assert.AreEqual(workflowResult.MatchingResults.Count(r =>
                r.TextSpan.Start == 70254 && r.SourceCodeFile.FullPath.EndsWith(@"WebGoat\Code\SQLiteMembershipProvider.cs".NormDirSeparator())), 1);
            Assert.AreEqual(workflowResult.MatchingResults.Count(r =>
                r.TextSpan.Start == 72299 && r.SourceCodeFile.FullPath.EndsWith(@"WebGoat\Code\SQLiteMembershipProvider.cs".NormDirSeparator())), 1);
            Assert.AreEqual(workflowResult.MatchingResults.Count(r =>
                r.TextSpan.Start == 618 && r.SourceCodeFile.FullPath.EndsWith(@"WebGoat\Content\EncryptVSEncode.aspx.cs".NormDirSeparator())), 1);
        }

        [Test]
        public void Match_TestPatternsCSharp_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.cs");
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, Language.CSharp, patternsRepository);
            IEnumerable<MatchingResultDto> matchingResults = workflow.Process().MatchingResults.ToDto();
            var patternDtos = patternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains(Language.CSharp)).ToArray();
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
            var workflow = new Workflow(sourceCodeRep, Language.CSharp, patternsRepository);
            IEnumerable<MatchingResultDto> matchingResults = workflow.Process().MatchingResults.ToDto();

            Assert.IsTrue(matchingResults.ElementAt(0).MatchedCode.Contains("Password"));
            Assert.IsTrue(matchingResults.ElementAt(1).MatchedCode.Contains("Random"));
            Assert.IsTrue(matchingResults.ElementAt(2).MatchedCode.Contains("try"));
        }

        [Test]
        public void Match_HardcodedPasswordAspx_WithoutException()
        {
            var hardcodedPassRepository = new DslPatternRepository("<[(?i)password]> = <[\"\\w*\"]>", Language.CSharp);
            var sourceCodeRep = new FileCodeRepository(Path.Combine(TestUtility.TestsDataPath, "HardcodedPassword.aspx"));
            var workflow = new Workflow(sourceCodeRep, Language.CSharp, hardcodedPassRepository);
            IEnumerable<MatchingResultDto> matchingResults = workflow.Process().MatchingResults.ToDto();

            string matching = matchingResults.ElementAt(0).MatchedCode;
            Assert.IsTrue(matching.Contains("password") && matching.Contains("hardcoded"));

            matching = matchingResults.ElementAt(1).MatchedCode;
            Assert.IsTrue(matching.Contains("PASSWORD") && matching.Contains("hardcoded"));
        }
    }
}
