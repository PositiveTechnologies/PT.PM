using System;
using System.Runtime.Serialization;
using PT.PM.Common.Files;

namespace PT.PM.Common.Exceptions
{
    public class ParsingException : PMException
    {
        public ParsingException()
        {
        }

        public ParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ParsingException(IFile sourceFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            File = sourceFile ?? TextFile.Empty;
        }
    }
}
