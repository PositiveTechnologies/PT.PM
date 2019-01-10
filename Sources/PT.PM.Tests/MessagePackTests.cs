using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Utils;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;
using PT.PM.Patterns.PatternsRepository;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using PT.PM.Common.Files;

namespace PT.PM.Tests
{
    [TestFixture]
    public class MessagePackTests
    {
        [Test]
        public void Serialize_MsgPack_Common()
        {
            CheckMsgPackSerialization("empty-try-catch.php");
        }

        [Test]
        public void Serialize_MsgPack_LinearTextSpan()
        {
            CheckMsgPackSerialization("empty-try-catch.php", true);
        }

        private static void CheckMsgPackSerialization(string inputFileName, bool linearTextSpans = false,
            bool checkPatternSerialization = false)
        {
            string path = Path.Combine(TestUtility.TestsDataPath, inputFileName);
            string ext = SerializationFormat.MsgPack.GetExtension();

            // Serialization
            string[] files = File.Exists(path)
                           ? new [] { path }
                           : Directory.GetFiles(path);
            var codeRepository = new FileCodeRepository(files);

            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(codeRepository)
            {
                DumpStages = new HashSet<Stage>() { Stage.Ust },
                DumpDir = TestUtility.TestsOutputPath,
                SerializationFormat = SerializationFormat.MsgPack,
                LineColumnTextSpans = !linearTextSpans,
                Stage = Stage.Ust,
                IsDumpPatterns = checkPatternSerialization,
                Logger = logger
            };
            WorkflowResult result = workflow.Process();

            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);

            var serializedFiles = new List<string>();
            foreach (string file in files)
            {
                string shortFileName = Path.GetFileName(file) + ".ust." + ext;
                string jsonFile = Path.Combine(TestUtility.TestsOutputPath, shortFileName);
                serializedFiles.Add(jsonFile);
            }

            // Deserialization
            var newLogger = new LoggerMessageCounter();
            var newCodeRepository = new FileCodeRepository(serializedFiles, format: SerializationFormat.MsgPack);

            var newPatternsRepository = checkPatternSerialization
                ? new JsonPatternsRepository(File.ReadAllText(Path.Combine(TestUtility.TestsOutputPath, "patterns." + ext)))
                : inputFileName == "MultiTextSpan"
                ? (IPatternsRepository)new DslPatternRepository("a", "php")
                : new DefaultPatternRepository();
            var newWorkflow = new Workflow(newCodeRepository, newPatternsRepository)
            {
                StartStage = Stage.Ust,
                SerializationFormat = SerializationFormat.MsgPack,
                LineColumnTextSpans = !linearTextSpans,
                Logger = newLogger
            };
            newWorkflow.Process();

            Assert.AreEqual(0, newLogger.ErrorCount, newLogger.ErrorsString);
            Assert.GreaterOrEqual(newLogger.Matches.Count, 1);

            if (inputFileName == "MultiTextSpan")
            {
                /*var matchResult = (MatchResult)logger.Matches[1];
                Assert.AreEqual(2, matchResult.TextSpans.Length);

                LineColumnTextSpan actualOriginTextSpan = originFile.GetLineColumnTextSpan(matchResult.TextSpans[1]);
                Assert.AreEqual(lcOriginTextSpan, actualOriginTextSpan);

                LineColumnTextSpan actualPreprocessedTextSpan = preprocessedFile.GetLineColumnTextSpan(matchResult.TextSpans[0]);
                Assert.AreEqual(lcPreprocessedTextSpan, actualPreprocessedTextSpan);*/
            }
            else
            {
                var match = (MatchResult)newLogger.Matches[0];
                using (var sourceCodeFilesEnumerator = result.SourceCodeFiles.GetEnumerator())
                {
                    sourceCodeFilesEnumerator.MoveNext();
                    var firstFile = (CodeFile) sourceCodeFilesEnumerator.Current;
                    Assert.AreEqual(new LineColumnTextSpan(2, 1, 3, 25), firstFile.GetLineColumnTextSpan(match.TextSpan));
                }
            }
        }
    }
}
