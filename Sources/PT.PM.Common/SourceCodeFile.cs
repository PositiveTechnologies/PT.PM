using System;
using System.IO;

namespace PT.PM.Common
{
    public class SourceCodeFile
    {
        public string Name { get; set; }

        public string RelativePath { get; set; } = "";

        public string Code { get; set; }

        public string FullPath => Path.Combine(RelativePath, Name);

        public SourceCodeFile()
        {
        }

        public SourceCodeFile(string name)
        {
            Name = name;
        }

        public SourceCodeFile(SourceCodeFile sourceCodeFile)
        {
            Name = sourceCodeFile.Name;
            RelativePath = sourceCodeFile.RelativePath;
            Code = sourceCodeFile.Code;
        }
    }
}
