using System;
using System.Runtime.Serialization;

namespace PT.PM.Common.Exceptions
{
    public class ParsingException : PMException
    {
        public TextSpan TextSpan { get; set; }

        public ParsingException()
            : base()
        {
        }

        public ParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ParsingException(CodeFile codeFile, Exception ex = null, string message = "")
            : base(ex, message)
        {
            CodeFile = codeFile ?? CodeFile.Empty;
        }
    }
}
