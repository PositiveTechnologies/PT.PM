using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Exceptions
{
    public class MatchingException : Exception
    {
        public string FileName { get; set; }

        public TextSpan TextSpan { get; set; }

        public MatchingException()
        {
        }

        public MatchingException(string fileName, Exception ex)
            : base("", ex)
        {
            FileName = fileName;
        }

        public MatchingException(string fileName, string message)
            : base(message)
        {
            FileName = fileName;
        }

        public override string ToString()
        {
            return string.Format("Matching error in \"{0}\": {1}.", FileName, InnerException != null ? InnerException.ToString() : Message);
        }
    }
}
