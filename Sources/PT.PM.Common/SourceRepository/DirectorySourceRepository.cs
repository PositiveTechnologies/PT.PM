using PT.PM.Common.Exceptions;
using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.Common.SourceRepository
{
    public class DirectorySourceRepository : SourceRepository
    {
        public string SearchPattern { get; set; } = "*.*";

        public Func<string, bool> SearchPredicate { get; set; } = null;

        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;

        public IEnumerable<string> IgnoredFiles { get; set; } = Enumerable.Empty<string>();

        public DirectorySourceRepository(string directoryPath, params Language[] languages)
            : this(directoryPath, (IEnumerable<Language>)languages)
        {
        }

        public DirectorySourceRepository(string directoryPath, IEnumerable<Language> languages = null)
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

            IEnumerable<string> result = DirectoryExt.EnumerateFiles(RootPath, SearchPattern, SearchOption);

            if (SearchPredicate != null)
            {
                result = result.Where(SearchPredicate);
            }

            return result;
        }

        public override IFile ReadFile(string fileName)
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

            IFile result;
            try
            {
                result = CommonUtils.GetFormatByFileName(name) == SerializationFormat.MsgPack
                    ? (IFile)new BinaryFile(FileExt.ReadAllBytes(fileName))
                    : new TextFile(FileExt.ReadAllText(fileName));
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                result = TextFile.Empty;
                Logger.LogError(new ReadException(result, ex));
            }

            result.RootPath = rootPath;
            result.RelativePath = relativePath;
            result.Name = name;

            return result;
        }

        public override bool IsFileIgnored(string fileName, bool withParser, out Language language)
        {
            bool result = IgnoredFiles.Any(fileName.EndsWith);
            if (result)
            {
                language = Language.Uncertain;
                return true;
            }

            return base.IsFileIgnored(fileName, withParser, out language);
        }
    }
}
