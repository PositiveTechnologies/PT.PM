using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PT.PM.Common.CodeRepository
{
    public class DirectoryCodeRepository : SourceCodeRepository
    {
        public string SearchPattern { get; set; } = "*.*";

        public Func<string, bool> SearchPredicate { get; set; } = null;

        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;

        public IEnumerable<string> IgnoredFiles { get; set; } = Enumerable.Empty<string>();

        public DirectoryCodeRepository(string directoryPath, params Language[] languages)
            : this(directoryPath, (IEnumerable<Language>)languages)
        {
        }

        public DirectoryCodeRepository(string directoryPath, IEnumerable<Language> languages)
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

            IEnumerable<string> result = Directory.EnumerateFiles(RootPath, SearchPattern, SearchOption);

            if (LoadJson)
            {
                result = result.Where(name => Path.GetExtension(name).EqualsIgnoreCase(".json"));
            }

            if (SearchPredicate != null)
            {
                result = result.Where(SearchPredicate);
            }

            return result;
        }

        public override CodeFile ReadFile(string fileName)
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

            string rootPath;
            if (fileName.StartsWith(RootPath))
            {
                rootPath = RootPath;
                int substringIndex = RootPath.Length;
                if (substringIndex + 1 < relativePath.Length && relativePath[substringIndex] == Path.DirectorySeparatorChar)
                {
                    substringIndex += 1;
                }
                relativePath = relativePath.Substring(substringIndex);
            }
            else
            {
                rootPath = "";
            }

            CodeFile result;
            try
            {
                result = new CodeFile(File.ReadAllText(fileName))
                {
                    RootPath = rootPath,
                    RelativePath = relativePath,
                    Name = name
                };
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                result = new CodeFile("")
                {
                    RootPath = rootPath,
                    RelativePath = relativePath,
                    Name = name
                };
                Logger.LogError(new ReadException(result, ex));
            }
            return result;
        }

        public override bool IsFileIgnored(string fileName, bool withParser)
        {
            bool result = IgnoredFiles.Any(fileName.EndsWith);
            if (result)
            {
                return true;
            }

            return base.IsFileIgnored(fileName, withParser);
        }
    }
}
