using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.CSharpParseTreeUst;
using PT.PM.Dsl;
using PT.PM.JavaParseTreeUst;
using PT.PM.Matching.Patterns;
using PT.PM.Matching.PatternsRepository;
using PT.PM.PhpParseTreeUst;
using PT.PM.TestUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class PatternMatchingTests
    {
        private PatternConverter patternsConverter;
        private MemoryPatternsRepository patternsRepository;
        private MemoryCodeRepository sourceCodeRep;
        private Workflow workflow;

        [SetUp]
        public void Init()
        {
            patternsConverter = new PatternConverter();
            patternsRepository = new MemoryPatternsRepository();
            sourceCodeRep = new MemoryCodeRepository(
                "<?php \n" +
                "test_call_0();\n" +
                "test_call_1(a);\n" +
                "test_call_2(a, e);\n" +
                "test_call_3(c, d, a);\n" +
                "test_call_4(c, d, a, e);\n" +
                "\n" +
                "$password = a;\n" +
                "call_with_password_param(1, $password, 2);\n" +
                "\n" +
                "$password2 = \"1234\";\n" +
                "if ($password2->Length > 0) { }\n" +
                "test_call_5(1, $password2, 2);",

                "samples.php"
            );
            workflow = new Workflow(sourceCodeRep, patternsRepository)
            {
                Logger = new LoggerMessageCounter()
            };
        }

        [TestCase("#()", new [] { 0 })]
        [TestCase("#(a)", new [] { 1 })]
        [TestCase("#(#*)", new [] { 0, 1, 2, 3, 4 })]
        [TestCase("#(a, #*)", new[] { 1, 2 })]
        [TestCase("#(#*, a)", new[] { 1, 3 })]
        [TestCase("#(#*, a, #*)", new[] { 1, 2, 3, 4 })]
        //[TestCase("#(#*, <[~e]>, #*)", new[] { 0, 1, 3 })]
        public void Match_PatternExpressionsInCalls(string patternData, params int[] matchMethodNumbers)
        {
            var processor = new DslProcessor();
            PatternRoot patternNode = processor.Deserialize(patternData);
            patternNode.DebugInfo = patternData;
            patternsRepository.Add(patternsConverter.ConvertBack(new List<PatternRoot>() { patternNode }));
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchResultDto> matchResults = workflowResult.MatchResults.ToDto();
            patternsRepository.Clear();

            Assert.AreEqual(matchMethodNumbers.Contains(0) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_0")));
            Assert.AreEqual(matchMethodNumbers.Contains(1) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_1")));
            Assert.AreEqual(matchMethodNumbers.Contains(2) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_2")));
            Assert.AreEqual(matchMethodNumbers.Contains(3) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_3")));
            Assert.AreEqual(matchMethodNumbers.Contains(4) ? 1 : 0, matchResults.Count(r => r.MatchedCode.StartsWith("test_call_4")));
        }

        [TestCase("<[@pwd:password]> = #; ... #(#*, <[@pwd]>, #*);")]
        [TestCase("<[@pwd:username]> = #; ... #(#*, <[@pwd]>, #*);")]
        public void Match_PatternVarWithRegex(string patternData)
        {
            var processor = new DslProcessor();
            PatternRoot patternNode = processor.Deserialize(patternData);
            patternNode.DebugInfo = patternData;
            patternsRepository.Add(patternsConverter.ConvertBack(new List<PatternRoot>() { patternNode }));
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchResultDto> matchResults = workflowResult.MatchResults.ToDto();
            patternsRepository.Clear();

            int expectedMatchingCount = patternData.Contains("password") ? 1 : 0;
            Assert.AreEqual(expectedMatchingCount, matchResults.Count());
        }

        [TestCase("<[@pwd:password2]> = #; ...                       #(#*, <[@pwd]>, #*);")]
        public void Match_PasswordCheckInsideStatement(string patternData)
        {
            var processor = new DslProcessor();
            PatternRoot patternNode = processor.Deserialize(patternData);
            patternNode.DebugInfo = patternData;
            patternsRepository.Add(patternsConverter.ConvertBack(new List<PatternRoot>() { patternNode }));
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchResultDto> matchResults = workflowResult.MatchResults.ToDto();
            patternsRepository.Clear();

            int expectedMatchingCount = patternData.Contains("~<[@pwd]>.Length") ? 0 : 1;
            Assert.AreEqual(expectedMatchingCount, matchResults.Count());
        }

        [Test]
        public void Match_PatternWithNegation_CorrectCount()
        {
            var code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "XxeSample.java"));
            var pattern = "new XMLUtil().parse(<[~\".*\"]>)";

            var matchResults = PatternMatchingUtils.GetMatches(code, pattern, Java.Language);
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

            MatchResultDto[] matchResults = PatternMatchingUtils.GetMatches(code, pattern, Php.Language);

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
        public void Create_PatternWithWrongLanguage_ThrowsException()
        {
            Assert.Throws(typeof(ArgumentException), () => new PatternRoot()
            {
                Languages = new HashSet<Language>() { Aspx.Language }
            });
        }
    }
}
