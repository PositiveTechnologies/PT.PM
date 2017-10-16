using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace PT.PM.Common.CodeRepository
{
    public class FilesAggregatorCodeRepository : SourceCodeRepository
    {
        public string SearchPattern { get; set; } = "*.*";

        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;

        public FilesAggregatorCodeRepository(string directoryPath, params Language[] languages)
            : this(directoryPath, (IEnumerable<Language>)languages)
        {
        }

        public FilesAggregatorCodeRepository(string directoryPath, IEnumerable<Language> languages)
        {
            Path = directoryPath;
            Languages = new HashSet<Language>(languages);
        }

        public override IEnumerable<string> GetFileNames()
        {
            return Directory.EnumerateFiles(Path, SearchPattern, SearchOption);
        }

        public override SourceCodeFile ReadFile(string fileName)
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

        public override string GetFullPath(string relativePath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetFullPath(Path), relativePath);
        }
    }
}
