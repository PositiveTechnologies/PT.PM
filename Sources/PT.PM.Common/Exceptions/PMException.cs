using System;
using System.Runtime.Serialization;
using PT.PM.Common.Files;

namespace PT.PM.Common.Exceptions
{
    public abstract class PMException : Exception
    {
        public string ExceptionType => GetType().Name;

        public IFile File { get; set; } = CodeFile.Empty;

        public TextSpan TextSpan { get; set; }

        public PMException()
            : base()
        {
        }

        public PMException(Exception innerException, string message = "")
            : base(message, innerException)
        {
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
                ? File.FullName
                : fileNameType == FileNameType.Relative
                ? File.RelativeName
                : File.Name;

            string fileNameString = !string.IsNullOrEmpty(fileName)
                ? $@" in ""{fileName}"""
                : "";
            string patternString = !string.IsNullOrEmpty(File.PatternKey) ? $"Pattern {File.PatternKey} " : "";

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
