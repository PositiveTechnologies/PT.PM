using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.Utils;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class FileNameMatchingTests
    {
        private static string TestFileName = "C:/Users/Unnamed/examples/sample.php".NormalizeDirSeparator();

        [TestCase("C:/Users/Unnamed/examples/sample.php")]
        [TestCase("**/examples/*")]
        [TestCase("**/Users/**/examples/*")]
        [TestCase("**/Users/**/examples/*.php")]
        [TestCase("**/USERS/**/EXAMPLES/*")]
        [TestCase("*.php")]
        [TestCase("*.*")]
        public void MatchWildcard(string wildcard)
        {
            var pathMatcher = new WildcardConverter();
            var regex = pathMatcher.Convert(wildcard);
            Assert.IsTrue(regex.IsMatch(TestFileName));
        }

        [TestCase("**/Temp/**/*")]
        [TestCase("**/Unnamed/**/examples/*.txt")]
        [TestCase("*.txt")]
        public void MatchWildcard_NotMatched(string wildcard)
        {
            var pathMatcher = new WildcardConverter();
            var regex = pathMatcher.Convert(wildcard);
            Assert.IsFalse(regex.IsMatch(TestFileName));
        }
    }
}
