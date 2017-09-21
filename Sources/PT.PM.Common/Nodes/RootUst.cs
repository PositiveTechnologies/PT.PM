using Newtonsoft.Json;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes
{
    public class RootUst : Ust
    {
        private Language[] sublanguges;

        public override UstKind Kind => UstKind.RootUst;

        public virtual Language Language { get; }

        public SourceCodeFile SourceCodeFile { get; set; }

        public Ust[] Nodes { get; set; } = ArrayUtils<Ust>.EmptyArray;

        public CommentLiteral[] Comments { get; set; } = ArrayUtils<CommentLiteral>.EmptyArray;

        [JsonIgnore]
        public Language[] Sublanguages => sublanguges ?? (sublanguges = GetSublangauges());

        [JsonIgnore]
        public Ust Node
        {
            get => Nodes.FirstOrDefault();
            set => Nodes = new[] { value };
        }

        public int LineOffset { get; set; }

        public RootUst(SourceCodeFile sourceCodeFile, Language language)
        {
            SourceCodeFile = sourceCodeFile ?? new SourceCodeFile();
            Language = language;
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
            var descendants = GetAllDescendants(child => child.Kind == UstKind.RootUst)
                .Cast<RootUst>();
            foreach (RootUst descendant in descendants)
            {
                result.Add(descendant.Language);
            }
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"{Language}: {base.ToString()}";
        }
    }
}
