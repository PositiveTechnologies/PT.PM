using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PT.PM.Common.CodeRepository
{
    public class FileCodeRepository : SourceCodeRepository
    {
        protected IEnumerable<string> fullNames;

        public FileCodeRepository(string fileName, Language language = null)
            : this(new string[] { fileName }, language)
        {
        }

        public FileCodeRepository(IEnumerable<string> fileNames, Language language = null)
        {
            fullNames = fileNames;
            if (language != null)
            {
                Languages = new HashSet<Language>() { language };
            }
            RootPath = string.Join("; ", fileNames);
        }

        public override IEnumerable<string> GetFileNames() => fullNames;

        public override CodeFile ReadFile(string fileName)
        {
            CodeFile result;
            try
            {
                result = new CodeFile(ReadCode(fileName))
                {
                    RootPath = !string.IsNullOrEmpty(fileName) ? Path.GetDirectoryName(fileName): "",
                    Name = Path.GetFileName(fileName)
                };
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                result = new CodeFile("")
                {
                    RootPath = !string.IsNullOrEmpty(fileName) ? Path.GetDirectoryName(fileName) : "",
                    Name = Path.GetFileName(fileName)
                };
                Logger.LogError(new ReadException(result, ex));
            }
            
            return result;
        }

        public override bool IsFileIgnored(string fileName, bool withParser)
        {
            if (Languages.Count == 1)
            {
                return withParser ? !Languages.First().IsParserExists() : false;
            }

            return base.IsFileIgnored(fileName, withParser);
        }

        protected virtual string ReadCode(string fileName) => File.ReadAllText(fileName);
    }
}
