using System;
using System.Runtime.Serialization;

namespace PT.PM.Common.Exceptions
{
    public abstract class PMException : Exception
    {
        public abstract PMExceptionType ExceptionType { get; }

        public string FileName { get; set; }

        public bool IsPattern { get; set; }

        public PMException()
            : base()
        {
        }

        public PMException(Exception innerException, string message = "", bool isPattern = false)
            : base(message, innerException)
        {
            IsPattern = isPattern;
        }

        protected PMException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool printStackTrace)
        {
            string fileNameString = !string.IsNullOrEmpty(FileName) ? $" in \"{FileName}\"" : "";
            string patternString = IsPattern ? "Pattern " : "";

            string exceptionString = "";
            if (printStackTrace)
            {
                exceptionString = InnerException?.FormatExceptionMessage() ?? Message;
            }
            else
            {
                exceptionString = Message;
            }
            if (!string.IsNullOrEmpty(exceptionString))
            {
                exceptionString = $": {exceptionString}";
            }

            return $"{patternString}{ExceptionType} error{fileNameString}{exceptionString}.";
        }
    }
}
