using PT.PM.Common.Exceptions;
using System;
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

        public SourceCodeFile ReadFile()
        {
            return ReadFile(Path);
        }

        public SourceCodeFile ReadFile(string fileName)
        {
            var result = new SourceCodeFile(fileName);
            try
            {
                result.RelativePath = "";
                result.Code = File.ReadAllText(fileName);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ReadException(fileName, ex));
            }
            return result;
        }

        public string GetFullPath(string relativePath)
        {
            return System.IO.Path.GetFullPath(relativePath);
        }

        public bool IsFileIgnored(string fileName) => false;
    }
}
