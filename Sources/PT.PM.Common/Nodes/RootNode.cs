using Newtonsoft.Json;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes
{
    public class RootNode : UstNode
    {
        private Language[] sublanguges;

        public override NodeType NodeType => NodeType.RootNode;

        public virtual Language Language { get; }

        public SourceCodeFile SourceCodeFile { get; set; }

        public UstNode[] Nodes { get; set; } = ArrayUtils<UstNode>.EmptyArray;

        public CommentLiteral[] Comments { get; set; } = ArrayUtils<CommentLiteral>.EmptyArray;

        [JsonIgnore]
        public Language[] Sublanguages => sublanguges ?? (sublanguges = GetSublangauges());

        [JsonIgnore]
        public UstNode Node
        {
            get => Nodes.FirstOrDefault();
            set => Nodes = new[] { value };
        }

        public int LineOffset { get; set; }

        public RootNode(SourceCodeFile sourceCodeFile, Language language)
        {
            SourceCodeFile = sourceCodeFile ?? new SourceCodeFile();
            Language = language;
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
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
            var descendants = GetAllDescendants(child => child.NodeType == NodeType.RootNode)
                .Cast<RootNode>();
            foreach (RootNode descendant in descendants)
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
