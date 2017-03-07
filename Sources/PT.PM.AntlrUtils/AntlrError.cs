using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.AntlrUtils
{
    public class AntlrError
    {
        public int Line { get; set; }

        public int Column { get; set; }

        public string Message { get; set; }

        public RecognitionException Exception { get; set; }

        public AntlrError(int line, int column, string message, RecognitionException exception)
        {
            Line = line;
            Column = column;
            Message = message;
            Exception = exception;
        }

        public override string ToString()
        {
            return string.Format("{0} at {1}:{2}", Message, Line, Column);
        }
    }
}
