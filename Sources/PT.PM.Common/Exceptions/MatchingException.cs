using System;
using PT.PM.Common.Files;

namespace PT.PM.Common.Exceptions
{
    public class MatchingException : PMException
    {
        public MatchingException()
        {
        }

        public MatchingException(IFile sourceFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            File = sourceFile ?? TextFile.Empty;
        }
    }
}
