using System;

namespace PT.PM.Common.Exceptions
{
    public class ReadException : PMException
    {
        public override PMExceptionType ExceptionType => PMExceptionType.Read;

        public ReadException(string fileName)
        {
            FileName = fileName;
        }

        public ReadException(string fileName, Exception ex = null, string message = "")
            : base(ex, message)
        {
            FileName = fileName;
        }

        public override string ToString()
        {
            return $"File {FileName} not found or can not be read.";
        }
    }
}
