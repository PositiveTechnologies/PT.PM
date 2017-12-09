using System;
using System.Runtime.Serialization;

namespace PT.PM.Common.Exceptions
{
    public abstract class PMException : Exception
    {
        public string ExceptionType => GetType().Name;

        public SourceCodeFile SourceCodeFile { get; set; }

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
            return ToString(FileNameType.Relative, false);
        }

        public string ToString(FileNameType fileNameType = FileNameType.Relative, bool printStackTrace = false)
        {
            string fileName = fileNameType == FileNameType.None
                ? ""
                : fileNameType == FileNameType.Full
                ? SourceCodeFile.FullName
                : fileNameType == FileNameType.Relative
                ? SourceCodeFile.RelativeName
                : SourceCodeFile.Name;

            string fileNameString = !string.IsNullOrEmpty(fileName)
                ? $@" in ""{fileName}"""
                : "";
            string patternString = IsPattern ? "Pattern " : "";

            string exceptionString = printStackTrace
                ? InnerException?.FormatExceptionMessage() ?? Message
                : Message;

            if (string.IsNullOrEmpty(exceptionString))
            {
                exceptionString = InnerException?.FormatExceptionMessage();
            }

            if (!string.IsNullOrEmpty(exceptionString))
            {
                exceptionString = $": {exceptionString}";
            }

            return $"{patternString}{ExceptionType}{fileNameString}{exceptionString}.";
        }
    }
}
