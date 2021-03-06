﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Common.Files;
using PT.PM.Common.MessagePack;
using PT.PM.Matching;
using PT.PM.Patterns.PatternsRepository;
using PT.PM.TestUtils;

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

        [Test]
        public void Serialize_MsgPack_IncorrectFilePathCatchErrors()
        {
            CheckMsgPackSerialization("empty-try-catch.php", incorrectFilePath: true);
        }

        private static void CheckMsgPackSerialization(string inputFileName, bool linearTextSpans = false,
            bool damaged = false, bool incorrectFilePath = false)
        {
            string path = Path.Combine(TestUtility.TestsDataPath, inputFileName);

            // Serialization
            string[] files = File.Exists(path)
                ? new[] {path}
                : Directory.GetFiles(path);
            var codeRepository = new FileSourceRepository(files);

            var logger = new TestLogger();
            var workflow = new Workflow(codeRepository)
            {
                DumpStages = new HashSet<Stage> {Stage.Ust},
                DumpDir = TestUtility.TestsOutputPath,
                SerializationFormat = SerializationFormat.MsgPack,
                LineColumnTextSpans = !linearTextSpans,
                Stage = Stage.Ust,
                Logger = logger
            };

            IFile sourceFile = null;
            workflow.FileRead += (sender, file) => sourceFile = file;
            workflow.Process();

            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);

            int errorOffset = 6;
            byte errorValue = 123;

            var serializedFiles = new List<string>();
            foreach (string serializedFile in logger.GetSerializedFileNames())
            {
                if (damaged || incorrectFilePath)
                {
                    byte[] bytes = File.ReadAllBytes(serializedFile);
                    if (damaged)
                    {
                        errorOffset += path.Length + 1;
                    }
                    else
                    {
                        errorOffset += 2;
                    }

                    bytes[errorOffset] = errorValue;
                    File.WriteAllBytes(serializedFile, bytes);
                }

                serializedFiles.Add(serializedFile);
            }

            // Deserialization
            var newLogger = new TestLogger();
            var newCodeRepository = new FileSourceRepository(serializedFiles);

            var newWorkflow = new Workflow(newCodeRepository, new DefaultPatternRepository())
            {
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

            var binaryFile = (BinaryFile) newCodeRepository.ReadFile(newCodeRepository.GetFileNames().ElementAt(0));
            RootUstMessagePackSerializer.Deserialize(binaryFile, new HashSet<IFile>(), null, logger, out int readSize);
            Assert.AreEqual(binaryFile.Data.Length, readSize);

            Assert.GreaterOrEqual(newLogger.Matches.Count, 1);

            if (incorrectFilePath)
            {
                string firstError = newLogger.Errors[0];
                Assert.IsTrue(firstError.Contains("ReadException"));
                Assert.IsTrue(firstError.Contains("Error during opening the file"));
                return;
            }

            Assert.AreEqual(0, newLogger.ErrorCount, newLogger.ErrorsString);

            var match = (MatchResult) newLogger.Matches[0];

            Assert.AreEqual(new LineColumnTextSpan(2, 1, 3, 25),
                ((TextFile) sourceFile).GetLineColumnTextSpan(match.TextSpan));
        }
    }
}
