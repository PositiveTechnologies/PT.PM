using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace PT.PM.Common.CodeRepository
{
    public class FileCodeRepository : SourceCodeRepository
    {
        public FileCodeRepository(string filePath, Language language = null)
        {
            Path = filePath;
            if (language != null)
            {
                Languages = new HashSet<Language>() { language };
            }
        }

        public override IEnumerable<string> GetFileNames()
        {
            return new string[] { Path };
        }

        public override SourceCodeFile ReadFile(string fileName)
        {
            var result = new SourceCodeFile(fileName);
            try
            {
                result.RelativePath = "";
                result.Code = File.ReadAllText(fileName);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ReadException(fileName, ex));
            }
            return result;
        }

        public override string GetFullPath(string relativePath)
        {
            return System.IO.Path.GetFullPath(relativePath);
        }

        public override bool IsFileIgnored(string fileName)
        {
            if (Languages.Count == 1)
            {
                return false;
            }

            return base.IsFileIgnored(fileName);
        }
    }
}
