using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class VariableDeclarationExpression : Expression
    {
        [Key(UstFieldOffset)]
        public TypeToken Type { get; set; }

        [Key(UstFieldOffset + 1)]
        public List<AssignmentExpression> Variables { get; set; }

        public VariableDeclarationExpression(TypeToken type, IEnumerable<AssignmentExpression> variables,
            TextSpan textSpan = default)
            : base(textSpan)
        {
            Type = type;
            Variables = variables as List<AssignmentExpression> ?? variables.ToList();
        }

        public VariableDeclarationExpression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public VariableDeclarationExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.Add(Type);
            result.AddRange(Variables);
            return result.ToArray();
        }

        public override Expression[] GetArgs()
        {
            return new Expression[0]; // TODO: Fix (I don't know how, maybe transform VariableDeclarationExpression to something else).
        }

        public override string ToString()
        {
            return $"{Type} {string.Join(", ", Variables)}";
        }
    }
}
