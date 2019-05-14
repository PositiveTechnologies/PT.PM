using Antlr4.Runtime;

namespace PT.PM.AntlrUtils
{
    public class LightTokenSource : ITokenSource
    {
        public LightTokenSource(ICharStream inputStream)
        {
            InputStream = inputStream;
        }

        public IToken NextToken() => throw new System.NotImplementedException();

        public int Line { get; }

        public int Column { get; }

        public ICharStream InputStream { get; }

        public string SourceName { get; }

        public ITokenFactory TokenFactory { get; set; }
    }
}