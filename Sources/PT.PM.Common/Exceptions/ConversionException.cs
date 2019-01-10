using System;
using System.Runtime.Serialization;
using PT.PM.Common.Files;

namespace PT.PM.Common.Exceptions
{
    public class ConversionException : PMException
    {
        public ConversionException()
        {
        }

        public ConversionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ConversionException(CodeFile codeFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            File = codeFile ?? CodeFile.Empty;
        }
    }
}
