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
            CheckSerialization("empty-try-catch.php", upperCase: true);
        }

        [Test]
        public void Process_JsonUst_NotIndented()
        {
            CheckSerialization("empty-try-catch.php", indented: false);
        }

        [Test]
        public void Process_JsonUst_WithoutTextSpan()
        {
            CheckSerialization("empty-try-catch.php", includeTextSpans: false);
        }

        [Test]
        public void Process_JsonUst_LineColumnTextSpan()
        {
            CheckSerialization("empty-try-catch.php", lineColumnTextSpans: true);
        }

        [Test]
        public void Process_JsonUst_WithoutCode()
        {
            CheckSerialization("empty-try-catch.php", includeCode: false);
        }

        [Test]
        public void Process_JsonUst_MultiTextSpan()
        {
            CheckSerialization("MultiTextSpan");
        }

        [Test]
        public void Process_JsonUst_MultiTextSpanLineColumn()
        {
            CheckSerialization("MultiTextSpan", lineColumnTextSpans: true);
        }

        private static void CheckSerialization(string inputFileName, bool lineColumnTextSpans = false,
            bool includeTextSpans = true, bool indented = true, bool upperCase = false, bool includeCode = true)
        {
            string path = Path.Combine(TestUtility.TestsDataPath, inputFileName);

            // Serialization
            string[] files = File.Exists(path)
                           ? new string[] { path }
                           : Directory.GetFiles(path);
            var codeRepository = new FileCodeRepository(files);

            var workflow = new Workflow(codeRepository)
            {
                DumpStages = new HashSet<Stage>() { Stage.Ust },
                DumpDir = TestUtility.TestsOutputPath,
                IncludeCodeInDump = includeCode,
                IndentedDump = indented,
                DumpWithTextSpans = includeTextSpans,
                LineColumnTextSpans = lineColumnTextSpans,
                Stage = Stage.SimplifiedUst
            };
            WorkflowResult result = workflow.Process();

            // Convert case to upper for checking correct deserialization
            var codes = new Dictionary<string, string>();
            foreach (string file in files)
            {
                string shortFileName = Path.GetFileName(file) + ".ust.json";
                string code = File.ReadAllText(
                    Path.Combine(TestUtility.TestsOutputPath, shortFileName));

                // Convert case to upper for checking correct deserialization
                if (upperCase)
                {
                    code = code.ToUpperInvariant().Replace("\\R", "\\r").Replace("\\N", "\\n")
                        .Replace("TRUE", "true").Replace("FALSE", "false");
                }

                if (file.Contains("preprocessed.php"))
                {
                    if (!lineColumnTextSpans)
                    {
                        string preprocessedTextSpan = "\"[26..28)\"";
                        code = code.Replace(preprocessedTextSpan, $"[ {preprocessedTextSpan}, \"[9..11); origin.php\" ]");
                    }
                    else
                    {
                        string preprocessedTextSpan = "\"[4,1]-[4,3)\"";
                        code = code.Replace(preprocessedTextSpan, $"[ {preprocessedTextSpan}, \"[3,1]-[3,3); origin.php\" ]");
                    }
                }

                codes.Add(shortFileName, code);
            }

            // Deserialization
            var logger = new LoggerMessageCounter();
            SourceCodeRepository sourceCodeRepository = new MemoryCodeRepository(codes)
            {
                LoadJson = true
            };

            var newWorkflow = new Workflow(sourceCodeRepository,
                inputFileName == "MultiTextSpan" ? new DslPatternRepository("a", "php") : null)
            {
                StartStage = Stage.Ust,
                IndentedDump = indented,
                DumpWithTextSpans = includeTextSpans,
                LineColumnTextSpans = lineColumnTextSpans,
                Logger = logger
            };
            WorkflowResult newResult = newWorkflow.Process();

            Assert.AreEqual(0, logger.ErrorCount);
            Assert.GreaterOrEqual(newResult.MatchResults.Count, 1);

            if (includeTextSpans)
            {
                if (inputFileName == "MultiTextSpan")
                {
                    var match = (MatchResult)newResult.MatchResults[1];
                    Assert.AreEqual(2, match.TextSpans.Length);
                    Assert.AreEqual(TextSpan.FromBounds(9, 11, "origin.php"), match.TextSpans[1]);
                }
                else
                {
                    var match = (MatchResult)newResult.MatchResults.FirstOrDefault();
                    Assert.AreEqual(TextSpan.FromBounds(7, 50), match.TextSpan);
                }
            }
        }
    }
}
