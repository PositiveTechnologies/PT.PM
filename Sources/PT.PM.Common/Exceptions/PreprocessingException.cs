using System;

namespace PT.PM.Common.Exceptions
{
    public class PreprocessingException : PMException
    {
        public PreprocessingException(SourceCodeFile sourceCodeFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            SourceCodeFile = sourceCodeFile ?? SourceCodeFile.Empty;
        }
    }
}
