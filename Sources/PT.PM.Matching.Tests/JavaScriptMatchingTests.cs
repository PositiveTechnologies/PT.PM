using NUnit.Framework;
using PT.PM.Common.CodeRepository;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.PhpParseTreeUst;
using PT.PM.TestUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                new Tuple<string, string>("document.body.innerHTML=\"<svg/onload=alert(1)>\"", "#.innerHTML=<[\"\"]>"),
                new Tuple<string, string>("document.write(\"\\u003csvg/onload\\u003dalert(1)\\u003e\")", "document.write(<[\"\"]>)"),
                new Tuple<string, string>("$('<svg/onload=alert(1)>')", "$(<[\"\"]>)")
            };
            foreach (var tuple in jsCodeAndPatterns)
            {
                var matchResults = PatternMatchingUtils.GetMatches(tuple.Item1, tuple.Item2, JavaScript.Language);
                Assert.AreEqual(1, matchResults.Length, tuple.Item2 + " doesn't match " + tuple.Item1);
            }
        }

        [Test]
        public void Match_JavaScriptAndPhpPatternInsidePhp_MatchedExpected()
        {
            string code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "JavaScriptTestPatternsInsidePhp.php"));
            MatchResultDto[] matchResults = PatternMatchingUtils.GetMatches(code, "#.innerHTML=<[\"\"]>", JavaScript.Language);
            Assert.AreEqual(1, matchResults.Length);
        }

        [Test]
        public void Match_JavaScriptAndPhpPatternInsidePhp_MatchCorrectPatternDependsOnLanguage()
        {
            string code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "JavaScriptTestPatternsInsidePhp.php"));
            MatchResultDto[] matchResults;

            matchResults = PatternMatchingUtils.GetMatches(code, "#.innerHTML=<[\"\"]>",
                new[] { JavaScript.Language }, new[] { JavaScript.Language });
            Assert.AreEqual(1, matchResults.Length);

            matchResults = PatternMatchingUtils.GetMatches(code, "<[password]> = null",
                new[] { Php.Language }, new[] { Php.Language });
            Assert.AreEqual(1, matchResults.Length);

            matchResults = PatternMatchingUtils.GetMatches(code, "#.innerHTML=<[\"\"]>",
                new[] { Php.Language }, new[] { JavaScript.Language });
            Assert.AreEqual(0, matchResults.Length);

            matchResults = PatternMatchingUtils.GetMatches(code, "<[password]> = null",
                new[] { JavaScript.Language }, new[] { Php.Language });
            Assert.AreEqual(0, matchResults.Length);
        }

        [Test]
        public void Match_TestPatternsJavaScript_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.js");
            var sourceCodeRep = new FileCodeRepository(path);

            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceCodeRep, Global.PatternsRepository) {Logger = logger};
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
            string code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, fileName));
            var matchResults = PatternMatchingUtils.GetMatches(code, "<[GLOBALS|frame_content]>",
                new[] { Php.Language, JavaScript.Language },
                new[] { Php.Language, JavaScript.Language });

            Assert.AreEqual(3, matchResults.Length);
            Assert.IsTrue(matchResults[0].MatchedCode.Contains("GLOBAL"));
            Assert.AreEqual(9, matchResults[0].LineColumnTextSpan.BeginLine);
            Assert.IsTrue(matchResults[1].MatchedCode.Contains("frame_content"));
            Assert.AreEqual(10, matchResults[1].LineColumnTextSpan.BeginLine);
        }
    }
}
