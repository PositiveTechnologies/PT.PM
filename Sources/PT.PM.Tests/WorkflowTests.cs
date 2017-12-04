using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching;
using PT.PM.PhpParseTreeUst;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Tests
{
    [TestFixture]
    public class WorkflowTests
    {
        [Test]
        public void Process_JsonUst()
        {
            string filePath = Path.Combine(TestUtility.TestsDataPath, "Ust.json");
            SourceCodeRepository sourceCodeRepository = RepositoryFactory.CreateSourceCodeRepository(
                filePath,
                new List<Language>() { Php.Language }, "", true);
            var workflow = new Workflow(sourceCodeRepository)
            {
                StartStage = Stage.Ust,
                IsAsyncPatternsConversion = false
            };
            var result = workflow.Process();

            Assert.GreaterOrEqual(result.MatchResults.Count, 1);
            MatchResult match = result.MatchResults.First();
            Assert.IsFalse(match.TextSpan.IsEmpty);
        }
    }
}
