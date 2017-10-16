using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;

namespace PT.PM.Common.CodeRepository
{
    public class MemoryCodeRepository : SourceCodeRepository
    {
        public string Code { get; set; }

        public MemoryCodeRepository(string code, string fileName = "", Language language = null)
        {
            Code = code;
            Path = fileName;
            if (language != null)
            {
                Languages = new HashSet<Language>() { language };
            }
        }

        public override IEnumerable<string> GetFileNames() => new string[] { Path };

        public override string GetFullPath(string relativePath) => Path;

        public override SourceCodeFile ReadFile(string fileName)
        {
            var result = new SourceCodeFile(fileName);
            try
            {
                result.RelativePath = "";
                result.Code = Code;
            }
            catch (Exception ex)
            {
                Logger.LogError(new ReadException(fileName, ex));
            }
            return result;
        }

        public override bool IsFileIgnored(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || Languages.Count == 1)
            {
                return false;
            }

            return base.IsFileIgnored(fileName);
        }
    }
}
