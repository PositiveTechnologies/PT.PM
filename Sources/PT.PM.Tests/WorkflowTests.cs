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
        public void Process_JsonUst_NotIndented()
        {
            CheckJsonSerialization("empty-try-catch.php", indented: false);
        }

        [Test]
        public void Process_JsonUst_WithoutTextSpan()
        {
            CheckJsonSerialization("empty-try-catch.php", includeTextSpans: false);
        }

        [Test]
        public void Process_JsonUst_LinearTextSpan()
        {
            CheckJsonSerialization("empty-try-catch.php", linearTextSpans: true);
        }

        [Test]
        public void Process_JsonUst_WithoutCode()
        {
            CheckJsonSerialization("empty-try-catch.php", includeCode: false);
        }

        [Test]
        public void Process_JsonUst_MultiTextSpan()
        {
            CheckJsonSerialization("MultiTextSpan");
        }

        [Test]
        public void Process_JsonUst_MultiTextSpanLineColumn()
        {
            CheckJsonSerialization("MultiTextSpan", linearTextSpans: true);
        }

        private static void CheckJsonSerialization(string inputFileName, bool linearTextSpans = false,
            bool includeTextSpans = true, bool indented = true, bool includeCode = true)
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
                LinearTextSpans = linearTextSpans,
                Stage = Stage.SimplifiedUst
            };
            WorkflowResult result = workflow.Process();

            CodeFile preprocessedFile = result.SourceCodeFiles.FirstOrDefault(f => f.Name == "preprocessed.php");
            CodeFile originFile = result.SourceCodeFiles.FirstOrDefault(f => f.Name == "origin.php");

            LineColumnTextSpan lcPreprocessedTextSpan = new LineColumnTextSpan(4, 1, 4, 3);
            LineColumnTextSpan lcOriginTextSpan = new LineColumnTextSpan(3, 1, 3, 3, originFile);

            var jsonFiles = new List<string>();
            foreach (string file in files)
            {
                string shortFileName = Path.GetFileName(file) + ".ust.json";
                string jsonFile = Path.Combine(TestUtility.TestsOutputPath, shortFileName);
                string json = File.ReadAllText(jsonFile);

                if (file.Contains("preprocessed.php"))
                {
                    string preprocessedTextSpanString, originTextSpanString;

                    if (linearTextSpans)
                    {
                        preprocessedTextSpanString = preprocessedFile.GetTextSpan(lcPreprocessedTextSpan).ToString();
                        originTextSpanString = originFile.GetTextSpan(lcOriginTextSpan).ToString();
                    }
                    else
                    {
                        preprocessedTextSpanString = lcPreprocessedTextSpan.ToString();
                        originTextSpanString = lcOriginTextSpan.ToString();
                    }

                    json = json.Replace($"\"{preprocessedTextSpanString}\"", $"[ \"{lcPreprocessedTextSpan}\", \"{originTextSpanString}\" ]");
                }

                File.WriteAllText(jsonFile, json);
                jsonFiles.Add(jsonFile);
            }

            // Deserialization
            var logger = new LoggerMessageCounter();
            var newCodeRepository = new FileCodeRepository(jsonFiles) { LoadJson = true };

            var newWorkflow = new Workflow(newCodeRepository,
                inputFileName == "MultiTextSpan" ? new DslPatternRepository("a", "php") : null)
            {
                StartStage = Stage.Ust,
                IndentedDump = indented,
                DumpWithTextSpans = includeTextSpans,
                LinearTextSpans = linearTextSpans,
                Logger = logger
            };
            WorkflowResult newResult = newWorkflow.Process();

            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);
            Assert.GreaterOrEqual(newResult.MatchResults.Count, 1);

            if (includeTextSpans)
            {
                if (inputFileName == "MultiTextSpan")
                {
                    var matchResult = (MatchResult)newResult.MatchResults[1];
                    Assert.AreEqual(2, matchResult.TextSpans.Length);

                    LineColumnTextSpan actualOriginTextSpan = originFile.GetLineColumnTextSpan(matchResult.TextSpans[1]);
                    Assert.AreEqual(lcOriginTextSpan, actualOriginTextSpan);

                    LineColumnTextSpan actualPreprocessedTextSpan = preprocessedFile.GetLineColumnTextSpan(matchResult.TextSpans[0]);
                    Assert.AreEqual(lcPreprocessedTextSpan, actualPreprocessedTextSpan);
                }
                else
                {
                    var match = (MatchResult)newResult.MatchResults.FirstOrDefault();
                    Assert.AreEqual(new LineColumnTextSpan(2, 1, 3, 25), result.SourceCodeFiles.First().GetLineColumnTextSpan(match.TextSpan));
                }
            }
        }
    }
}
