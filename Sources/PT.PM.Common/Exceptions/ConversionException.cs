using System;
using System.Runtime.Serialization;

namespace PT.PM.Common.Exceptions
{
    public class ConversionException : PMException
    {
        public TextSpan TextSpan { get; set; }

        public ConversionException()
        {
        }

        public ConversionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ConversionException(SourceCodeFile sourceCodeFile, Exception ex = null, string message = "", bool isPattern = false)
            : base(ex, message, isPattern)
        {
            SourceCodeFile = sourceCodeFile ?? SourceCodeFile.Empty;
        }
    }
}
