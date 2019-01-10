using System;
using PT.PM.Common.Files;

namespace PT.PM.Common.Exceptions
{
    public class ReadException : PMException
    {
        public ReadException(IFile file, Exception ex = null, string message = "")
            : base(ex, message)
        {
            File = file;
        }

        public override string ToString()
        {
            return $"File {File.RelativeName} not found or can not be read.";
        }
    }
}
