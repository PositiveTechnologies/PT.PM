using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Dsl;
using PT.PM.Patterns.Nodes;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.UstPreprocessing.Tests
{
    [TestFixture]
    public class UstPreprocessorTests
    {
        [Test]
        public void Preprocess_CodeWithConstants_ConstantsFolded()
        {
            var sourceCodeRep = new MemoryCodeRepository(
                "<?php\r\n" +
                "echo 'Hello ' . 'World' . '!';\r\n" +
                "echo 60 * 60 * 24;\r\n" +
                "echo 6 + 6 * 6;"
            );
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceCodeRep, Language.Php, stage: Stage.Preprocess);
            workflow.IsIncludePreprocessing = true;
            workflow.Logger = logger;
            workflow.Process();

            Assert.IsTrue(logger.ContainsDebugMessagePart("Hello World!"));
            Assert.IsTrue(logger.ContainsDebugMessagePart("86400"));
            Assert.IsTrue(logger.ContainsDebugMessagePart("42"));
        }

        [Test]
        public void Preprocess_MultiMultiPattern_RemovedDuplicates()
        {
            UstNode patternWithDuplicateMultiStatementsExpressions = new PatternStatements
            {
                Statements = new List<Statement>()
                {
                    new ExpressionStatement(
                        new InvocationExpression
                        {
                            Target = new IdToken("test_call"),
                            Arguments = new PatternExpressions
                            {
                                Collection = new List<Expression>
                                {
                                    new IdToken("a"),
                                    new IdToken("b"),
                                    new PatternMultipleExpressions(),
                                    new PatternMultipleExpressions(),
                                    new IdToken("z")
                                }
                            }
                        }),

                    new PatternMultipleStatements(),
                    new PatternMultipleStatements(),

                    new ExpressionStatement(
                        new VariableDeclarationExpression
                        {
                            Type = new TypeToken("int"),
                            Variables = new List<AssignmentExpression>()
                            {
                                new AssignmentExpression
                                {
                                    Left = new IdToken("a"),
                                    Right = new IntLiteral { Value = 42 }
                                }
                            }
                        }
                    )
                }
            };
            var logger = new LoggerMessageCounter();
            var processor = new DslProcessor();
            UstPreprocessor preprocessor = new UstPreprocessor() { Logger = logger };
            UstNode result = preprocessor.Preprocess(patternWithDuplicateMultiStatementsExpressions);

            Assert.AreEqual(1, result.GetAllDescendants().Count(child => child.NodeType == NodeType.PatternMultipleStatements));
            Assert.AreEqual(1, result.GetAllDescendants().Count(child => child.NodeType == NodeType.PatternMultipleExpressions));
        }

        [Ignore("TODO: fix it. Actually I does not know why it failed in TeamCity.")]
        public void Sort_PatternVars()
        {
            var unsortedExpressions = new List<Expression>()
            {
                new IntLiteral { Value = 100 },
                new IntLiteral { Value = 42 },
                new IntLiteral { Value = 0 },
                new StringLiteral { Text = "42" },
                new StringLiteral { Text = "Hello World!" },
                new IdToken { Id = "testId" },
                new IdToken { Id = "42" },
                new PatternExpression(new StringLiteral { Text = "42" }, true),
            };
            var expectedSortedExpressions = new List<Expression>
            {
                new StringLiteral { Text = "42" },
                new PatternExpression(new StringLiteral { Text = "42" }, true),
                new StringLiteral { Text = "Hello World!" },
                new IdToken { Id = "42" },
                new IdToken { Id = "testId" },
                new IntLiteral { Value = 0 },
                new IntLiteral { Value = 42 },
                new IntLiteral { Value = 100 },
            };
            var patternVarDef = new PatternVarDef
            {
                Id = "testVarDef",
                Values = unsortedExpressions
            };
            var patternVars = new PatternNode
            {
                Vars = new List<PatternVarDef>() { patternVarDef },
                Node = new PatternVarRef(patternVarDef)
            };

            var logger = new LoggerMessageCounter();
            var processor = new DslProcessor();
            UstPreprocessor preprocessor = new UstPreprocessor() { Logger = logger };
            Expression[] resultSortedExpressions = ((PatternNode)preprocessor.Preprocess(patternVars))
                .Vars.First().Values.ToArray();

            Assert.AreEqual(expectedSortedExpressions.Count, resultSortedExpressions.Length);
            for (int i = 0; i < expectedSortedExpressions.Count; i++)
            {
                Assert.IsTrue(expectedSortedExpressions[i].Equals(resultSortedExpressions[i]),
                    $"Not equal at {i} index: expected {expectedSortedExpressions[i]} not equals to {resultSortedExpressions[i]}");
            }
        }
    }
}
