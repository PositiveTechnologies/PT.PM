using System.Collections.Generic;
using System.Linq;
using MessagePack;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.GeneralScope
{
    [MessagePackObject]
    public class NamespaceDeclaration : Ust
    {
        [Key(UstFieldOffset)]
        public StringLiteral Name { get; set; }

        [Key(UstFieldOffset + 1)]
        public List<Ust> Members { get; set; }

        public NamespaceDeclaration(StringLiteral name, Ust member,
            TextSpan textSpan)
            : this(name, new [] { member }, textSpan)
        {
        }

        public NamespaceDeclaration(StringLiteral name, IEnumerable<Ust> members,
            TextSpan textSpan)
            : base(textSpan)
        {
            Name = name;
            Members = members as List<Ust> ?? members.ToList();
        }

        public NamespaceDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new Ust[Members.Count + 1];
            int index = 0;
            result[index++] = Name;
            foreach (var member in Members)
                result[index++] = member;
            return result;
        }

        public override string ToString()
        {
            return $"namespace {Name}\n{{\n{string.Join("\n", Members).ToStringWithTrailNL()}}}";
        }
    }
}
