using System;
using Antlr4.Runtime;

namespace PT.PM.AntlrUtils
{
    public class LightTokenFactory : ITokenFactory
    {
        private int index;

        public IToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine)
        {
            return new LightToken((LightInputStream)source.Item2, type, channel, index++, start, stop + 1);
        }

        public IToken Create(int type, string text)
        {
            throw new NotImplementedException();
        }
    }
}