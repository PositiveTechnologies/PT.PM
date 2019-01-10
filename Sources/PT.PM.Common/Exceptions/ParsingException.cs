using System;
using System.Runtime.Serialization;
using PT.PM.Common.Files;

namespace PT.PM.Common.Exceptions
{
    public class ParsingException : PMException
    {
        public ParsingException()
            : base()
        {
        }

        public ParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ParsingException(IFile codeFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            File = codeFile ?? CodeFile.Empty;
        }
    }
}
