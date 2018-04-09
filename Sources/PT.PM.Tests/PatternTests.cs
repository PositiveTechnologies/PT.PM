using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Matching.Json;
using PT.PM.Matching.Patterns;
using PT.PM.PhpParseTreeUst;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;

namespace PT.PM.Tests
{
    [TestFixture]
    public class PatternTests
    {
        SourceCodeRepository codeRepository = new MemoryCodeRepository(new Dictionary<string, string>
        {
            ["test.php"] = "<?php $a = 42;"
        }, Php.Language);

        PatternRoot pattern = new PatternRoot
        {
            Node = new PatternAssignmentExpression
            {
                Left = new PatternIdRegexToken(),
                Right = new PatternAnyExpression()
            },
        };

        JsonPatternSerializer jsonPatternSerializer = new JsonPatternSerializer
        {
            Indented = true,
        };

        [Test]
        public void Check_RepositoryFactory_Pattern()
        {
            string serialized = jsonPatternSerializer.Serialize(pattern);
            string filePath = Path.Combine(TestUtility.TestsOutputPath, "pattern.json");
            File.WriteAllText(filePath, serialized);

            Check(filePath);
        }

        [Test]
        public void Check_RepositoryFactory_Patterns()
        {
            string serialized = jsonPatternSerializer.Serialize(new[] { pattern });
            string filePath = Path.Combine(TestUtility.TestsOutputPath, "patterns.json");
            File.WriteAllText(filePath, serialized);

            Check(filePath);
        }

        [Test]
        public void Check_RepositoryFactory_PatternDto()
        {
            string serialized = jsonPatternSerializer.Serialize(pattern);
            string patternDto = JsonConvert.SerializeObject(new PatternDto() { Value = serialized });
            string filePath = Path.Combine(TestUtility.TestsOutputPath, "patternDto.json");
            File.WriteAllText(filePath, patternDto);

            Check(filePath);
        }

        [Test]
        public void Check_RepositoryFactory_PatternDtos()
        {
            string serialized = jsonPatternSerializer.Serialize(pattern);
            string patternDto = JsonConvert.SerializeObject(new[] { new PatternDto() { Value = serialized } });
            string filePath = Path.Combine(TestUtility.TestsOutputPath, "patternDtos.json");
            File.WriteAllText(filePath, patternDto);

            Check(filePath);
        }

        [Test]
        public void Check_RepositoryFactory_Dsl()
        {
            Check("<[.*]> = #");
        }

        [Test]
        public void JsonSerialize_PatternWithVar_JsonEqualsToDsl()
        {
            var patternRoot = new PatternRoot
            {
                Node = new PatternStatements
                {
                    Statements = new List<PatternUst>
                    {
                        new PatternAssignmentExpression
                        {
                             Left = new PatternVar("pwd") { Value = new PatternIdRegexToken("password") },
                             Right = new PatternAnyExpression()
                        },

                        new PatternInvocationExpression
                        {
                            Target = new PatternAnyExpression(),
                            Arguments = new PatternArgs(
                                new PatternMultipleExpressions(),
                                new PatternVar("pwd"),
                                new PatternMultipleExpressions())
                        }
                    }
                }
            };

            var jsonSerializer = new JsonPatternSerializer
            {
                Indented = true,
                IncludeTextSpans = false
            };

            string json = jsonSerializer.Serialize(patternRoot);
            PatternRoot nodeFromJson = jsonSerializer.Deserialize(new CodeFile(json) { IsPattern = true });

            var dslSeializer = new DslProcessor() { PatternExpressionInsideStatement = false };
            var nodeFromDsl = dslSeializer.Deserialize(
                new CodeFile("<[@pwd:password]> = #; ... #(#*, <[@pwd]>, #*);") { IsPattern = true });

            Assert.IsTrue(nodeFromJson.Node.Equals(patternRoot.Node));
            Assert.IsTrue(nodeFromJson.Node.Equals(nodeFromDsl.Node));
        }

        [Test]
        public void JsonDeserialize_PatternWithoutFormatAndLanguages_CorrectlyProcessed()
        {
            var data = "[{\"Key\":\"96\",\"Value\":\"(<[expr]>.)?<[(?i)(password|pwd)]> = <[\\\"\\\\w*\\\"]>\"}]";
            var patternDtos = JsonConvert.DeserializeObject<List<PatternDto>>(data);
            Assert.AreEqual(1, patternDtos.Count);
        }

        private void Check(string patternsString)
        {
            var logger = new LoggerMessageCounter();
            var patternsRepository = RepositoryFactory.CreatePatternsRepository(patternsString, logger);

            var workflow = new Workflow(codeRepository)
            {
                PatternsRepository = patternsRepository
            };
            var result = workflow.Process();

            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);
            Assert.AreEqual(1, result.MatchResults.Count);
        }
    }
}
