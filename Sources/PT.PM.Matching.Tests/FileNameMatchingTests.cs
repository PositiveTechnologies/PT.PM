using NUnit.Framework;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class FileNameMatchingTests
    {
        private const string TestFileName = "C:/Users/Unnamed/examples/sample.php";

        [TestCase(TestFileName)]
        [TestCase("**/examples/*")]
        [TestCase("**/Users/**/examples/*")]
        [TestCase("**/Users/**/examples/*.php")]
        [TestCase("*.php")]
        [TestCase("*.*")]
        public void MatchWildcard(string wildcard)
        {
            var pathMatcher = new FileNameMatcher();
            var regex = pathMatcher.WildcardToRegex(wildcard);
            Assert.IsTrue(regex.IsMatch(TestFileName));
        }

        [TestCase("**/Temp/**/*")]
        [TestCase("**/Unnamed/**/examples/*.txt")]
        [TestCase("*.txt")]
        public void MatchWildcard_NotMatched(string wildcard)
        {
            var pathMatcher = new FileNameMatcher();
            var regex = pathMatcher.WildcardToRegex(wildcard);
            Assert.IsFalse(regex.IsMatch(TestFileName));
        }

        [TestCase("**/USERS/**/EXAMPLES/*")]
        public void MatchWildcard_CaseInsensitive(string wildcard)
        {
            var pathMatcher = new FileNameMatcher() { IsCaseInsensitive = true };
            var regex = pathMatcher.WildcardToRegex(wildcard);
            Assert.IsTrue(regex.IsMatch(TestFileName));
        }
    }
}
