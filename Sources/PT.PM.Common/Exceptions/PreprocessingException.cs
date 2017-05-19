using System;

namespace PT.PM.Common.Exceptions
{
    public class PreprocessingException : PMException
    {
        public override PMExceptionType ExceptionType => PMExceptionType.Preprocessing;

        public PreprocessingException(string fileName, Exception ex = null, string message = "")
            : base(ex, message)
        {
            FileName = fileName;
        }
    }
}
