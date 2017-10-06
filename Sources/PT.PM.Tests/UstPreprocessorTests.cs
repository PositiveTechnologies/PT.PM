using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Dsl;
using PT.PM.JavaParseTreeUst;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using PT.PM.PhpParseTreeUst;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Tests
{
    [TestFixture]
    public class UstPreprocessorTests
    {
        [Test]
        public void Preprocess_PhpCodeWithConstants_ConstantsFolded()
        {
            var sourceCodeRep = new MemoryCodeRepository(
                "<?php\r\n" +
                "echo 'Hello ' . 'World' . '!';\r\n" +
                "echo 60 * 60 * 24;\r\n" +
                "echo 6 + 6 * 6;\r\n" +
                "$a = -3;\r\n" +
                "$b = -3.1;"
            );
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceCodeRep, Php.Language, stage: Stage.Preprocess);
            workflow.IsIncludePreprocessing = true;
            workflow.Logger = logger;
            workflow.Process();

            Assert.IsTrue(logger.ContainsDebugMessagePart("Hello World!"));
            Assert.IsTrue(logger.ContainsDebugMessagePart("86400"));
            Assert.IsTrue(logger.ContainsDebugMessagePart("42"));
            Assert.IsTrue(logger.ContainsDebugMessagePart("-3"));
            Assert.IsTrue(logger.ContainsDebugMessagePart("-3.1"));
        }

        [Test]
        public void Preprocess_JavaCodeWithConstantCharArray_ArrayFolded()
        {
            var sourceCodeRep = new MemoryCodeRepository(
                "class Wrapper {\r\n" +
                "  public void init() {\r\n" +
                "    char[] array = { 'n', 'o', 'n', 'e' };\r\n" +
                "  }\r\n" +
                "}"
            );

            var workflow = new Workflow(sourceCodeRep, Java.Language, stage: Stage.Preprocess);
            workflow.IsIncludePreprocessing = true;
            var ust = workflow.Process().Usts.First();

            Assert.IsTrue(ust.AnyDescendant(
                node => node is StringLiteral str && str.Text == "none"));
        }

        [Test]
        public void Preprocess_MultiMultiPattern_RemovedDuplicates()
        {
            var input = new PatternStatements
            {
                Statements = new List<PatternUst>()
                {
                    new PatternInvocationExpression
                    {
                        Target = new PatternIdToken("test_call"),
                        Arguments = new PatternArgs
                        (
                            new PatternIdToken("a"),
                            new PatternIdToken("b"),
                            new PatternMultipleExpressions(),
                            new PatternMultipleExpressions(),
                            new PatternIdToken("z")
                        )
                    },

                    new PatternVarOrFieldDeclaration
                    {
                        Type = new PatternIdToken("int"),
                        Assignment = new PatternAssignmentExpression
                        {
                            Left = new PatternIdToken("a"),
                            Right = new PatternIntLiteral(42)
                        }
                    }
                }
            };
            var logger = new LoggerMessageCounter();
            var processor = new DslProcessor();
            var normalizer = new PatternNormalizer() { Logger = logger };
            PatternUst result = normalizer.Visit(input);

            var statements = ((PatternStatements)result).Statements;
            var invocation = (PatternInvocationExpression)statements.ElementAt(0);
            Assert.AreEqual(1, invocation.Arguments.Args.Count(child => child is PatternMultipleExpressions));
        }

        [Test]
        [Ignore("Conside a unified hashing function for UST and pattern types")]
        public void Sort_PatternVars_CorrectOrder()
        {
            var unsorted = new PatternOr
            (
                new PatternStringLiteral("42"),
                new PatternIntLiteral(100),
                new PatternIntLiteral(42),
                new PatternIntLiteral(0),
                new PatternStringLiteral("42"),
                new PatternNot(new PatternStringLiteral("42")),
                new PatternStringLiteral("Hello World!"),
                new PatternIdToken("testId"),
                new PatternIdToken("42"),
                new PatternNot(new PatternStringLiteral("42"))
            );
            var expectedSorted = new PatternOr
            (
                new PatternIdToken("42"),
                new PatternIdToken("testId"),
                new PatternStringLiteral("42"),
                new PatternStringLiteral("42"),
                new PatternStringLiteral("Hello World!"),
                new PatternNot(new PatternStringLiteral("42")),
                new PatternNot(new PatternStringLiteral("42")),
                new PatternIntLiteral(0),
                new PatternIntLiteral(42),
                new PatternIntLiteral(100)
            );

            var logger = new LoggerMessageCounter();
            var processor = new DslProcessor();
            var normalizer = new PatternNormalizer() { Logger = logger };

            var actualPattern = (PatternOr)normalizer.Visit(unsorted);
            List<PatternUst> actualAlternatives = actualPattern.Patterns;
            List<PatternUst> expectedAlternatives = expectedSorted.Patterns;

            Assert.AreEqual(expectedAlternatives.Count, actualAlternatives.Count);
            for (int i = 0; i < expectedAlternatives.Count; i++)
            {
                Assert.IsTrue(expectedAlternatives[i].Equals(actualAlternatives[i]),
                    $"Not equal at {i} index: expected {expectedAlternatives[i]} not equals to {actualAlternatives[i]}");
            }
        }
    }
}
