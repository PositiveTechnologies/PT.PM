using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Common.Files;
using PT.PM.Dsl;
using PT.PM.Matching.Patterns;
using PT.PM.Matching.PatternsRepository;
using PT.PM.TestUtils;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class CommonMatchingTests
    {
        private PatternConverter patternsConverter;
        private MemoryPatternsRepository patternsRepository;
        private SourceRepository sourceRep;

        [SetUp]
        public void Init()
        {
            patternsConverter = new PatternConverter();
            patternsRepository = new MemoryPatternsRepository();
            sourceRep = new FileSourceRepository(Path.Combine(TestUtility.TestsDataPath, "common-matching.php"));
        }

        [TestCase("#()", new[] { 0 })]
        [TestCase("#(a)", new[] { 1 })]
        [TestCase("#(#*)", new[] { 0, 1, 2, 3, 4 })]
        [TestCase("#(a, #*)", new[] { 1, 2 })]
        [TestCase("#(#*, a)", new[] { 1, 3 })]
        [TestCase("#(#*, a, #*)", new[] { 1, 2, 3, 4 })]
        //[TestCase("#(#*, <[~e]>, #*)", new[] { 0, 1, 3 })]
        public void Match_PatternExpressionsInCalls(string patternData, params int[] matchMethodNumbers)
        {
            var processor = new DslProcessor();
            PatternRoot patternNode = processor.Deserialize(new TextFile(patternData) { PatternKey = patternData });
            patternNode.DebugInfo = patternData;
            patternsRepository.Add(patternsConverter.ConvertBack(new List<PatternRoot> { patternNode }));
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceRep, patternsRepository) {Logger = logger};
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches.ToDto();
            patternsRepository.Clear();

            Assert.AreEqual(matchMethodNumbers.Contains(0) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_0")));
            Assert.AreEqual(matchMethodNumbers.Contains(1) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_1")));
            Assert.AreEqual(matchMethodNumbers.Contains(2) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_2")));
            Assert.AreEqual(matchMethodNumbers.Contains(3) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_3")));
            Assert.AreEqual(matchMethodNumbers.Contains(4) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_4")));
        }

        [Test]
        public void Match_RefOutArg()
        {
            var codeRepository = new MemorySourceRepository("class P { void Main() { Func(ref a); } }", "test.cs");
            var patternsRepository = new DslPatternRepository("Func(a)", "CSharp");
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(codeRepository, patternsRepository) {Logger = logger};
            workflow.Process();

            Assert.AreEqual(1, logger.Matches.Count);
        }

        [TestCase("<[@pwd:password]> = #; ... #(#*, <[@pwd]>, #*);")]
        [TestCase("<[@pwd:username]> = #; ... #(#*, <[@pwd]>, #*);")]
        public void Match_PatternVarWithRegex(string patternData)
        {
            var processor = new DslProcessor();
            PatternRoot patternNode = processor.Deserialize(new TextFile(patternData) { PatternKey = patternData });
            patternNode.DebugInfo = patternData;
            patternsRepository.Add(patternsConverter.ConvertBack(new List<PatternRoot>() { patternNode }));
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceRep, patternsRepository) {Logger = logger};
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches.ToDto();
            patternsRepository.Clear();

            int expectedMatchingCount = patternData.Contains("password") ? 1 : 0;
            Assert.AreEqual(expectedMatchingCount, matchResults.Count());
        }

        [TestCase("<[@pwd:password2]> = #; ...                       #(#*, <[@pwd]>, #*);")]
        public void Match_PasswordCheckInsideStatement(string patternData)
        {
            var processor = new DslProcessor();
            PatternRoot patternNode = processor.Deserialize(new TextFile(patternData) { PatternKey = patternData });
            patternNode.DebugInfo = patternData;
            patternsRepository.Add(patternsConverter.ConvertBack(new List<PatternRoot>() { patternNode }));
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceRep, patternsRepository) { Logger = logger };
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches.ToDto();
            patternsRepository.Clear();

            int expectedMatchingCount = patternData.Contains("~<[@pwd]>.Length") ? 0 : 1;
            Assert.AreEqual(expectedMatchingCount, matchResults.Count());
        }

        [Test]
        public void Match_PatternAnyWithRegex()
        {
            PatternRoot pattern = new PatternRoot
            {
                DebugInfo = "Test PatternAny",
                Languages = new HashSet<Language> { Language.Php },
                Node = new PatternAssignmentExpression
                {
                    Left = new PatternAny("password"),
                    Right = new PatternStringRegexLiteral("hardcoded")
                }
            };
            patternsRepository.Add(patternsConverter.ConvertBack(new List<PatternRoot>() { pattern }));

            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceRep, patternsRepository) { Logger = logger };
            workflow.Process();
            IEnumerable<MatchResultDto> matchResults = logger.Matches.ToDto();
            patternsRepository.Clear();

            Assert.AreEqual(1, matchResults.Count());
        }

        [Test]
        public void Match_PatternAnyWithRegexAsSingleNode()
        {
            PatternRoot pattern = new PatternRoot
            {
                DebugInfo = "Test PatternAny",
                Languages = new HashSet<Language> { Language.Php },
                Node = new PatternAny("password")
            };
            patternsRepository.Add(patternsConverter.ConvertBack(new List<PatternRoot>() { pattern }));

            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceRep, patternsRepository) { Logger = logger };
            workflow.Process();
            patternsRepository.Clear();

            Assert.AreEqual(1, logger.Matches.Count);
            Assert.AreEqual(7, ((MatchResult)logger.Matches[0]).TextSpans.Length);
        }

        [Test]
        public void Match_PatternWithNegation_CorrectCount()
        {
            var code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "XxeSample.java"));
            var pattern = "new XMLUtil().parse(<[~\".*\"]>)";

            var matchResults = PatternMatchingUtils.GetMatches(code, pattern, Language.Java);
            Assert.AreEqual(4, matchResults.Length);
        }

        [Test]
        public void Match_Comments_CorrectMatchingPosition()
        {
            var code =
                "<?php\n" +
                "#password=secret\n" +
                "/*password=secret*/\n" +
                "/*\n" +
                "\n" +
                "    password\n" +
                "              =secret\n" +
                "*/" +
                "?>";
            var pattern = "Comment: <[ \"(?i)(password|pwd)\\s*(\\=|is|\\:)\" ]>";

            MatchResultDto[] matchResults = PatternMatchingUtils.GetMatches(code, pattern, Language.Php);

            LineColumnTextSpan textSpan0 = matchResults[0].LineColumnTextSpan;
            Assert.AreEqual(2, textSpan0.BeginLine);
            Assert.AreEqual(2, textSpan0.BeginColumn);
            Assert.AreEqual(2, textSpan0.EndLine);
            Assert.AreEqual(11, textSpan0.EndColumn);

            LineColumnTextSpan textSpan1 = matchResults[1].LineColumnTextSpan;
            Assert.AreEqual(3, textSpan1.BeginLine);
            Assert.AreEqual(3, textSpan1.BeginColumn);
            Assert.AreEqual(3, textSpan1.EndLine);
            Assert.AreEqual(12, textSpan1.EndColumn);

            LineColumnTextSpan textSpan2 = matchResults[2].LineColumnTextSpan;
            Assert.AreEqual(6, textSpan2.BeginLine);
            Assert.AreEqual(5, textSpan2.BeginColumn);
            Assert.AreEqual(7, textSpan2.EndLine);
            Assert.AreEqual(16, textSpan2.EndColumn);
        }

        [Test]
        public void Match_Reduce_CorrectMatchingPosition()
        {
            var processor = new DslProcessor();
            string pattern = "<[ \"\\d+\" ]>";
            PatternRoot patternNode = processor.Deserialize(new TextFile(pattern) { PatternKey = pattern });
            patternsRepository.Add(patternsConverter.ConvertBack(new List<PatternRoot> { patternNode }));
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceRep, patternsRepository) { Logger = logger };
            workflow.Process();
            List<MatchResultDto> matchResults = logger.Matches.ToDto().ToList();
            patternsRepository.Clear();

            Assert.AreEqual(2, matchResults.Count);

            LineColumnTextSpan textSpan = matchResults[1].LineColumnTextSpan;
            Assert.AreEqual(15, textSpan.BeginLine);
            Assert.AreEqual(12, textSpan.BeginColumn);
            Assert.AreEqual(17, textSpan.EndLine);
            Assert.AreEqual(7, textSpan.EndColumn);
        }

        [Test]
        public void Create_PatternWithWrongLanguage_ThrowsException()
        {
            Assert.Throws(typeof(ArgumentException), () => new PatternRoot
            {
                Languages = new HashSet<Language> { Language.Aspx }
            });
        }

        [Test]
        public void Match_Suppress_CorrectCount()
        {
            var code =
                "<?php $password = \"hardcoded\";\n" +
                "\n" +
                "// ptai: suppress\n" +
                "$password = \"hardcoded\"";

            var pattern = "<[password]> = <[\"\"]>";

            MatchResultDto[] matchResults = PatternMatchingUtils.GetMatches(code, pattern, Language.Php);

            Assert.AreEqual(2, matchResults.Length);
            Assert.AreEqual(1, matchResults.Count(matchResult => matchResult.Suppressed));
        }
    }
}
