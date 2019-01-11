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

        [Test]
        public void Serialize_MsgPack_DamagedFileCatchErrors()
        {
            CheckMsgPackSerialization("empty-try-catch.php", damaged: true);
        }

        private static void CheckMsgPackSerialization(string inputFileName, bool linearTextSpans = false,
            bool damaged = false)
        {
            string path = Path.Combine(TestUtility.TestsDataPath, inputFileName);
            string ext = SerializationFormat.MsgPack.GetExtension();

            // Serialization
            string[] files = File.Exists(path)
                ? new[] {path}
                : Directory.GetFiles(path);
            var codeRepository = new FileCodeRepository(files);

            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(codeRepository)
            {
                DumpStages = new HashSet<Stage> {Stage.Ust},
                DumpDir = TestUtility.TestsOutputPath,
                SerializationFormat = SerializationFormat.MsgPack,
                LineColumnTextSpans = !linearTextSpans,
                Stage = Stage.Ust,
                Logger = logger
            };
            WorkflowResult result = workflow.Process();

            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);

            int errorOffset = 106;
            byte errorValue = 123;

            var serializedFiles = new List<string>();
            foreach (string file in files)
            {
                string shortFileName = Path.GetFileName(file) + ".ust." + ext;
                string serializedFile = Path.Combine(TestUtility.TestsOutputPath, shortFileName);

                if (damaged)
                {
                    byte[] bytes = File.ReadAllBytes(serializedFile);
                    bytes[errorOffset] = errorValue;
                    File.WriteAllBytes(serializedFile, bytes);
                }

                serializedFiles.Add(serializedFile);
            }

            // Deserialization
            var newLogger = new LoggerMessageCounter();
            var newCodeRepository = new FileCodeRepository(serializedFiles, format: SerializationFormat.MsgPack);

            var newWorkflow = new Workflow(newCodeRepository, new DefaultPatternRepository())
            {
                StartStage = Stage.Ust,
                SerializationFormat = SerializationFormat.MsgPack,
                LineColumnTextSpans = !linearTextSpans,
                Logger = newLogger
            };
            newWorkflow.Process();

            if (damaged)
            {
                Assert.AreEqual(1, newLogger.ErrorCount);
                Assert.IsTrue(newLogger.ErrorsString.Contains(errorValue.ToString()));
                Assert.IsTrue(newLogger.ErrorsString.Contains(errorOffset.ToString()));
                return;
            }

            Assert.AreEqual(0, newLogger.ErrorCount, newLogger.ErrorsString);
            Assert.GreaterOrEqual(newLogger.Matches.Count, 1);

            var match = (MatchResult) newLogger.Matches[0];
            using (var sourceFilesEnumerator = result.SourceFiles.GetEnumerator())
            {
                sourceFilesEnumerator.MoveNext();
                var firstFile = (TextFile) sourceFilesEnumerator.Current;
                Assert.AreEqual(new LineColumnTextSpan(2, 1, 3, 25), firstFile.GetLineColumnTextSpan(match.TextSpan));
            }
        }
    }
}
