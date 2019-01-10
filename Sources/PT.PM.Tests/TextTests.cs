using PT.PM.Common;
using NUnit.Framework;
using PT.PM.Common.Files;

namespace PT.PM.Tests
{
    [TestFixture]
    public class TextTests
    {
        [Test]
        public void ConvertPosition_LineColumn_CorrectLinear()
        {
            string text = "row1\r\n" +
                          "row2\n" +
                          "row3\r" +
                          "row4";

            var sourceCodeFile = new CodeFile(text);
            int linearPos = sourceCodeFile.GetLinearFromLineColumn(4, 4);
            Assert.AreEqual('4', text[linearPos]);

            sourceCodeFile.GetLineColumnFromLinear(linearPos, out int line, out int column);
            Assert.AreEqual(4, line);
            Assert.AreEqual(4, column);

            Assert.AreEqual(4, sourceCodeFile.GetLinesCount());
        }

        [Test]
        public void GetStringAtLine_CorrectString()
        {
            string text = "row1\r\n" +
                          "\n" +
                          "row3\r" +
                          "";

            var textFile = new CodeFile(text);
            Assert.AreEqual("row1", textFile.GetStringAtLine(1));
            Assert.AreEqual("", textFile.GetStringAtLine(2));
            Assert.AreEqual("row3", textFile.GetStringAtLine(3));
            Assert.AreEqual("", textFile.GetStringAtLine(4));

            Assert.AreEqual("row1", new CodeFile("row1").GetStringAtLine(1));
        }

        [Test]
        public void PrettyPrintMessages()
        {
            var printer = new PrettyPrinter() { MaxMessageLength = 32 };
            string origin = "The sample of very long message.";
            string actual = printer.Print(origin);
            Assert.AreEqual(origin, actual);

            printer = new PrettyPrinter
            {
                MaxMessageLength = 30,
                CutWords = true
            };
            actual = printer.Print(origin);
            Assert.AreEqual(30, actual.Length);
            Assert.AreEqual("The sample of v ... g message.", actual);

            printer = new PrettyPrinter
            {
                MaxMessageLength = 30,
                CutWords = false
            };
            actual = printer.Print(origin);
            Assert.AreEqual("The sample of ... message.", actual);

            printer = new PrettyPrinter
            {
                Trim = false,
                MaxMessageLength = 10,
                CutWords = false
            };
            Assert.AreEqual(" ... ", printer.Print("                          "));

            origin = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            printer = new PrettyPrinter
            {
                MaxMessageLength = 20,
                CutWords = false
            };
            Assert.AreEqual("aaaaaaaaaa ... aaaaa", printer.Print(origin));

            origin = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            printer = new PrettyPrinter
            {
                MaxMessageLength = 20,
                StartRatio = 1.0,
                CutWords = false
            };
            Assert.AreEqual("aaaaaaaaaaaaaaaaaaaa ... ", printer.Print(origin));
        }

        [Test]
        public void TrimIndents()
        {
            string origin =
@"try {
			$LogHandler->writeToLog(""message"");
		} catch (Exception $e) {
			// do nothing
		}";

            var printer = new PrettyPrinter() { TrimIndent = true };
            string actual = printer.Print(origin);

            string expected =
@"try {
	$LogHandler->writeToLog(""message"");
} catch (Exception $e) {
	// do nothing
}";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReduceWhitespaces()
        {
            string origin = " a   =   b ";
            var printer = new PrettyPrinter
            {
                Trim = true,
                ReduceWhitespaces = true
            };
            Assert.AreEqual("a = b", printer.Print(origin));
        }

        [Test]
        public void EscapeString()
        {
            string origin = "\"\\\r\n";
            var printer = new PrettyPrinter
            {
                Escape = true
            };
            Assert.AreEqual("\\\"\\\\", printer.Print(origin));
        }
    }
}
