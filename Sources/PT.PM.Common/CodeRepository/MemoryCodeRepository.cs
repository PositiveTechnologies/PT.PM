using System.Collections.Generic;

namespace PT.PM.Common.CodeRepository
{
    public class MemoryCodeRepository : ISourceCodeRepository
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string Path { get; set; } = "";

        public string Code { get; set; }

        public MemoryCodeRepository(string code)
        {
            Code = code;
        }

        public IEnumerable<string> GetFileNames()
        {
            return new string[] { "Source Code" };
        }

        public string GetFullPath(string relativePath)
        {
            return Path;
        }

        public SourceCodeFile ReadFile(string fileName)
        {
            return new SourceCodeFile(fileName)
            {
                Code = Code,
                RelativePath = Path
            };
        }
    }
}
