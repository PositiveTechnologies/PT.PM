using System;

namespace PT.PM.Common.Exceptions
{
    public class MatchingException : PMException
    {
        public TextSpan TextSpan { get; set; }

        public MatchingException()
        {
        }

        public MatchingException(SourceCodeFile sourceCodeFile, Exception ex = null, string message = "", bool isPattern = false)
            : base(ex, message, isPattern)
        {
            SourceCodeFile = sourceCodeFile ?? SourceCodeFile.Empty;
        }
    }
}
