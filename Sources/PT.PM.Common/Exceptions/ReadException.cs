using System;

namespace PT.PM.Common.Exceptions
{
    public class ReadException : PMException
    {
        public ReadException(CodeFile codeFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            CodeFile = codeFile;
        }

        public override string ToString()
        {
            return $"File {CodeFile.RelativeName} not found or can not be read.";
        }
    }
}
