using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PT.PM.Common.Nodes
{
    public class RootUst : Ust
    {
        private Language[] sublanguges;

        public Language Language { get; }

        public CodeFile SourceCodeFile { get; set; }

        public Ust[] Nodes { get; set; } = ArrayUtils<Ust>.EmptyArray;

        public CommentLiteral[] Comments { get; set; } = ArrayUtils<CommentLiteral>.EmptyArray;

        [JsonIgnore]
        public Dictionary<Ust, List<TextSpan>> TextSpans { get; } = new Dictionary<Ust, List<TextSpan>>(UstRefComparer.Instance);
        
        public Language[] Sublanguages => sublanguges ?? (sublanguges = GetSublangauges());

        public Ust Node
        {
            get => Nodes.FirstOrDefault();
            set => Nodes = new[] { value };
        }

        public int LineOffset { get; set; }

        public RootUst(CodeFile sourceCodeFile, Language language)
        {
            SourceCodeFile = sourceCodeFile ?? CodeFile.Empty;
            Language = language;
            TextSpan = new TextSpan(0, SourceCodeFile.Code.Length);
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
            return $"{SourceCodeFile.Name}: {base.ToString()}";
        }
    }
}
