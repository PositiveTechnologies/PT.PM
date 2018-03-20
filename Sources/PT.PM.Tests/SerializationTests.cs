using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Matching.Json;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void Parse_TextSpan()
        {
            var textSpan = new TextSpan(42, 0);
            string textSpanString = textSpan.ToString();
            Assert.AreEqual(textSpan, TextUtils.ParseTextSpan(textSpanString));

            textSpan = new TextSpan(42, 5);
            textSpanString = textSpan.ToString();
            Assert.AreEqual(textSpan, TextUtils.ParseTextSpan(textSpanString));
        }

        [Test]
        public void Parse_LineColumnTextSpan()
        {
            var lcTextSpan = new LineColumnTextSpan(42, 1, 42, 1);
            string textSpanString = lcTextSpan.ToString();
            Assert.AreEqual(lcTextSpan, TextUtils.ParseLineColumnTextSpan(textSpanString));

            lcTextSpan = new LineColumnTextSpan(42, 1, 41, 5);
            textSpanString = lcTextSpan.ToString();
            Assert.AreEqual(lcTextSpan, TextUtils.ParseLineColumnTextSpan(textSpanString));
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
            var stringEnumConverter = new StringEnumConverter();

            var patternDtos = JsonConvert.DeserializeObject<List<PatternDto>>(data, stringEnumConverter);
            Assert.AreEqual(1, patternDtos.Count);
        }
    }
}
