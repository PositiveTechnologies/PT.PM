using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace PT.PM.Common.CodeRepository
{
    public class FileCodeRepository : SourceCodeRepository
    {
        protected string fullName;

        public FileCodeRepository(string fileName, Language language = null)
        {
            RootPath = !string.IsNullOrEmpty(fileName)
                ? Path.GetDirectoryName(fileName)
                : "";
            fullName = fileName;
            if (language != null)
            {
                Languages = new HashSet<Language>() { language };
            }
        }

        public override IEnumerable<string> GetFileNames()
        {
            return new string[] { fullName };
        }

        public override CodeFile ReadFile(string fileName)
        {
            CodeFile result;
            try
            {
                result = new CodeFile(ReadCode(fileName))
                {
                    RootPath = RootPath,
                    Name = Path.GetFileName(fileName)
                };
            }
            catch (Exception ex)
            {
                result = new CodeFile("")
                {
                    RootPath = RootPath,
                    Name = Path.GetFileName(fileName)
                };
                Logger.LogError(new ReadException(result, ex));
            }
            
            return result;
        }

        public override bool IsFileIgnored(string fileName)
        {
            if (Languages.Count == 1)
            {
                return false;
            }

            return base.IsFileIgnored(fileName);
        }

        protected virtual string ReadCode(string fileName) => File.ReadAllText(fileName);
    }
}
