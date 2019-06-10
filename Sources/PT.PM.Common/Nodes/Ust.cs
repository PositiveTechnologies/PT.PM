using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using MessagePack;
using PT.PM.Common.Files;

namespace PT.PM.Common.Nodes
{
    [DebuggerDisplay("{" + nameof(ToStringWithoutLineBreaks) + "()}")]
    [MessagePackObject]
    public abstract class Ust : IComparable<Ust>, IEquatable<Ust>
    {
        internal const int UstFieldOffset = 3;

        private static readonly PrettyPrinter debuggerPrinter = new PrettyPrinter
        {
            MaxMessageLength = 0,
            ReduceWhitespaces = true
        };

        [IgnoreMember]
        public abstract UstType UstType { get; }

        [IgnoreMember, JsonIgnore]
        public RootUst Root { get; set; }

        [IgnoreMember, JsonIgnore]
        public Ust Parent { get; set; }

        [Key(1), JsonIgnore]
        public TextSpan TextSpan { get; set; }

        [Key(2)]
        public int Key { get; set; }

        [IgnoreMember, JsonIgnore]
        public string Kind => GetType().Name;

        [IgnoreMember, JsonIgnore]
        public int KindId => GetType().Name.GetHashCode();

        [IgnoreMember, JsonIgnore]
        public string Substring => CurrentSourceFile.Data.Substring(TextSpan);

        [IgnoreMember, JsonIgnore]
        public LineColumnTextSpan LineColumnTextSpan => CurrentSourceFile?.GetLineColumnTextSpan(TextSpan) ?? LineColumnTextSpan.Zero;

        [IgnoreMember, JsonIgnore]
        public TextFile CurrentSourceFile => this is RootUst rootUst ? rootUst.SourceFile : Root?.SourceFile;

        [IgnoreMember, JsonIgnore]
        public RootUst RootOrThis => this is RootUst rootUst ? rootUst : Root;

        [IgnoreMember, JsonIgnore]
        public Ust[] Children => GetChildren();

        public int GetKey()
        {
            if (Key != 0)
            {
                return Key;
            }

            return base.GetHashCode();
        }

        public string ToStringWithoutLineBreaks() => debuggerPrinter?.Print(ToString()) ?? "";

        protected Ust(TextSpan textSpan, RootUst rootUst)
            : this(textSpan)
        {
            Root = rootUst;
        }

        protected Ust(TextSpan textSpan)
        {
            TextSpan = textSpan;
        }

        protected Ust()
        {
        }

        public abstract Ust[] GetChildren();

        public bool Equals(Ust other)
        {
            return CompareTo(other) == 0;
        }

        public virtual int CompareTo(Ust other)
        {
            if (other == null)
            {
                return KindId;
            }

            int nodeTypeCompareResult = KindId - other.KindId;
            if (nodeTypeCompareResult != 0)
            {
                return nodeTypeCompareResult;
            }

            return Children.CompareTo(other.Children);
        }

        public override string ToString() => ToString(true);

        public string ToString(bool space)
        {
            if (Children == null || Children.Length == 0)
            {
                return Substring;
            }

            var result = new StringBuilder();
            foreach (Ust child in Children)
            {
                result.Append(child);
                if (space)
                {
                    result.Append(' ');
                }
            }
            return result.ToString();
        }
    }
}
