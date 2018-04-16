using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;
using System.Linq;
using System.Collections.Generic;
using PT.PM.TestUtils;
using NUnit.Framework;
using PT.PM.Common.Nodes.GeneralScope;
using System.IO;

namespace PT.PM.JavaParseTreeUst.Tests
{
    [TestFixture]
    public class JavaConverterTests
    {
        [TestCase("ManyStringsConcat.java")]
        [TestCase("AllInOne7.java")]
        [TestCase("AllInOne8.java")]
        public void Convert_Java_WithoutErrors(string fileName)
        {
            if (fileName == "ManyStringsConcat.java")
            {
                Assert.Ignore("Not sufficient stack size");
            }

            TestUtility.CheckFile(Path.Combine(TestUtility.GrammarsDirectory, "java", "examples", fileName), Stage.Ust);
        }

        [Test]
        public void Convert_JavaPatternsWithErrors_MatchedResultsEqual()
        {
            var patternsLogger = new LoggerMessageCounter();
            TestUtility.CheckFile("Patterns.java", Stage.Match, patternsLogger);

            var patternWithErrorsLogger = new LoggerMessageCounter();
            TestUtility.CheckFile("PatternsWithParseErrors.java", Stage.Match, patternWithErrorsLogger, true);

            Assert.AreEqual(0, patternWithErrorsLogger.InfoMessageCount - patternsLogger.InfoMessageCount);
        }

        [Test]
        public void Convert_JavaArrayInitialization()
        {
            var sourceCodeRep = new MemoryCodeRepository(
                "class ArrayInitialization {\r\n" +
                    "public void init() {\r\n" +
                        "int[] arr1 = new int[] { 1, 2, 3 };\r\n" +
                        "int[][] arr2 = new int[1][2];\r\n" +
                        "int[][] arr3 = new int[1][];\r\n" +
                    "}\r\n" +
                "}",

                "ArrayInitialization.java"
            );

            var workflow = new Workflow(sourceCodeRep, stage: Stage.Ust);
            var workflowResult = workflow.Process();
            var ust = workflowResult.Usts.First();
            var intType = new TypeToken("int");

            var arrayData = new List<Tuple<List<Expression>, List<Expression>>>();
            // new int[] { 1, 2, 3 };
            arrayData.Add(new Tuple<List<Expression>, List<Expression>>(
                Enumerable.Range(1, 3).Select(num => new IntLiteral(num)).ToList<Expression>(),
                new List<Expression> { new IntLiteral(0) }
            ));
            // new int[1][2];
            arrayData.Add(new Tuple<List<Expression>, List<Expression>>(
                null,
                new List<Expression> { new IntLiteral(1), new IntLiteral(2) }
            ));
            // new int[1][];
            arrayData.Add(new Tuple<List<Expression>, List<Expression>>(
                null,
                new List<Expression> { new IntLiteral(1), new IntLiteral(0) }
            ));

            for (var i = 0; i < arrayData.Count; i++)
            {
                var data = arrayData[i];
                var arrayCreationExpression = new ArrayCreationExpression
                {
                    Type = intType,
                    Initializers = data.Item1,
                    Sizes = data.Item2
                };
                bool exist = ust.AnyDescendant(node => node.Equals(arrayCreationExpression));
                Assert.IsTrue(exist, "Test failed on " + i + " iteration.");
            }
        }

        [Test]
        public void Convert_Char_StringLiteralWithoutQuotes()
        {
            var sourceCodeRep = new MemoryCodeRepository(
                @"class foo {
                    bar() {
                        obj.f1 = 'a';
                        obj.f2 = ""'b'"";
                    }
                }",

                "StringLiteralWithoutQuotes.java"
            );

            var workflow = new Workflow(sourceCodeRep, stage: Stage.Ust);
            var workflowResult = workflow.Process();
            var ust = workflowResult.Usts.First();

            Assert.IsTrue(ust.AnyDescendant(ustNode =>
                ustNode is StringLiteral stringLiteral && stringLiteral.Text == "a"));
        }

        [Test]
        public void Convert_Java_BaseTypesExist()
        {
            string fileName = Path.Combine(TestUtility.GrammarsDirectory, "java", "examples", "AllInOne7.java");
            var workflowResults = TestUtility.CheckFile(fileName, Stage.Ust);
            var ust = workflowResults.Usts.First();
            bool result = ust.AnyDescendant(descendant =>
            {
                return descendant is TypeDeclaration typeDeclaration &&
                       typeDeclaration.BaseTypes.Any(type => type.TypeText == "ActionListener");
            });
            Assert.IsTrue(result, "Ust doesn't contain type declaration node with ActionListener base type");
        }
    }
}
