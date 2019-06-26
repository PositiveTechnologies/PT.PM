using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Newtonsoft.Json;
using PT.PM.Common.Files;

namespace PT.PM.Common.Nodes
{
    [MessagePackObject]
    public class RootUst : Ust
    {
        private Language[] sublanguages;

        [Key(0)] public override UstType UstType => UstType.RootUst;

        [JsonIgnore, IgnoreMember]
        public int FileKey { get; set; }

        [Key(UstFieldOffset)]
        public Language Language { get; set; }

        [Key(UstFieldOffset + 1)]
        public Ust[] Nodes { get; set; } = ArrayUtils<Ust>.EmptyArray;

        [Key(UstFieldOffset + 2)]
        public Comment[] Comments { get; set; } = ArrayUtils<Comment>.EmptyArray;

        [Key(UstFieldOffset + 3)]
        public int LineOffset { get; set; }

        [JsonProperty("SourceCodeFile"), JsonIgnore, IgnoreMember] // Workaround for correct deserialization of external jsons
        public TextFile SourceFile { get; set; }

        [JsonIgnore, IgnoreMember]
        public Language[] Sublanguages => sublanguages ?? (sublanguages = GetSublangauges());

        [JsonIgnore, IgnoreMember]
        public Ust Node
        {
            get => Nodes.Length > 0 ? Nodes[0] : null;
            set => Nodes = new[] { value };
        }

        public RootUst(TextFile sourceFile, Language language)
            : this(sourceFile, language, TextSpan.Zero)
        {
        }

        public RootUst(TextFile sourceFile, Language language, TextSpan textSpan)
            : base(textSpan)
        {
            SourceFile = sourceFile ?? TextFile.Empty;
            Language = language;
            TextSpan = textSpan.IsZero ? new TextSpan(0, SourceFile.Data.Length) : textSpan;
        }

        public RootUst()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Nodes);
            result.AddRange(Comments);
            return result.ToArray();
        }

        public void UpdateSublanguages()
        {
            sublanguages = GetSublangauges();
        }

        private Language[] GetSublangauges()
        {
            var result = new HashSet<Language>();
            var descendants = this.WhereDescendantsOrSelf(child => child is RootUst)
                .Cast<RootUst>();
            foreach (RootUst descendant in descendants)
            {
                result.Add(descendant.Language);
            }
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"{SourceFile.Name}: {base.ToString()}";
        }
    }
}
