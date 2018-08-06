using System;

namespace PT.PM.Common.Exceptions
{
    public class MatchingException : PMException
    {
        public MatchingException()
        {
        }

        public MatchingException(CodeFile codeFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            CodeFile = codeFile ?? CodeFile.Empty;
        }
    }
}
