using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
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
            var workflow = new Workflow(sourceCodeRep, Language.Php, stage: Stage.Preprocess);
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

            var workflow = new Workflow(sourceCodeRep, Language.Java, stage: Stage.Preprocess);
            workflow.IsIncludePreprocessing = true;
            var ust = workflow.Process().Usts.First();

            Assert.IsTrue(ust.DoesAnyDescendantMatchPredicate(
                node => node is StringLiteral str && str.Text == "none"));
        }

        [Test]
        public void Preprocess_MultiMultiPattern_RemovedDuplicates()
        {
            Ust patternWithDuplicateMultiStatementsExpressions = new PatternStatements
            {
                Statements = new List<PatternBase>()
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
            UstSimplifier preprocessor = new UstSimplifier() { Logger = logger };
            Ust result = preprocessor.Preprocess(patternWithDuplicateMultiStatementsExpressions);

            Assert.AreEqual(1, result.GetAllDescendants().Count(child => child is PatternMultipleExpressions));
        }

        [Test]
        public void Sort_PatternVars_CorrectOrder()
        {
            if (CommonUtils.IsRunningOnLinux)
            {
                Assert.Ignore("TODO: fix failed unit-test on mono (Linux)");
            }

            var unsortedExpressions = new List<PatternBase>()
            {
                new PatternStringLiteral("42"),
                new PatternIntLiteral(100),
                new PatternIntLiteral(42),
                new PatternIntLiteral(0),
                new PatternStringLiteral("42"),
                new PatternNot(new PatternStringLiteral("42")),
                new PatternStringLiteral("Hello World!"),
                new PatternIdToken("testId"),
                new PatternIdToken("42"),
                new PatternNot(new PatternStringLiteral("42")),
            };
            var expectedSortedExpressions = new List<PatternBase>
            {
                new PatternStringLiteral("42"),
                new PatternStringLiteral("Hello World!"),
                new PatternIdToken("42"),
                new PatternIdToken("testId"),
                new PatternIntLiteral(0),
                new PatternIntLiteral(42),
                new PatternIntLiteral(100),
                new PatternStringLiteral("42"),
                new PatternNot(new PatternStringLiteral("42")),
                new PatternNot(new PatternStringLiteral("42")),
            };
            var patternVarDef = new PatternVar
            {
                Id = "testVarDef",
                Value = new PatternOr(unsortedExpressions)
            };
            var patternVars = new PatternRootUst
            {
                Node = patternVarDef
            };

            var logger = new LoggerMessageCounter();
            var processor = new DslProcessor();
            UstSimplifier preprocessor = new UstSimplifier() { Logger = logger };
            Assert.Inconclusive();

            /*Expression[] resultSortedExpressions = ((PatternRootUst)preprocessor.Preprocess(patternVars))
                .Vars.First().Values.ToArray();

            Assert.AreEqual(expectedSortedExpressions.Count, resultSortedExpressions.Length);
            for (int i = 0; i < expectedSortedExpressions.Count; i++)
            {
                Assert.IsTrue(expectedSortedExpressions[i].Equals(resultSortedExpressions[i]),
                    $"Not equal at {i} index: expected {expectedSortedExpressions[i]} not equals to {resultSortedExpressions[i]}");
            }*/
        }
    }
}
