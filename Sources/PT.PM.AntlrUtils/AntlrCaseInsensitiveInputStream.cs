using Antlr4.Runtime;

namespace PT.PM.AntlrUtils
{
    public class AntlrCaseInsensitiveInputStream : AntlrInputStream
    {
        private string lookaheadData;

        public AntlrCaseInsensitiveInputStream(string input)
            : base(input)
        {
            lookaheadData = input.ToLower();
        }

        public override int La(int i)
        {
            if (i == 0)
            {
                return 0; // undefined
            }
            if (i < 0)
            {
                i++; // e.g., translate LA(-1) to use offset i=0; then data[p+0-1]
                if (p + i - 1 < 0)
                {
                    return IntStreamConstants.Eof; // invalid; no char before first char
                }
            }

            if (p + i - 1 >= n)
            {
                return IntStreamConstants.Eof;
            }

            return lookaheadData[p + i - 1];
        }
    }
}
