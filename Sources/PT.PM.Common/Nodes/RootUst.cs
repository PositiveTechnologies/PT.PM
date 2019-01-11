using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Newtonsoft.Json;
using PT.PM.Common.Files;
using PT.PM.Common.MessagePack;

namespace PT.PM.Common.Nodes
{
    [MessagePackObject]
    [MessagePackFormatter(typeof(RootUstFormatter))]
    public class RootUst : Ust
    {
        private Language[] sublanguges;

        [Key(UstFieldOffset)]
        public Language Language { get; }

        [JsonProperty("SourceCodeFile")] // TODO: back compatibility with extenral serializers
        [Key(UstFieldOffset + 1)]
        public TextFile SourceFile { get; set; }

        [Key(UstFieldOffset + 2)]
        public Ust[] Nodes { get; set; } = ArrayUtils<Ust>.EmptyArray;

        [Key(UstFieldOffset + 3)]
        public CommentLiteral[] Comments { get; set; } = ArrayUtils<CommentLiteral>.EmptyArray;

        [Key(UstFieldOffset + 4)]
        public int LineOffset { get; set; }

        [IgnoreMember]
        [JsonIgnore]
        public Dictionary<Ust, List<TextSpan>> TextSpans { get; } = new Dictionary<Ust, List<TextSpan>>(UstRefComparer.Instance);

        [IgnoreMember]
        public Language[] Sublanguages => sublanguges ?? (sublanguges = GetSublangauges());

        [IgnoreMember]
        public Ust Node
        {
            get => Nodes.FirstOrDefault();
            set => Nodes = new[] { value };
        }

        public RootUst(TextFile sourceFile, Language language)
        {
            SourceFile = sourceFile ?? TextFile.Empty;
            Language = language;
            TextSpan = new TextSpan(0, SourceFile.Data.Length);
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
            sublanguges = GetSublangauges();
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
