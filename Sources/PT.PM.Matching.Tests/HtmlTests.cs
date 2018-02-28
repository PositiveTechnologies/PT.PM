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
            WorkflowResult result = TestUtility.CheckFile("Patterns.html", Stage.Match,
                isIgnoreFilenameWildcards: true);
            Assert.AreEqual(2, result.MatchResults.Count);
            Assert.AreEqual(TextSpan.FromBounds(94, 102), ((MatchResult)result.MatchResults[0]).TextSpan);
            var secondResult = (MatchResult)result.MatchResults[1];
            Assert.AreEqual(TextSpan.FromBounds(71, 85), secondResult.TextSpans[0]);
            Assert.AreEqual(TextSpan.FromBounds(127, 135), secondResult.TextSpans[1]);
        }
    }
}