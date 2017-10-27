using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Common.CodeRepository
{
    public class FilesAggregatorCodeRepository : SourceCodeRepository
    {
        public string SearchPattern { get; set; } = "*.*";

        public Func<string, bool> SearchPredicate { get; set; } = null;

        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;

        public IEnumerable<string> IgnoredFiles { get; set; } = Enumerable.Empty<string>();

        public FilesAggregatorCodeRepository(string directoryPath, params Language[] languages)
            : this(directoryPath, (IEnumerable<Language>)languages)
        {
        }

        public FilesAggregatorCodeRepository(string directoryPath, IEnumerable<Language> languages)
            : base()
        {
            RootPath = directoryPath;
            if (RootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                RootPath = RootPath.Remove(RootPath.Length - 1);
            }
            if (languages?.Count() > 0)
            {
                Languages = new HashSet<Language>(languages);
            }
        }

        public override IEnumerable<string> GetFileNames()
        {
            if (RootPath == null)
            {
                return Enumerable.Empty<string>();
            }

            var result = Directory.EnumerateFiles(RootPath, SearchPattern, SearchOption);
            if (SearchPredicate != null)
            {
                result = result.Where(SearchPredicate);
            }
            return result;
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

        public override bool IsFileIgnored(string fileName)
        {
            bool result = IgnoredFiles.Any(fileName.EndsWith);
            if (result)
            {
                return true;
            }

            return base.IsFileIgnored(fileName);
        }
    }
}
