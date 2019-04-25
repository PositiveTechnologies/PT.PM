using System;
using MessagePack;
using PT.PM.Common.Utils;

namespace PT.PM.Common.Files
{
    [MessagePackObject]
    [Union((int) FileType.TextFile, typeof(TextFile))]
    [Union((int) FileType.BinaryFile, typeof(BinaryFile))]
    public abstract class File<T> : IFile, IEquatable<File<T>>, IComparable<File<T>>
        where T : class
    {
        private T internalData; // Fill if file does not exist on disk

        protected WeakReference<T> data;

        [IgnoreMember]
        public abstract FileType Type { get; }

        [Key(0)]
        public string RootPath { get; set; } = "";

        [Key(1)]
        public string RelativePath { get; set; } = "";

        [Key(2)]
        public string Name { get; set; } = "";

        [Key(3)]
        public string PatternKey { get; set; }

        [Key(4)]
        public T Data
        {
            get
            {
                if (data.TryGetTarget(out T target))
                {
                    return target;
                }

                if (FileExt.Exists(FullName))
                {
                    T d = ReadData();
                    data.SetTarget(d);
                }

                return internalData;
            }
        }

        [IgnoreMember]
        public object Content => Data;

        [IgnoreMember]
        public string RelativeName => System.IO.Path.Combine(RelativePath, Name);

        [IgnoreMember]
        public string FullName => System.IO.Path.Combine(RootPath, RelativePath, Name);

        [IgnoreMember]
        public int DataLength { get; }

        [IgnoreMember]
        public abstract bool IsEmpty { get; }

        protected File(T data, int dataLength)
        {
            this.data = new WeakReference<T>(data ?? throw new ArgumentNullException(nameof(data)));
            DataLength = dataLength;
        }

        public override string ToString() => FullName;

        public static bool operator ==(File<T> sourceFile1, File<T> sourceFile2)
        {
            if (sourceFile1 is null)
            {
                return sourceFile2 is null;
            }

            return sourceFile1.Equals(sourceFile2);
        }

        public static bool operator !=(File<T> sourceFile1, File<T> sourceFile2)
        {
            if (sourceFile1 is null)
            {
                return !(sourceFile2 is null);
            }

            return !sourceFile1.Equals(sourceFile2);
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

            return String.Compare(FullName, other.FullName, StringComparison.Ordinal);
        }

        protected abstract int CompareData(T data1, T data2);

        protected abstract T ReadData();
    }
}