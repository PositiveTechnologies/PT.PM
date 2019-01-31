using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.TypeMembers
{
    [MessagePackObject]
    public class FieldDeclaration : EntityDeclaration
    {
        [Key(EntityFieldOffset)]
        public TypeToken Type { get; set; }

        [Key(EntityFieldOffset + 1)]
        public List<AssignmentExpression> Variables { get; set; }

        public FieldDeclaration(TypeToken type, IEnumerable<AssignmentExpression> variables,
            TextSpan textSpan)
            : base(null, textSpan)
        {
            Type = type;
            Variables = variables as List<AssignmentExpression> ?? variables.ToList();
        }

        public FieldDeclaration(IEnumerable<AssignmentExpression> variables,
            TextSpan textSpan)
            : base(null, textSpan)
        {
            Type = null;
            Variables = variables as List<AssignmentExpression> ?? variables.ToList();
        }

        public FieldDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>(base.GetChildren());
            result.Add(Type);
            result.AddRange(Variables);
            return result.ToArray();
        }
    }
}
