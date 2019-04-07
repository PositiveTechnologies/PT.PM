using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Common.Files;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;
using PT.PM.Patterns.PatternsRepository;
using PT.PM.TestUtils;

namespace PT.PM.Tests
{
    [TestFixture]
    public class JsonTests
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
        public void Process_JsonUst_WithCode()
        {
            CheckJsonSerialization("empty-try-catch.php", includeCode: true);
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

        [Test]
        public void Process_JsonUst_ExcessPropertyInJson()
        {
            CheckJsonSerialization("empty-try-catch.php", checkStrict: true, strict: true);
        }

        [Test]
        public void Process_JsonUst_ExcessPropertyInJsonNotStrict()
        {
            CheckJsonSerialization("empty-try-catch.php", checkStrict: true, strict: false);
        }

        [Test]
        public void WriteReadPatternsToJson()
        {
            CheckJsonSerialization("empty-try-catch.php", checkPatternSerialization: true);
        }

        private static void CheckJsonSerialization(string inputFileName, bool linearTextSpans = false,
            bool includeTextSpans = true, bool indented = true, bool includeCode = false,
            bool checkStrict = false, bool strict = true, bool checkPatternSerialization = false)
        {
            string path = Path.Combine(TestUtility.TestsDataPath, inputFileName);

            // Serialization
            string[] files = File.Exists(path)
                           ? new [] { path }
                           : Directory.GetFiles(path);
            var codeRepository = new FileSourceRepository(files);

            var logger = new TestLogger();
            var workflow = new Workflow(codeRepository)
            {
                DumpStages = new HashSet<Stage> { Stage.Ust },
                DumpDir = TestUtility.TestsOutputPath,
                IncludeCodeInDump = includeCode,
                IndentedDump = indented,
                StrictJson = strict,
                DumpWithTextSpans = includeTextSpans,
                LineColumnTextSpans = !linearTextSpans,
                Stage = Stage.Ust,
                IsDumpPatterns = checkPatternSerialization
            };
            WorkflowResult result = workflow.Process();

            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);

            var preprocessedFile = (TextFile)result.SourceFiles.FirstOrDefault(f => f.Name == "preprocessed.php");
            var originFile = (TextFile)result.SourceFiles.FirstOrDefault(f => f.Name == "origin.php");

            LineColumnTextSpan lcPreprocessedTextSpan = new LineColumnTextSpan(4, 1, 4, 3);
            LineColumnTextSpan lcOriginTextSpan = new LineColumnTextSpan(3, 1, 3, 3, originFile);

            var jsonFiles = new List<string>();
            foreach (string jsonFile in logger.GetSerializedFileNames())
            {
                if (jsonFile.Contains("preprocessed.php") || checkStrict)
                {
                    string json = File.ReadAllText(jsonFile);
                    if (jsonFile.Contains("preprocessed.php"))
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

                    if (checkStrict)
                    {
                        json = json.Replace("\"Kind\": \"IntLiteral\"", "\"Kind\": \"IntLiteral\", \"ExcessProperty\": \"value\"");
                    }

                    File.WriteAllText(jsonFile, json);
                }

                jsonFiles.Add(jsonFile);
            }

            // Deserialization
            var newLogger = new TestLogger();
            var newCodeRepository = new FileSourceRepository(jsonFiles);

            var newPatternsRepository = checkPatternSerialization
                ? new JsonPatternsRepository(File.ReadAllText(Path.Combine(TestUtility.TestsOutputPath, "patterns.json")))
                : inputFileName == "MultiTextSpan"
                ? (IPatternsRepository)new DslPatternRepository("a", "php")
                : new DefaultPatternRepository();
            var newWorkflow = new Workflow(newCodeRepository, newPatternsRepository)
            {
                StrictJson = strict,
                Logger = newLogger
            };
            newWorkflow.Process();

            if (checkStrict && strict)
            {
                Assert.IsTrue(newLogger.ErrorsString.Contains("ExcessProperty"));
            }
            else
            {
                Assert.AreEqual(0, newLogger.ErrorCount, newLogger.ErrorsString);
                Assert.GreaterOrEqual(newLogger.Matches.Count, 1);

                if (includeTextSpans)
                {
                    if (inputFileName == "MultiTextSpan")
                    {
                        var matchResult = (MatchResult)newLogger.Matches[1];
                        Assert.AreEqual(2, matchResult.TextSpans.Length);

                        LineColumnTextSpan actualOriginTextSpan = originFile.GetLineColumnTextSpan(matchResult.TextSpans[1]);
                        Assert.AreEqual(lcOriginTextSpan, actualOriginTextSpan);

                        LineColumnTextSpan actualPreprocessedTextSpan = preprocessedFile.GetLineColumnTextSpan(matchResult.TextSpans[0]);
                        Assert.AreEqual(lcPreprocessedTextSpan, actualPreprocessedTextSpan);
                    }
                    else
                    {
                        var match = (MatchResult)newLogger.Matches[0];
                        var enumerator = result.SourceFiles.GetEnumerator();
                        enumerator.MoveNext();
                        var firstFile = (TextFile)enumerator.Current;
                        Assert.AreEqual(new LineColumnTextSpan(2, 1, 3, 25), firstFile.GetLineColumnTextSpan(match.TextSpan));
                    }
                }
            }
        }
    }
}
