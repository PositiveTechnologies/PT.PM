using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Collections;

namespace PT.PM.Common.Nodes.Expressions
{
    public class ObjectCreateExpression : Expression
    {
        public override UstKind Kind => UstKind.ObjectCreateExpression;

        public Token Type { get; set; }

        public ArgsUst Arguments { get; set; }

        public List<Expression> Initializers { get; set; }

        public ObjectCreateExpression(Token type, ArgsUst args, TextSpan textSpan)
            : base(textSpan)
        {
            Type = type;
            Arguments = args;
        }

        public ObjectCreateExpression()
        {
            Arguments = new ArgsUst();
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.Add(Type);
            result.Add(Arguments);
            if (Initializers != null)
            {
                result.AddRange(Initializers);
            }
            else
            {
                result.Add(null);
            }
            return result.ToArray();
        }

        public override Expression[] GetArgs()
        {
            var result = new List<Expression> { Type };
            result.AddRange(Arguments.Collection);
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"new {Type}({string.Join(", ", Arguments)})";
        }
    }
}
