using System;

namespace PT.PM.Common.Exceptions
{
    public class ConversionException : Exception
    {
        public string FileName { get; set; }

        public TextSpan TextSpan { get; set; }

        public ConversionException()
        {
        }

        public ConversionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }

        public ConversionException(string message)
            : base(message)
        {
        }

        public ConversionException(string fileName, Exception ex)
            : base("", ex)
        {
            FileName = fileName;
        }

        public ConversionException(string fileName, string message)
            : base(message)
        {
            FileName = fileName;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                return string.Format("Conversion error in \"{0}\": {1}.", FileName, InnerException != null ? InnerException.ToString() : Message);
            }
            else
            {
                return string.Format("Conversion error: {0}", InnerException != null ? InnerException.ToString() : Message);
            }
        }
    }
}
