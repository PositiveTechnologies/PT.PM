using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Exceptions
{
    public class PreprocessingException : Exception
    {
        public string FileName { get; set; }

        public PreprocessingException(string message)
            : base(message)
        {
        }

        public PreprocessingException(string fileName, Exception ex)
            : base("", ex)
        {
            FileName = fileName;
        }

        public PreprocessingException(string fileName, string message)
            : base(message)
        {
            FileName = fileName;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                return string.Format("Ust preprocessing error in \"{0}\": {1}.", FileName, InnerException != null ? InnerException.ToString() : Message);
            }
            else
            {
                return string.Format("Ust preprocessing error: {0}", InnerException != null ? InnerException.ToString() : Message);
            }
        }
    }
}
