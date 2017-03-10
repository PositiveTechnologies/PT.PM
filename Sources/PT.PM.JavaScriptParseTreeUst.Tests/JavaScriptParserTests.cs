using PT.PM.Common;
using PT.PM.Common.Tests;
using NUnit.Framework;
using System.Linq;

namespace PT.PM.JavaScriptParseTreeUst.Tests
{
    [TestFixture]
    public class JavaScriptParserTests
    {
        [TestCase("helloworld.js")]
        public void Parse_JavaScriptSyntax_WithoutErrors(string fileName)
        {
            TestHelper.CheckFile(fileName, Language.JavaScript, Stage.Parse);
        }

        [TestCase("JavaScript-Style-Guide-v14.0.0")]
        //[TestCase("bootstrap-v3.3.7")] // TODO: Improve JavaScript parser performance
        public void Parse_JavaScriptProject_WithoutErrors(string projectKey)
        {
            TestHelper.CheckProject(TestProjects.JavaScriptProjects
                .Single(p => p.Key == projectKey), Language.JavaScript, Stage.Parse);
        }
    }
}
