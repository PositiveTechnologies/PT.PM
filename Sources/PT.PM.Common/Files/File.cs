using System;
using MessagePack;

namespace PT.PM.Common.Files
{
    [MessagePackObject]
    [Union((int)FileType.CodeFile, typeof(CodeFile))]
    [Union((int)FileType.BinaryFile, typeof(BinaryFile))]
    public abstract class File<T> : IFile, IEquatable<File<T>>, IComparable<File<T>>
        where T : class
    {
        [IgnoreMember]
        public abstract FileType Type { get; }
        
        [Key(0)]
        public string RootPath { get; set; } = "";

        [Key(1)]
        public string RelativePath { get; set; } = "";

        [Key(2)]
        public string Name { get; set; } = "";

        [Key(3)]
        public string PatternKey { get; set; } = null;

        [Key(4)]
        public T Data { get; protected set; }

        [IgnoreMember]
        public object Content => Data;

        [IgnoreMember]
        public string RelativeName => System.IO.Path.Combine(RelativePath, Name);

        [IgnoreMember]
        public string FullName => System.IO.Path.Combine(RootPath, RelativePath, Name);

        [IgnoreMember]
        public abstract bool IsEmpty { get; }

        public File(T data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override string ToString() => FullName;

        public static bool operator ==(File<T> codeFile1, File<T> codeFile2)
        {
            if (codeFile1 is null)
            {
                return codeFile2 is null;
            }

            return codeFile1.Equals(codeFile2);
        }

        public static bool operator !=(File<T> codeFile1, File<T> codeFile2)
        {
            if (codeFile1 is null)
            {
                return !(codeFile2 is null);
            }

            return !codeFile1.Equals(codeFile2);
        }

        public override bool Equals(object obj) => Equals(obj as File<T>);

        public bool Equals(File<T> other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(FullName) && string.IsNullOrEmpty(other.FullName))
            {
                return Data.Equals(other.Data);
            }

            return FullName == other.FullName;
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(RelativeName)
                ? Data.GetHashCode()
                : RelativeName.GetHashCode();
        }

        public int CompareTo(object obj) => CompareTo(obj as File<T>);

        public int CompareTo(File<T> other)
        {
            if (other is null)
            {
                return 1;
            }

            if (string.IsNullOrEmpty(FullName) && string.IsNullOrEmpty(other.FullName))
            {
                return CompareData(Data, other.Data);
            }

            return FullName.CompareTo(other.FullName);
        }

        protected abstract int CompareData(T data1, T data2);
    }
}