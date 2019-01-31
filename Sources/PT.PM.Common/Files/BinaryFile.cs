using MessagePack;

namespace PT.PM.Common.Files
{
    [MessagePackObject]
    public class BinaryFile : File<byte[]>
    {
        public static BinaryFile Empty => new BinaryFile(new byte[0]);

        [IgnoreMember]
        public override FileType Type => FileType.BinaryFile;

        [IgnoreMember]
        public override bool IsEmpty => Data.Length == 0;

        public BinaryFile(byte[] data)
            : base(data)
        {
        }

        protected override int CompareData(byte[] data1, byte[] data2)
        {
            if (data2 == null)
            {
                return 1;
            }

            if (data1 == data2)
            {
                return 0;
            }

            if (data1.Length != data2.Length)
            {
                return data1.Length - data2.Length;
            }

            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != data2[i])
                {
                    return data1[i] - data2[i];
                }
            }

            return 0;
        }
    }
}