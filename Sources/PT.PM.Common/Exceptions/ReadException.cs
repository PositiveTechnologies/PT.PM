using System;

namespace PT.PM.Common.Exceptions
{
    public class ReadException : PMException
    {
        public ReadException(SourceCodeFile sourceCodeFile)
        {
            SourceCodeFile = sourceCodeFile;
        }

        public ReadException(SourceCodeFile sourceCodeFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            SourceCodeFile = sourceCodeFile;
        }

        public override string ToString()
        {
            return $"File {SourceCodeFile.RelativeName} not found or can not be read.";
        }
    }
}
