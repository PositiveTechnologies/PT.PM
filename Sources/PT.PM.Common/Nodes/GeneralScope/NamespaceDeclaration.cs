using System.Collections.Generic;
using System.Linq;
using System;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.GeneralScope
{
    public class NamespaceDeclaration : Ust
    {
        public override UstKind Kind => UstKind.NamespaceDeclaration;

        public StringLiteral Name { get; set; }

        public List<Ust> Members { get; set; }

        public NamespaceDeclaration(StringLiteral name, Ust member,
            TextSpan textSpan)
            : this(name, new Ust[] { member }, textSpan)
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
            string nl = Environment.NewLine;
            return $"namespace {Name}\r\n{{  {(Members == null ? string.Empty : string.Join(nl, Members))}{nl}}}";
        }
    }
}
