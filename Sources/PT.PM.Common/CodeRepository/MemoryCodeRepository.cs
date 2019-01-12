using System.Collections.Generic;
using PT.PM.Common.Files;

namespace PT.PM.Common.CodeRepository
{
    public class MemoryCodeRepository : FileCodeRepository
    {
        public Dictionary<string, object> Data { get; set; }

        public MemoryCodeRepository(string code, string fileName = "", Language? language = null)
            : base(fileName, language, null)
        {
            Data = new Dictionary<string, object> { [fileName] = code };
        }

        public MemoryCodeRepository(byte[] bytes, string fileName = "", Language? language = null)
            : base(fileName, language, SerializationFormat.MsgPack)
        {
            Data = new Dictionary<string, object> { [fileName] = bytes };
        }

        public override bool IsFileIgnored(string fileName, bool withParser)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return base.IsFileIgnored(fileName, withParser);
        }

        public override IEnumerable<string> GetFileNames() => Data.Keys;

        protected override IFile Read(string fileName)
        {
            IFile result;
            if (Format == SerializationFormat.MsgPack)
            {
                result = new BinaryFile((byte[])Data[fileName]);
            }
            else
            {
                result = new TextFile((string)Data[fileName]);
            }
            return result;
        }
    }
}
