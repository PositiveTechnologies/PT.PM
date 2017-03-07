using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using System;

namespace PT.PM.Common.Nodes.GeneralScope
{
    public class TypeDeclaration : EntityDeclaration
    {
        public override NodeType NodeType => NodeType.TypeDeclaration;

        public TypeTypeLiteral Type { get; set; }

        public IEnumerable<StringLiteral> BaseTypes { get; set; }

        public IEnumerable<EntityDeclaration> TypeMembers { get;set; }

        public TypeDeclaration(TypeTypeLiteral type, IdToken name, IEnumerable<EntityDeclaration> typeMembers, TextSpan textSpan, FileNode fileNode)
            : base(name, textSpan, fileNode)
        {
            Type = type;
            BaseTypes = new List<StringLiteral>();
            TypeMembers = typeMembers ?? ArrayUtils<EntityDeclaration>.EmptyArray;
        }

        public TypeDeclaration()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.AddRange(base.GetChildren());
            result.Add(Type);
            result.AddRange(BaseTypes);
            result.AddRange(TypeMembers);
            return result.ToArray();
        }

        public override string ToString()
        {
            string nl = Environment.NewLine;
            return $"{Name}{nl}{{  {string.Join(nl, TypeMembers)}}}";
        }
    }
}
