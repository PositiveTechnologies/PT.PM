using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Files;
using PT.PM.Common.Utils;

namespace PT.PM.Common.SourceRepository
{
    public class MemorySourceRepository : FileSourceRepository
    {
        public Dictionary<string, object> Data { get; }

        public MemorySourceRepository(string code, string fileName = "", Language? language = null)
            : base(fileName, language)
        {
            Data = new Dictionary<string, object> { [fileName] = code };
        }

        public MemorySourceRepository(byte[] bytes, string fileName = "", Language? language = null)
            : base(fileName, language)
        {
            Data = new Dictionary<string, object> { [fileName] = bytes };
        }

        public override Language[] GetLanguages(string fileName, bool withParser)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return Languages.ToArray();
            }

            return base.GetLanguages(fileName, withParser);
        }

        public override IEnumerable<string> GetFileNames() => Data.Keys;

        protected override IFile Read(string fileName)
        {
            IFile result;
            if (CommonUtils.GetFormatByFileName(fileName) == SerializationFormat.MsgPack)
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
