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
            string origin = "The sample of very long message.";
            string actual = origin.Trunc(32);
            Assert.AreEqual(origin, actual);

            actual = origin.Trunc(30, truncMessageCutWords: true);
            Assert.AreEqual(30, actual.Length);
            Assert.AreEqual("The sample of v ... g message.", actual);

            actual = origin.Trunc(30, truncMessageCutWords: false);
            Assert.AreEqual("The sample of ... message.", actual);
        }
    }
}
