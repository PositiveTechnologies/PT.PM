using MessagePack;
using System;

namespace PT.PM.Common.Files
{
    [Union((int)FileType.TextFile, typeof(TextFile))]
    [Union((int)FileType.BinaryFile, typeof(BinaryFile))]
    public interface IFile : IComparable
    {
        [IgnoreMember]
        FileType Type { get; }

        [Key(0)]
        string RootPath { get; set; }

        [Key(1)]
        string RelativePath { get; set; }

        [Key(2)]
        string Name { get; set; }

        [Key(3)]
        string PatternKey { get; set; }

        [Key(4)]
        object Content { get; }

        [IgnoreMember]
        string RelativeName { get; }

        [IgnoreMember]
        string FullName { get; }

        [IgnoreMember]
        bool IsEmpty { get; }
    }
}
