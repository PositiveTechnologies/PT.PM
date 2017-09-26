using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Dsl;
using PT.PM.Matching.Patterns;
using PT.PM.Matching.PatternsRepository;
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
        private MemoryPatternsRepository patternsRep;
        private MemoryCodeRepository sourceCodeRep;
        private Workflow workflow;

        [SetUp]
        public void Init()
        {
            patternsConverter = new PatternConverter(new JsonUstSerializer());
            patternsRep = new MemoryPatternsRepository();
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
                "test_call_5(1, $password2, 2);"
            );
            workflow = new Workflow(sourceCodeRep, Language.Php, patternsRep)
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
        [TestCase("#(#*, <[~e]>, #*)", new[] { 0, 1, 3 })]
        public void Match_PatternExpressionsInCalls(string patternData, params int[] matchMethodNumbers)
        {
            var processor = new DslProcessor();
            var patternNode = (PatternRootUst)processor.Deserialize(patternData);
            patternNode.DebugInfo = patternData;
            patternsRep.Add(patternsConverter.ConvertBack(new List<PatternRootUst>() { patternNode }));
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchingResultDto> matchingResults = workflowResult.MatchingResults.ToDto();
            patternsRep.Clear();

            Assert.AreEqual(matchMethodNumbers.Contains(0) ? 1 : 0, matchingResults.Count(r => r.MatchedCode.StartsWith("test_call_0")));
            Assert.AreEqual(matchMethodNumbers.Contains(1) ? 1 : 0, matchingResults.Count(r => r.MatchedCode.StartsWith("test_call_1")));
            Assert.AreEqual(matchMethodNumbers.Contains(2) ? 1 : 0, matchingResults.Count(r => r.MatchedCode.StartsWith("test_call_2")));
            Assert.AreEqual(matchMethodNumbers.Contains(3) ? 1 : 0, matchingResults.Count(r => r.MatchedCode.StartsWith("test_call_3")));
            Assert.AreEqual(matchMethodNumbers.Contains(4) ? 1 : 0, matchingResults.Count(r => r.MatchedCode.StartsWith("test_call_4")));
        }

        [TestCase("<[@pwd:password]> = #; ... #(#*, <[@pwd]>, #*);")]
        [TestCase("<[@pwd:username]> = #; ... #(#*, <[@pwd]>, #*);")]
        public void Match_PatternVarWithRegex(string patternData)
        {
            Assert.Ignore("Won't be supported in future versions of PT.PM");

            var processor = new DslProcessor();
            var patternNode = (PatternRootUst)processor.Deserialize(patternData);
            patternNode.DebugInfo = patternData;
            patternsRep.Add(patternsConverter.ConvertBack(new List<PatternRootUst>() { patternNode }));
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchingResultDto> matchingResults = workflowResult.MatchingResults.ToDto();
            patternsRep.Clear();

            int expectedMatchingCount = patternData.Contains("password") ? 1 : 0;
            Assert.AreEqual(expectedMatchingCount, matchingResults.Count());
        }

        [TestCase("<[@pwd:password2]> = #; ...                       #(#*, <[@pwd]>, #*);")]
        public void Match_PasswordCheckInsideStatement(string patternData)
        {
            var processor = new DslProcessor();
            var patternNode = (PatternRootUst)processor.Deserialize(patternData);
            patternNode.DebugInfo = patternData;
            patternsRep.Add(patternsConverter.ConvertBack(new List<PatternRootUst>() { patternNode }));
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchingResultDto> matchingResults = workflowResult.MatchingResults.ToDto();
            patternsRep.Clear();

            int expectedMatchingCount = patternData.Contains("~<[@pwd]>.Length") ? 0 : 1;
            Assert.AreEqual(expectedMatchingCount, matchingResults.Count());
        }

        [Test]
        public void Match_PatternWithNegation_CorrectCount()
        {
            var code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "XxeSample.java"));
            var pattern = "new XMLUtil().parse(<[~\".*\"]>)";

            var matchingResults = PatternMatchingUtils.GetMatchings(code, pattern, Language.Java);
            Assert.AreEqual(4, matchingResults.Length);
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

            var matchingResults = PatternMatchingUtils.GetMatchings(code, pattern, Language.Php);

            Assert.AreEqual(2, matchingResults[0].BeginLine);
            Assert.AreEqual(2, matchingResults[0].BeginColumn);
            Assert.AreEqual(2, matchingResults[0].EndLine);
            Assert.AreEqual(11, matchingResults[0].EndColumn);

            Assert.AreEqual(3, matchingResults[1].BeginLine);
            Assert.AreEqual(3, matchingResults[1].BeginColumn);
            Assert.AreEqual(3, matchingResults[1].EndLine);
            Assert.AreEqual(12, matchingResults[1].EndColumn);

            Assert.AreEqual(6, matchingResults[2].BeginLine);
            Assert.AreEqual(5, matchingResults[2].BeginColumn);
            Assert.AreEqual(7, matchingResults[2].EndLine);
            Assert.AreEqual(16, matchingResults[2].EndColumn);
        }

        [Test]
        public void Create_PatternWithWrongLanguage_ThrowsException()
        {
            Assert.Throws(typeof(ArgumentException), () => new PatternDto() { Languages = new HashSet<Language>() { Language.Aspx } });
        }
    }
}
