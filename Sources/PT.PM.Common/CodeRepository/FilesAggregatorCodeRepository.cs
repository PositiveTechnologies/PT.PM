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

        public IEnumerable<string> Extenstions { get; set; } = LanguageExt.Extensions;

        public FilesAggregatorCodeRepository(string directoryPath, string extension)
            : this(directoryPath, new[] { extension })
        {
        }

        public FilesAggregatorCodeRepository(string directoryPath, IEnumerable<string> extensions)
        {
            Path = directoryPath;
            Extenstions = extensions;
        }

        public IEnumerable<string> GetFileNames()
        {
            string[] files = Directory.GetFiles(Path, "*.*", SearchOption.AllDirectories);
            return files;
        }

        public SourceCodeFile ReadFile(string file)
        {
            var removeBeginLength = Path.Length + (Path.EndsWith("\\") ? 0 : 1);
            var fileName = System.IO.Path.GetFileName(file);
            SourceCodeFile result = new SourceCodeFile(fileName);
            try
            {
                int removeEndLength = fileName.Length + 1;
                result.RelativePath = removeEndLength + removeBeginLength > file.Length
                        ? "" : file.Remove(file.Length - removeEndLength).Remove(0, removeBeginLength);
                result.Code = File.ReadAllText(file);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError("File read error: " + file, ex);
            }
            return result;
        }

        public string GetFullPath(string relativePath)
        {
            var result = System.IO.Path.Combine(System.IO.Path.GetFullPath(Path), relativePath);
            return result;
        }

        public bool IsFileIgnored(string fileName)
        {
            return !Extenstions.Any(fileName.EndsWith);
        }
    }
}
