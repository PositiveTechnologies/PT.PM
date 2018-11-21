using System.Collections.Generic;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class HtmlTests
    {
        [Test]
        public void Match_HtmlTestPatterns_MatchedExpected()
        {
            IReadOnlyList<IMatchResultBase> result = TestUtility.CheckFile("Patterns.html", Stage.Match,
                isIgnoreFilenameWildcards: true);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(TextSpan.FromBounds(94, 102), ((MatchResult)result[0]).TextSpan);
            var secondResult = (MatchResult)result[1];
            Assert.AreEqual(TextSpan.FromBounds(71, 85), secondResult.TextSpans[0]);
            Assert.AreEqual(TextSpan.FromBounds(127, 135), secondResult.TextSpans[1]);
        }
    }
}