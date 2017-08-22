using PT.PM.Common;
using NUnit.Framework;

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

            int linearPos = TextHelper.LineColumnToLinear(text, 4, 4);
            Assert.AreEqual('4', text[linearPos]);

            int line, column;
            TextHelper.LinearToLineColumn(linearPos, text, out line, out column);
            Assert.AreEqual(4, line);
            Assert.AreEqual(4, column);
        }

        [Test]
        public void TruncMessages()
        {
            var truncater = new TextTruncater() { MaxMessageLength = 32 };
            string origin = "The sample of very long message.";
            string actual = truncater.Trunc(origin);
            Assert.AreEqual(origin, actual);

            truncater = new TextTruncater() { MaxMessageLength = 30, CutWords = true };
            actual = truncater.Trunc(origin);
            Assert.AreEqual(30, actual.Length);
            Assert.AreEqual("The sample of v ... g message.", actual);

            truncater = new TextTruncater() { MaxMessageLength = 30, CutWords = false };
            actual = truncater.Trunc(origin);
            Assert.AreEqual("The sample of ... message.", actual);

            truncater = new TextTruncater() { MaxMessageLength = 10, CutWords = false };
            Assert.AreEqual(" ... ", truncater.Trunc("                          "));

            origin = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            truncater = new TextTruncater() { MaxMessageLength = 20, CutWords = false };
            Assert.AreEqual("aaaaaaaaaa ... aaaaa", truncater.Trunc(origin));
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

            var truncater = new TextTruncater() { TrimIndent = true };
            string actual = truncater.Trunc(origin);

            string expected =
@"try {
	$LogHandler->writeToLog(""message"");
} catch (Exception $e) {
	// do nothing
}";

            Assert.AreEqual(expected, actual);
        }
    }
}
