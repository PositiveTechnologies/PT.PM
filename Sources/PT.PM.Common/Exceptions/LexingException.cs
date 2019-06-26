using System;
using System.Runtime.Serialization;
using PT.PM.Common.Files;

namespace PT.PM.Common.Exceptions
{
    public class LexingException : PMException
    {
        public LexingException()
        {
        }

        public LexingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public LexingException(IFile sourceFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            File = sourceFile ?? TextFile.Empty;
        }
    }
}
