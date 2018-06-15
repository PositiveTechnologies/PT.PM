using System.Collections.Generic;

namespace PT.PM.Common.CodeRepository
{
    public class MemoryCodeRepository : FileCodeRepository
    {
        public Dictionary<string, string> Code { get; set; }

        public MemoryCodeRepository(string code, string fileName = "", Language language = null)
            : this(new Dictionary<string, string>() { [fileName] = code }, language)
        {
        }

        public MemoryCodeRepository(Dictionary<string, string> code, Language language = null)
            : base(code.Keys, language)
        {
            Code = code;
        }

        public override bool IsFileIgnored(string fileName, bool withParser)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return base.IsFileIgnored(fileName, withParser);
        }

        public override IEnumerable<string> GetFileNames() => Code.Keys;

        protected override string ReadCode(string fileName) => Code[fileName];
    }
}
