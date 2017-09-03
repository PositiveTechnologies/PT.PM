using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Common.CodeRepository
{
    public class FilesAggregatorCodeRepository : ISourceCodeRepository
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string Path { get; set; }

        public IEnumerable<string> Extensions { get; set; } = LanguageExt.AllExtensions;

        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;

        public FilesAggregatorCodeRepository(string directoryPath, string extension)
            : this(directoryPath, new[] { extension })
        {
        }

        public FilesAggregatorCodeRepository(string directoryPath, IEnumerable<string> extensions)
        {
            Path = directoryPath;
            Extensions = extensions;
        }

        public IEnumerable<string> GetFileNames()
        {
            return Directory.EnumerateFiles(Path, "*.*", SearchOption);
        }

        public SourceCodeFile ReadFile(string fileName)
        {
            SourceCodeFile result = null;
            try
            {
                var removeBeginLength = Path.Length + (Path.EndsWith("\\") ? 0 : 1);
                var shortFileName = System.IO.Path.GetFileName(fileName);
                result = new SourceCodeFile(shortFileName);
            
                int removeEndLength = shortFileName.Length + 1;
                result.RelativePath = removeEndLength + removeBeginLength > fileName.Length
                        ? "" : fileName.Remove(fileName.Length - removeEndLength).Remove(0, removeBeginLength);
                result.Code = File.ReadAllText(fileName);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(new ReadException(fileName, ex));
                if (result == null)
                {
                    result = new SourceCodeFile(fileName);
                }
            }
            return result;
        }

        public string GetFullPath(string relativePath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetFullPath(Path), relativePath);
        }

        public bool IsFileIgnored(string fileName)
        {
            return !Extensions.Any(fileName.EndsWith);
        }
    }
}
