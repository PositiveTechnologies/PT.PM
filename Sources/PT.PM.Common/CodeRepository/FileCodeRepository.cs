using System.Collections.Generic;
using System.IO;

namespace PT.PM.Common.CodeRepository
{
    public class FileCodeRepository : ISourceCodeRepository
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string Path { get; set; }

        public FileCodeRepository(string filePath)
        {
            Path = filePath;
        }

        public IEnumerable<string> GetFileNames()
        {
            return new string[] { Path };
        }

        public SourceCodeFile ReadFile(string fileName)
        {
            return new SourceCodeFile(fileName)
            {
                Code = File.ReadAllText(fileName),
                RelativePath = ""
            };
        }

        public SourceCodeFile ReadFile()
        {
            return ReadFile(Path);
        }

        public string GetFullPath(string relativePath)
        {
            return System.IO.Path.GetFullPath(relativePath);
        }

        public bool IsFileIgnored(string fileName) => false;
    }
}
