using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common
{
    public class ParsingException : Exception
    {
        public string FileName { get; set; }

        public TextSpan TextSpan { get; set; }

        public ParsingException()
            : base()
        {
        }

        public ParsingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }

        public ParsingException(string message)
            : base(message)
        {
        }

        public ParsingException(string fileName, Exception ex)
            : base("", ex)
        {
            FileName = fileName;
        }

        public ParsingException(string fileName, string message)
            : base(message)
        {
            FileName = fileName;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                return string.Format("Parsing error in \"{0}\": {1}.", FileName, InnerException != null ? InnerException.ToString() : Message);
            }
            else
            {
                return string.Format("Parsing error: {0}", InnerException != null ? InnerException.ToString() : Message);
            }
        }
    }
}
