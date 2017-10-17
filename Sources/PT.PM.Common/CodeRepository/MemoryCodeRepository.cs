namespace PT.PM.Common.CodeRepository
{
    public class MemoryCodeRepository : FileCodeRepository
    {
        public string Code { get; set; }

        public MemoryCodeRepository(string code, string fileName = "", Language language = null)
            : base(fileName, language)
        {
            Code = code;
        }

        public override bool IsFileIgnored(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return base.IsFileIgnored(fileName);
        }

        protected override string ReadCode(string fileName) => Code;
    }
}
