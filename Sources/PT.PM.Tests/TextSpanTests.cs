using PT.PM.Common;
using NUnit.Framework;

namespace PT.PM.Tests
{
    [TestFixture]
    public class TextSpanTests
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
    }
}
