using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void CompressEscape_TestString_UnescapedDecompressedIsEqual()
        {
            var testData = "test_string0-9`,привет мир!{\"'}";
            var compressed = StringCompressorEscaper.CompressEscape(testData);
            var decompressed = StringCompressorEscaper.UnescapeDecompress(compressed);

            Assert.AreEqual(testData, decompressed);
        }

        [Test]
        public void JsonSerialize_PatternWithVar_JsonEqualsToDsl()
        {
            var patternNode = new PatternRootUst
            {
                Node = new PatternStatements
                {
                    Statements = new List<PatternBase>
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

            var jsonSerializer = new JsonUstSerializer();
            jsonSerializer.Indented = true;
            jsonSerializer.IncludeTextSpans = false;

            string json = jsonSerializer.Serialize(patternNode);
            Ust nodeFromJson = jsonSerializer.Deserialize(json);

            var dslSeializer = new DslProcessor() { PatternExpressionInsideStatement = false };
            var nodeFromDsl = dslSeializer.Deserialize("<[@pwd:password]> = #; ... #(#*, <[@pwd]>, #*);");

            Assert.IsTrue(nodeFromJson.Equals(patternNode));
            Assert.IsTrue(nodeFromJson.Equals(nodeFromDsl));
        }

        [Test]
        public void JsonDeserialize_PatternWithoutFormatAndLanguages_CorrectlyProcessed()
        {
            var data = "[{\"Key\":\"96\",\"Value\":\"(<[expr]>.)?<[(?i)(password|pwd)]> = <[\\\"\\\\w*\\\"]>\"}]";
            var stringEnumConverter = new StringEnumConverter();
            var languageFlagsConverter = new PatternJsonSafeConverter();

            var patternDtos = JsonConvert.DeserializeObject<List<PatternDto>>(data, stringEnumConverter, languageFlagsConverter);
            Assert.AreEqual(1, patternDtos.Count);
        }
    }
}
