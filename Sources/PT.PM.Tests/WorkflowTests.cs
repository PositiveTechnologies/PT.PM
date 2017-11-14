using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.PhpParseTreeUst;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;

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
                new List<Language>() { Php.Language }, "", Stage.Ust);
            var workflow = new Workflow(sourceCodeRepository)
            {
                StartStage = Stage.Ust
            };
            var result = workflow.Process();

            Assert.GreaterOrEqual(result.MatchingResults.Count, 1);
        }
    }
}
