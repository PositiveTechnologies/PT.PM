﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.Files;
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
            TextFile source = TextFile.Read(Path.Combine(TestUtility.TestsDataPath, "simplify-sample.php"));

            var matches = PatternMatchingUtils.GetMatches(source, "<[\"Hello World!\"]>", Language.Php);
            Assert.AreEqual(new LineColumnTextSpan(2, 6, 2, 30), matches[0].LineColumnTextSpan);

            matches = PatternMatchingUtils.GetMatches(source, "<[86400]>", Language.Php);
            Assert.AreEqual(new LineColumnTextSpan(3, 6, 3, 18), matches[0].LineColumnTextSpan);

            matches = PatternMatchingUtils.GetMatches(source, "<[42]>", Language.Php);
            Assert.AreEqual(new LineColumnTextSpan(4, 6, 4, 15), matches[0].LineColumnTextSpan);

            matches = PatternMatchingUtils.GetMatches(source, "<[-3]>", Language.Php);
            Assert.AreEqual(new LineColumnTextSpan(5, 6, 5, 8), matches[0].LineColumnTextSpan);

            //matches = PatternMatchingUtils.GetMatches(code, "<[-3.1]>", Language.Php); // TODO: fix float literal patterns
            //Assert.AreEqual(new LineColumnTextSpan(6, 6, 6, 10), matches.Length);
        }

        [Test]
        public void Unescape_PlSqlUnistr()
        {
            var source = new TextFile("test ( template => 'line1' || unistr('\\000a') || 'line2' );", "PlSqlUnistr.sql");
            var matches = PatternMatchingUtils.GetMatches(source, "<[\"line1\nline2\"]>", Language.PlSql);
            Assert.AreEqual(1, matches.Length); // TODO: should be correct matching location
        }

        [Test]
        public void Simplify_JavaCodeWithConstantCharArray_ArrayFolded()
        {
            TextFile source = TextFile.Read(Path.Combine(TestUtility.TestsDataPath, "FoldArrayOfChars.java"));

            var matches = PatternMatchingUtils.GetMatches(source, "<[\"none\"]>", Language.Java);
            Assert.AreEqual(new LineColumnTextSpan(3, 22, 3, 40), matches[0].LineColumnTextSpan);
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
            var logger = new TestLogger();
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

            var logger = new TestLogger();
            var normalizer = new PatternNormalizer { Logger = logger };

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
