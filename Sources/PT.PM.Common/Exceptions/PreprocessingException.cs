using System;

namespace PT.PM.Common.Exceptions
{
    public class PreprocessingException : PMException
    {
        public PreprocessingException(CodeFile codeFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            CodeFile = codeFile ?? CodeFile.Empty;
        }
    }
}
