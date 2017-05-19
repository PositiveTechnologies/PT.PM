using System;
using System.Runtime.Serialization;

namespace PT.PM.Common.Exceptions
{
    public class ParsingException : PMException
    {
        public override PMExceptionType ExceptionType => PMExceptionType.Parsing;

        public TextSpan TextSpan { get; set; }

        public ParsingException()
            : base()
        {
        }

        public ParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ParsingException(string fileName, Exception ex = null, string message = "")
            : base(ex, message)
        {
            FileName = fileName;
        }
    }
}
