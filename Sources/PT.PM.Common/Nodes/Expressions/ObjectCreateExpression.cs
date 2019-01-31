using PT.PM.Common.Nodes.Collections;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class ObjectCreateExpression : Expression
    {
        [Key(UstFieldOffset)]
        public Expression Type { get; set; }

        [Key(UstFieldOffset + 1)]
        public ArgsUst Arguments { get; set; }

        [Key(UstFieldOffset + 2)]
        public List<Expression> Initializers { get; set; }

        public ObjectCreateExpression(Expression target, ArgsUst args, TextSpan textSpan)
            : base(textSpan)
        {
            Type = target;
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

        public override string ToString() => $"new {Type}({string.Join(", ", Arguments)})";
    }
}
