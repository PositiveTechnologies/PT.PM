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
            // Serialization
            string inputFileName = "empty-try-catch.php";
            var codeRepository = new MemoryCodeRepository("<?php try { echo 1/0; } catch (Exception $e) { } ?>", inputFileName);
            var workflow = new Workflow(codeRepository)
            {
                DumpStages = new HashSet<Stage>() { Stage.Ust },
                DumpDir = TestUtility.TestsOutputPath,
                IncludeCodeInDump = true
            };
            WorkflowResult result = workflow.Process();

            // Convert case to upper for checking correct deserialization
            var code = File.ReadAllText(Path.Combine(TestUtility.TestsOutputPath, inputFileName + ".ust.json"));
            code = code.ToUpperInvariant().Replace("\\R", "\\r").Replace("\\N", "\\n");

            // Deserialization
            SourceCodeRepository sourceCodeRepository = new MemoryCodeRepository(code, inputFileName + ".ust.json")
            {
                LoadJson = true
            };
            var newWorkflow = new Workflow(sourceCodeRepository)
            {
                StartStage = Stage.Ust,
                IsAsyncPatternsConversion = false
            };
            var newResult = newWorkflow.Process();

            Assert.GreaterOrEqual(newResult.MatchResults.Count, 1);
            MatchResult match = newResult.MatchResults.First();
            Assert.IsFalse(match.TextSpan.IsEmpty);
        }
    }
}
