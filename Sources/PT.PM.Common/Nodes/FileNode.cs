using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using Newtonsoft.Json;
using System.Linq;
using PT.PM.Common.Nodes.GeneralScope;

namespace PT.PM.Common.Nodes
{
    public class FileNode : UstNode
    {
        public override NodeType NodeType => NodeType.FileNode;

        public StringLiteral FileName { get; set; }

        [JsonIgnore]
        public string FileData { get; set; }

        public UstNode Root { get; set; }

        public FileNode(string fileName, string fileData)
        {
            FileName = new StringLiteral(fileName, default(TextSpan), null);
            FileData = fileData;
        }

        public FileNode()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode> {FileName};
            result.Add(Root);
            return result.ToArray();
        }
    }
}
