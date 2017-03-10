using System.IO;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Tests;
using PT.PM.Patterns;
using PT.PM.Patterns.PatternsRepository;
using NUnit.Framework;

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
            var matchingResults = TestHelper.CheckProject(
                TestProjects.CSharpProjects.Single(p => p.Key == projectKey), Language.CSharp, Stage.Match);

            Assert.AreEqual(13, matchingResults.Length);
            Assert.AreEqual(matchingResults.Count(r =>
                r.BeginLine == 17 && r.SourceFile.EndsWith(@"WebGoat\AddNewUser.aspx.cs".NormDirSeparator())), 1);
            Assert.AreEqual(matchingResults.Count(r =>
                r.BeginLine == 1703 && r.SourceFile.EndsWith(@"WebGoat\Code\SQLiteMembershipProvider.cs".NormDirSeparator())), 1);
            Assert.AreEqual(matchingResults.Count(r =>
                r.BeginLine == 1705 && r.SourceFile.EndsWith(@"WebGoat\Code\SQLiteMembershipProvider.cs".NormDirSeparator())), 1);
            Assert.AreEqual(matchingResults.Count(r =>
                r.BeginLine == 1766 && r.SourceFile.EndsWith(@"WebGoat\Code\SQLiteMembershipProvider.cs".NormDirSeparator())), 1);
            Assert.AreEqual(matchingResults.Count(r =>
                r.BeginLine == 28 && r.SourceFile.EndsWith(@"WebGoat\Content\EncryptVSEncode.aspx.cs".NormDirSeparator())), 1);
        }

        [Test]
        public void Match_TestPatternsCSharp_MatchedAllDefault()
        {
            var path = Path.Combine(TestHelper.TestsDataPath, "Patterns.cs");
            var sourceCodeRep = new FileCodeRepository(path);

            var workflow = new Workflow(sourceCodeRep, Language.CSharp, patternsRepository);

            var matchingResults = workflow.Process().ToArray();
            var patternDtos = patternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Is(LanguageFlags.CSharp)).ToArray();
            foreach (var dto in patternDtos)
            {
                 Assert.Greater(matchingResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
        }

        [Test]
        public void Match_TestPatternsAspx_MatchedExpected()
        {
            var path = Path.Combine(TestHelper.TestsDataPath, "Patterns.aspx");
            var sourceCodeRep = new FileCodeRepository(path);
            var workflow = new Workflow(sourceCodeRep, Language.CSharp, patternsRepository);
            var matchingResults = workflow.Process().ToArray();

            Assert.IsTrue(matchingResults[0].MatchedCode.Contains("Password"));
            Assert.IsTrue(matchingResults[1].MatchedCode.Contains("try"));
        }

        [Test]
        public void Match_HardcodedPasswordAspx_WithoutException()
        {
            var hardcodedPassRepository = new DslPatternRepository("<[(?i)password]> = <[\"\\w*\"]>", LanguageFlags.CSharp);
            var sourceCodeRep = new FileCodeRepository(Path.Combine(TestHelper.TestsDataPath, "HardcodedPassword.aspx"));
            var workflow = new Workflow(sourceCodeRep, Language.CSharp, hardcodedPassRepository);
            var matchingResults = workflow.Process().ToArray();

            Assert.IsTrue(matchingResults[0].MatchedCode.Contains("password = \"hardcoded\""));
            Assert.IsTrue(matchingResults[1].MatchedCode.Contains("PASSWORD = \"hardcoded\""));
        }
    }
}
