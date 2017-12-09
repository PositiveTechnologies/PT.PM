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

        public override SourceCodeFile ReadFile(string fileName)
        {
            string code;
            try
            {
                code = ReadCode(fileName);
            }
            catch (Exception ex)
            {
                code = string.Empty;
                Logger.LogError(new ReadException(fileName, ex));
            }
            var result = new SourceCodeFile(code)
            {
                RootPath = RootPath,
                RelativePath = "",
                Name = Path.GetFileName(fileName)
            };
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
