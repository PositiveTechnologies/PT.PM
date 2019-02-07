using PT.PM.Common.Exceptions;
using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.Common.SourceRepository
{
    public class FileSourceRepository : SourceRepository
    {
        protected IEnumerable<string> fullNames;

        public static string GetLongestCommonPath(IList<string> fileNames)
        {
            if (fileNames.Count == 0)
            {
                return "";
            }

            string firstString = fileNames[0];
            int index = firstString.Length;

            for (int i = 1; i < fileNames.Count; i++)
            {
                index = Math.Min(index, fileNames[i].Length);
                for (int j = 0; j < index; j++)
                    if (fileNames[i][j] != firstString[j])
                    {
                        index = j;
                        break;
                    }
            }

            if (index == firstString.Length || firstString[index] != Path.DirectorySeparatorChar)
            {
                int lastSeparator = firstString.LastIndexOf(Path.DirectorySeparatorChar,
                    index == firstString.Length ? index - 1 : index);
                if (lastSeparator != -1)
                {
                    index = lastSeparator;
                }
            }

            return index < firstString.Length ? firstString.Remove(index) : firstString;
        }

        public FileSourceRepository(string fileName, Language? language = null)
            : this(new [] { fileName }, language)
        {
        }

        public FileSourceRepository(IEnumerable<string> fileNames, Language? language = null, string rootPath = null)
        {
            var fileNamesArray = fileNames as string[] ?? fileNames.ToArray();
            fullNames = fileNamesArray;

            if (language.HasValue)
            {
                Languages = new HashSet<Language> {language.Value};
            }

            RootPath = rootPath ?? GetLongestCommonPath(fileNamesArray);
        }

        public override IEnumerable<string> GetFileNames() => fullNames;

        public override IFile ReadFile(string fileName)
        {
            IFile result;
            string rootPath = "";
            string name = "";

            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    rootPath = Path.GetDirectoryName(fileName);
                }
                name = Path.GetFileName(fileName);
                result = Read(fileName);
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                result = TextFile.Empty;
                Logger.LogError(new ReadException(result, ex));
            }

            result.RootPath = rootPath;
            result.Name = name;

            return result;
        }

        public override Language[] GetLanguages(string fileName, bool withParser)
        {
            if (Languages.Count == 1)
            {
                Language language = Languages.First();
                return withParser && !language.IsParserExistsOrSerialized() ? new Language[0] : new Language[] { language };
            }

            return base.GetLanguages(fileName, withParser);
        }

        protected virtual IFile Read(string fileName)
        {
            IFile result;
            if (CommonUtils.GetFormatByFileName(fileName) == SerializationFormat.MsgPack)
            {
                result = new BinaryFile(FileExt.ReadAllBytes(fileName));
            }
            else
            {
                result = new TextFile(FileExt.ReadAllText(fileName));
            }
            return result;
        }
    }
}
