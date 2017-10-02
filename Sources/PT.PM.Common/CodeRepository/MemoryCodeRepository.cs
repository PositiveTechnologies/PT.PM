using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;

namespace PT.PM.Common.CodeRepository
{
    public class MemoryCodeRepository : ISourceCodeRepository
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string Path { get; set; } = "";

        public string Code { get; set; }

        public MemoryCodeRepository(string code, string fileName = "")
        {
            Code = code;
            Path = fileName;
        }

        public IEnumerable<string> GetFileNames()
        {
            return new string[] { Path };
        }

        public string GetFullPath(string relativePath)
        {
            return Path;
        }

        public SourceCodeFile ReadFile(string fileName)
        {
            var result = new SourceCodeFile(fileName);
            try
            {
                result.RelativePath = "";
                result.Code = Code;
            }
            catch (Exception ex)
            {
                Logger.LogError(new ReadException(fileName, ex));
            }
            return result;
        }

        public bool IsFileIgnored(string fileName) => false;
    }
}
