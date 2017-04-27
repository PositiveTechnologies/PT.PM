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
            Assert.AreEqual(2, result.MatchingResults.Count);
        }
    }
}
