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
            WorkflowResult result = TestHelper.CheckFile("Patterns.html", Language.Html, Stage.Match,
                isIgnoreFileNameWildcards: true);
            Assert.AreEqual(3, result.MatchingResults.Count);
            Assert.AreEqual(TextSpan.FromBounds(94, 102), result.MatchingResults[0].TextSpan);
            Assert.AreEqual(TextSpan.FromBounds(71, 85), result.MatchingResults[1].TextSpan);
            Assert.AreEqual(TextSpan.FromBounds(127, 135), result.MatchingResults[2].TextSpan);
        }
    }
}
