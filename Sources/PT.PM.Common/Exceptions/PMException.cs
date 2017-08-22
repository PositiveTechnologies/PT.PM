using System;
using System.IO;
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
            return ToString(FileNameType.Short, false);
        }

        public string ToString(FileNameType fileNameType = FileNameType.Short, bool printStackTrace = false)
        {
            string fileName = fileNameType == FileNameType.None
                ? ""
                : fileNameType == FileNameType.Full
                ? FileName
                : Path.GetFileName(FileName);

            string fileNameString = !string.IsNullOrEmpty(fileName)
                ? $@" in ""{fileName}"""
                : "";
            string patternString = IsPattern ? "Pattern " : "";
            string exceptionString = printStackTrace
                ? InnerException?.FormatExceptionMessage() ?? Message
                : Message;

            if (!string.IsNullOrEmpty(exceptionString))
            {
                exceptionString = $": {exceptionString}";
            }

            return $"{patternString}{ExceptionType} Error{fileNameString}{exceptionString}.";
        }
    }
}
