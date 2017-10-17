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
            RootPath = directoryPath;
            if (RootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                RootPath = RootPath.Remove(RootPath.Length - 1);
            }
            Languages = new HashSet<Language>(languages);
        }

        public override IEnumerable<string> GetFileNames()
        {
            return Directory.EnumerateFiles(RootPath, SearchPattern, SearchOption);
        }

        public override SourceCodeFile ReadFile(string fileName)
        {
            string name = Path.GetFileName(fileName);
            string relativePath;
            int removeIndex = fileName.Length - name.Length;
            if (removeIndex != 0)
            {
                removeIndex -= 1; // remove directory seprator char
                relativePath = fileName.Remove(removeIndex);
            }
            else
            {
                relativePath = fileName;
            }

            int substringIndex = RootPath.Length;
            if (substringIndex + 1 < relativePath.Length && relativePath[substringIndex] == Path.DirectorySeparatorChar)
            {
                substringIndex += 1;
            }
            relativePath = relativePath.Substring(substringIndex);

            var result = new SourceCodeFile
            {
                RootPath = RootPath,
                RelativePath = relativePath,
                Name = name
            };
            try
            {
                result.Code = File.ReadAllText(fileName);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(new ReadException(fileName, ex));
            }
            return result;
        }
    }
}
