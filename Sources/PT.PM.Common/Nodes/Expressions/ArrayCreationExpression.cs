using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Expressions
{
    public class ArrayCreationExpression : Expression
    {
        public override NodeType NodeType => NodeType.ArrayCreationExpression;

        public TypeToken Type { get; set; }

        public IEnumerable<Expression> Sizes { get; set; }

        public IEnumerable<Expression> Initializers { get; set; }

        public ArrayCreationExpression(TypeToken type, IEnumerable<Expression> sizes, IEnumerable<Expression> inits,
            TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Type = type;
            Sizes = sizes;
            Initializers = inits;
        }

        public ArrayCreationExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.Add(Type);
            result.AddRange(Sizes);
            if (Initializers != null)
            {
                result.AddRange(Initializers);
            }
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"new {Type}[{string.Join(", ", Sizes)}]";
        }
    }
}
