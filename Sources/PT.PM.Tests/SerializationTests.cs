using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Dsl;
using PT.PM.Patterns.Nodes;
using NUnit.Framework;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using PT.PM.Patterns;
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
            var pwdVar = new PatternVarDef { Id = "pwd", Values = new[] { new PatternIdToken("password") } };
            var patternNode = new PatternNode
            {
                Vars = new[] { pwdVar },
                Node = new PatternStatements(
                     new ExpressionStatement
                     {
                         Expression = new AssignmentExpression
                         {
                             Left = new PatternVarRef(pwdVar),
                             Right = new PatternExpression()
                         }
                     },
                     new PatternMultipleStatements(),
                     new ExpressionStatement
                     {
                         Expression = new InvocationExpression
                         {
                             Target = new PatternExpression(),
                             Arguments = new PatternExpressions(
                                 new PatternMultipleExpressions(),
                                 new PatternVarRef(pwdVar),
                                 new PatternMultipleExpressions())
                         }
                     }
                )
            };

            var jsonSerializer = new JsonAstNodeSerializer(typeof(UstNode), typeof(PatternVarDef));
            jsonSerializer.Indented = true;
            jsonSerializer.IncludeTextSpans = false;

            string json = jsonSerializer.Serialize(patternNode);
            UstNode nodeFromJson = jsonSerializer.Deserialize(json, LanguageExt.AllPatternLanguages);

            var dslSeializer = new DslProcessor() { PatternExpressionInsideStatement = false };
            var nodeFromDsl = dslSeializer.Deserialize("<[@pwd:password]> = #; ... #(#*, <[@pwd]>, #*);", LanguageExt.AllPatternLanguages);

            Assert.IsTrue(nodeFromJson.Equals(patternNode));
            Assert.IsTrue(nodeFromJson.Equals(nodeFromDsl));
        }

        [Test]
        public void JsonDeserialize_PatternWithoutFormatAndLanguages_CorrectlyProcessed()
        {
            var data = "[{\"Key\":\"96\",\"Value\":\"(<[expr]>.)?<[(?i)(password|pwd)]> = <[\\\"\\\\w*\\\"]>\"}]";
            var stringEnumConverter = new StringEnumConverter();
            var languageFlagsConverter = new PatternLanguageFlagsSafeConverter();

            var patternDtos = JsonConvert.DeserializeObject<List<PatternDto>>(data, stringEnumConverter, languageFlagsConverter);
            Assert.AreEqual(1, patternDtos.Count);
        }
    }
}
