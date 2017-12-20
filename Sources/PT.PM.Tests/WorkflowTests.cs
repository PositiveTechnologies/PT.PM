using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching;
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
        public void Process_JsonUst_InvariantCase()
        {
            CheckSerialization(upperCase: true);
        }

        [Test]
        public void Process_JsonUst_NotIndented()
        {
            CheckSerialization(indented: false);
        }

        [Test]
        public void Process_JsonUst_WithoutTextSpan()
        {
            CheckSerialization(includeTextSpans: false);
        }

        [Test]
        public void Process_JsonUst_LineColumnTextSpan()
        {
            CheckSerialization(lineColumnTextSpans: true);
        }

        private static void CheckSerialization(bool lineColumnTextSpans = false,
            bool includeTextSpans = true, bool indented = true, bool upperCase = false)
        {
            // Serialization
            string inputFileName = "empty-try-catch.php";
            var codeRepository = new MemoryCodeRepository("<?php try { echo 1/0; } \r\n catch (Exception $e) { } ?>", inputFileName);
            var workflow = new Workflow(codeRepository)
            {
                DumpStages = new HashSet<Stage>() { Stage.Ust },
                DumpDir = TestUtility.TestsOutputPath,
                IncludeCodeInDump = true,
                IndentedDump = indented,
                DumpWithTextSpans = includeTextSpans,
                LineColumnTextSpans = lineColumnTextSpans
            };
            WorkflowResult result = workflow.Process();

            // Convert case to upper for checking correct deserialization
            var code = File.ReadAllText(Path.Combine(TestUtility.TestsOutputPath, inputFileName + ".ust.json"));
            if (upperCase)
            {
                code = code.ToUpperInvariant().Replace("\\R", "\\r").Replace("\\N", "\\n");
            }

            // Deserialization
            SourceCodeRepository sourceCodeRepository = new MemoryCodeRepository(code, inputFileName + ".ust.json")
            {
                LoadJson = true
            };
            var newWorkflow = new Workflow(sourceCodeRepository)
            {
                StartStage = Stage.Ust,
                IsAsyncPatternsConversion = false,
                IndentedDump = indented,
                DumpWithTextSpans = includeTextSpans,
                LineColumnTextSpans = lineColumnTextSpans
            };
            var newResult = newWorkflow.Process();

            Assert.GreaterOrEqual(newResult.MatchResults.Count, 1);
            MatchResult match = newResult.MatchResults.FirstOrDefault();
            if (includeTextSpans)
            {
                Assert.AreEqual(TextSpan.FromBounds(6, 51), match.TextSpan);
            }
            else
            {
                Assert.IsNotNull(match);
            }
        }
    }
}
