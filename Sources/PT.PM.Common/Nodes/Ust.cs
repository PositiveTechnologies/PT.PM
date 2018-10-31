using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace PT.PM.Common.Nodes
{
    [DebuggerDisplay("{ToStringWithoutLineBreaks()}")]
    public abstract class Ust : IComparable<Ust>, IEquatable<Ust>, IUst
    {
        private static readonly PrettyPrinter debuggerPrinter = new PrettyPrinter
        {
            MaxMessageLength = 0,
            ReduceWhitespaces = true
        };

        public string Kind => GetType().Name;

        public int KindId => GetType().Name.GetHashCode();

        public RootUst Root { get; set; }

        public virtual bool IsTerminal => false;

        public LineColumnTextSpan LineColumnTextSpan => CurrentCodeFile?.GetLineColumnTextSpan(TextSpan);

        public CodeFile CurrentCodeFile => this is RootUst rootUst ? rootUst.SourceCodeFile : Root?.SourceCodeFile;

        public RootUst RootOrThis => this is RootUst rootUst ? rootUst : Root;

        [JsonIgnore]
        public TextSpan TextSpan { get; set; }

        public Ust[] Children => GetChildren();

        public string ToStringWithoutLineBreaks() => debuggerPrinter?.Print(ToString()) ?? "";

        /// <summary>
        /// The list of text spans before any UST transformation or reduction.
        /// For example it won't be empty for concatenation of several strings,
        /// i.e. "1" + "2" + "3" -> "123".
        /// These spans map on an original source code.
        /// </summary>
        public List<TextSpan> InitialTextSpans { get; set; }

        public List<TextSpan> GetRealTextSpans()
        {
            if (InitialTextSpans != null && InitialTextSpans.Count > 0)
            {
                return InitialTextSpans;
            }
            else
            {
                return new List<TextSpan>() { TextSpan };
            }
        }

        protected Ust()
        {
        }

        protected Ust(TextSpan textSpan)
            : this()
        {
            TextSpan = textSpan;
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

        public override string ToString()
        {
            if (Children == null || Children.Length == 0)
            {
                return "";
            }

            var result = new StringBuilder();
            foreach (Ust child in Children)
            {
                result.Append(child);
                result.Append(" ");
            }
            return result.ToString();
        }
    }
}
