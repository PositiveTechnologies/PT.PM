using System.Collections.Generic;

namespace PT.PM.Common
{
    public abstract class ParseTree
    {
        public abstract Language SourceLanguage { get; }

        public string FileName { get; set; }

        public string FileData { get; set; }
    }
}
