using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes
{
    public class RootUst : Ust
    {
        private Language[] sublanguges;

        public Language Language { get; }

        public SourceCodeFile SourceCodeFile { get; set; }

        public Ust[] Nodes { get; set; } = ArrayUtils<Ust>.EmptyArray;

        public CommentLiteral[] Comments { get; set; } = ArrayUtils<CommentLiteral>.EmptyArray;

        public Language[] Sublanguages => sublanguges ?? (sublanguges = GetSublangauges());

        public Ust Node
        {
            get => Nodes.FirstOrDefault();
            set => Nodes = new[] { value };
        }

        public int LineOffset { get; set; }

        public RootUst(SourceCodeFile sourceCodeFile, Language language)
        {
            SourceCodeFile = sourceCodeFile ?? SourceCodeFile.Empty;
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
            var descendants = this.WhereDescendants(child => child is RootUst)
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
