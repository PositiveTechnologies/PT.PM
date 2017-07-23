using System;
using System.Runtime.Serialization;

namespace PT.PM.Common.Exceptions
{
    public class ConversionException : PMException
    {
        public override PMExceptionType ExceptionType => PMExceptionType.Conversion;

        public TextSpan TextSpan { get; set; }

        public ConversionException()
        {
        }

        public ConversionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ConversionException(string fileName, Exception ex = null, string message = "", bool isPattern = false)
            : base(ex, message, isPattern)
        {
            FileName = fileName;
        }
    }
}
