using System;
using PT.PM.Common.Files;

namespace PT.PM.Common.Exceptions
{
    public class PreprocessingException : PMException
    {
        public PreprocessingException(IFile file, Exception ex = null, string message = "")
            : base(ex, message)
        {
            File = file ?? TextFile.Empty;
        }
    }
}
