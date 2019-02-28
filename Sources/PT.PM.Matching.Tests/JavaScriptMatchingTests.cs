using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.Files;
using PT.PM.Common.SourceRepository;
using PT.PM.TestUtils;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class JavaScriptMatchingTests
    {
        [Test]
        public void Match_JavaScriptTestPatterns_MatchedExpected()
        {
            var jsCodeAndPatterns = new []
            {
                new Tuple<TextFile, string>(new TextFile("document.body.innerHTML=\"<svg/onload=alert(1)>\"", "patterns.js"), "#.innerHTML=<[\"\"]>"),
                new Tuple<TextFile, string>(new TextFile("document.write(\"\\u003csvg/onload\\u003dalert(1)\\u003e\")", "patterns.js"), "document.write(<[\"\"]>)"),
                new Tuple<TextFile, string>(new TextFile("$('<svg/onload=alert(1)>')", "patterns.js"), "$(<[\"\"]>)")
            };
            foreach (var tuple in jsCodeAndPatterns)
            {
                var matchResults = PatternMatchingUtils.GetMatches(tuple.Item1, tuple.Item2, Language.JavaScript);
                Assert.AreEqual(1, matchResults.Length, tuple.Item2 + " doesn't match " + tuple.Item1);
            }
        }

        [Test]
        public void Match_JavaScriptAndPhpPatternInsidePhp_MatchedExpected()
        {
            TextFile source = TextFile.Read(Path.Combine(TestUtility.TestsDataPath, "JavaScriptTestPatternsInsidePhp.php"));
            MatchResultDto[] matchResults = PatternMatchingUtils.GetMatches(source, "#.innerHTML=<[\"\"]>", Language.JavaScript);
            Assert.AreEqual(1, matchResults.Length);
        }

        [Test]
        public void Match_JavaScriptAndPhpPatternInsidePhp_MatchCorrectPatternDependsOnLanguage()
        {
            TextFile source = TextFile.Read(Path.Combine(TestUtility.TestsDataPath, "JavaScriptTestPatternsInsidePhp.php"));
            MatchResultDto[] matchResults;

            matchResults = PatternMatchingUtils.GetMatches(source, "#.innerHTML=<[\"\"]>",
                new[] { Language.JavaScript }, new[] { Language.JavaScript });
            Assert.AreEqual(1, matchResults.Length);

            matchResults = PatternMatchingUtils.GetMatches(source, "<[password]> = null",
                new[] { Language.Php }, new[] { Language.Php });
            Assert.AreEqual(1, matchResults.Length);

            matchResults = PatternMatchingUtils.GetMatches(source, "#.innerHTML=<[\"\"]>",
                new[] { Language.Php }, new[] { Language.JavaScript });
            Assert.AreEqual(0, matchResults.Length);

            matchResults = PatternMatchingUtils.GetMatches(source, "<[password]> = null",
                new[] { Language.JavaScript }, new[] { Language.Php });
            Assert.AreEqual(0, matchResults.Length);
        }

        [Test]
        public void Match_TestPatternsJavaScript_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.js");
            var sourceRep = new FileSourceRepository(path);

            var logger = new TestLogger();
            var workflow = new Workflow(sourceRep, Global.PatternsRepository) {Logger = logger};
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches
                .ToDto().OrderBy(r => r.PatternKey);
            IEnumerable<PatternDto> patternDtos = Global.PatternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains("JavaScript"));
            foreach (PatternDto dto in patternDtos)
            {
                Assert.Greater(matchResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
        }

        [Test]
        public void Match_PhpInJsInPhp_CorrectMatching()
        {
            string fileName = Path.Combine(TestUtility.GrammarsDirectory, "php", "examples", "php-js-php.php");
            TextFile source = TextFile.Read(Path.Combine(TestUtility.TestsDataPath, fileName));
            var matchResults = PatternMatchingUtils.GetMatches(source, "<[GLOBALS|frame_content]>",
                new[] { Language.Php, Language.JavaScript },
                new[] { Language.Php, Language.JavaScript });

            Assert.AreEqual(3, matchResults.Length);
            Assert.IsTrue(matchResults[0].MatchedCode.Contains("GLOBAL"));
            Assert.AreEqual(9, matchResults[0].LineColumnTextSpan.BeginLine);
            Assert.IsTrue(matchResults[1].MatchedCode.Contains("frame_content"));
            Assert.AreEqual(10, matchResults[1].LineColumnTextSpan.BeginLine);
        }
    }
}
