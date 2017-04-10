using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Collections;

namespace PT.PM.Common.Nodes.Expressions
{
    public class ObjectCreateExpression : Expression
    {
        public override NodeType NodeType => NodeType.ObjectCreateExpression;

        public Token Type { get; set; }

        public ArgsNode Arguments { get; set; }

        public List<Expression> Initializers { get; set; }

        public ObjectCreateExpression(Token type, ArgsNode args, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Type = type;
            Arguments = args;
        }

        public ObjectCreateExpression()
        {
            Arguments = new ArgsNode();
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
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

        public override string ToString()
        {
            return $"new {Type}({string.Join(", ", Arguments)})";
        }
    }
}
