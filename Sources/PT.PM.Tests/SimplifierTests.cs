using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using PT.PM.TestUtils;

namespace PT.PM.Tests
{
    [TestFixture]
    public class SimplifierTests
    {
        [Test]
        public void Simplify_PhpCodeWithConstants_ConstantsFolded()
        {
            string code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "simplify-sample.php"));

            var matches = PatternMatchingUtils.GetMatches(code, "<[\"Hello World!\"]>", Language.Php);
            Assert.AreEqual(new LineColumnTextSpan(2, 7, 2, 29), matches[0].LineColumnTextSpan); // TODO: should be (2, 8, 2, 30)
            
            matches = PatternMatchingUtils.GetMatches(code, "<[86400]>", Language.Php);
            Assert.AreEqual(new LineColumnTextSpan(3, 6, 3, 18), matches[0].LineColumnTextSpan);
            
            matches = PatternMatchingUtils.GetMatches(code, "<[42]>", Language.Php);
            Assert.AreEqual(new LineColumnTextSpan(4, 6, 4, 15), matches[0].LineColumnTextSpan);
            
            matches = PatternMatchingUtils.GetMatches(code, "<[-3]>", Language.Php);
            Assert.AreEqual(new LineColumnTextSpan(5, 6, 5, 8), matches[0].LineColumnTextSpan);
            
            //matches = PatternMatchingUtils.GetMatches(code, "<[-3.1]>", Language.Php); // TODO: fix float literal patterns
            //Assert.AreEqual(new LineColumnTextSpan(6, 6, 6, 10), matches.Length);
        }

        [Test]
        public void Simplify_JavaCodeWithConstantCharArray_ArrayFolded()
        {
            string code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "FoldArrayOfChars.java"));

            var matches = PatternMatchingUtils.GetMatches(code, "<[\"none\"]>", Language.Java);
            Assert.AreEqual(new LineColumnTextSpan(3, 21, 3, 44), matches[0].LineColumnTextSpan); // TODO: should be (3, 20, 3, 42)
        }

        [Test]
        public void Simplify_MultiMultiPattern_RemovedDuplicates()
        {
            var input = new PatternStatements
            {
                Statements = new List<PatternUst>
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
            var normalizer = new PatternNormalizer { Logger = logger };
            PatternUst result = normalizer.Visit(input);

            var statements = ((PatternStatements)result).Statements;
            var invocation = (PatternInvocationExpression)statements.ElementAt(0);
            Assert.AreEqual(1, ((PatternArgs)invocation.Arguments).Args.Count(child => child is PatternMultipleExpressions));
        }

        [Test]
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
                new PatternIntLiteral(0),
                new PatternIntLiteral(42),
                new PatternIntLiteral(100),
                new PatternNot(new PatternStringLiteral("42")),
                new PatternNot(new PatternStringLiteral("42")),
                new PatternStringLiteral("42"),
                new PatternStringLiteral("42"),
                new PatternStringLiteral("Hello World!")
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
