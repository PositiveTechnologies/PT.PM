using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes.Tokens;
using System;

namespace PT.PM.Common.Nodes.GeneralScope
{
    public class NamespaceDeclaration : UstNode
    {
        public override NodeType NodeType => NodeType.NamespaceDeclaration;

        public StringLiteral Name { get; set; }

        public List<UstNode> Members { get; set; }

        public Language Language { get; set; }

        public NamespaceDeclaration(StringLiteral name, UstNode member,
            Language language, TextSpan textSpan, FileNode fileNode)
            : this(name, new UstNode[] { member }, language, textSpan, fileNode)
        {
        }

        public NamespaceDeclaration(StringLiteral name, IEnumerable<UstNode> members,
            Language language, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Name = name;
            Members = members as List<UstNode> ?? members.ToList();
            Language = language;
        }

        public NamespaceDeclaration()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new UstNode[Members.Count + 1];
            int index = 0;
            result[index++] = Name;
            foreach (var member in Members)
                result[index++] = member;
            return result;
        }

        public override string ToString()
        {
            string nl = Environment.NewLine;
            return $"namespace {Name}\r\n{{  {(Members == null ? string.Empty : string.Join(nl, Members))}{nl}}}";
        }
    }
}
