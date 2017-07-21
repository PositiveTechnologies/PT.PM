using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using System.Linq;

namespace PT.PM.JavaScriptParseTreeUst.Tests
{
    [TestFixture]
    public class JavaScriptParserTests
    {
        [TestCase("JavaScript-Style-Guide-v14.0.0")]
        public void Parse_JavaScriptProject_WithoutErrors(string projectKey)
        {
            TestHelper.CheckProject(TestProjects.JavaScriptProjects
                .Single(p => p.Key == projectKey), Language.JavaScript, Stage.Parse);
        }
    }
}
