using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using System.Linq;

namespace PT.PM.Common.Nodes.Expressions
{
    public class ArrayCreationExpression : Expression
    {
        public override NodeType NodeType => NodeType.ArrayCreationExpression;

        public TypeToken Type { get; set; }

        public List<Expression> Sizes { get; set; }

        public List<Expression> Initializers { get; set; }

        public ArrayCreationExpression(TypeToken type, IEnumerable<Expression> sizes, IEnumerable<Expression> inits,
            TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Type = type;
            Sizes = sizes as List<Expression> ?? Sizes?.ToList();
            Initializers = inits as List<Expression> ?? inits?.ToList();
        }

        public ArrayCreationExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.Add(Type);
            if (Sizes != null)
            {
                result.AddRange(Sizes);
            }
            if (Initializers != null)
            {
                result.AddRange(Initializers);
            }
            return result.ToArray();
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            var nodeTypeCompareResult = NodeType - other.NodeType;
            if (nodeTypeCompareResult != 0)
            {
                return nodeTypeCompareResult;
            }

            var otherArrayCreation = (ArrayCreationExpression)other;
            int compareSizesResult =
                UstNodeHelper.CompareCollections(Sizes?.ToArray(), otherArrayCreation.Sizes?.ToArray());
            if (compareSizesResult != 0)
            {
                return compareSizesResult;
            }

            return UstNodeHelper.CompareCollections(
                Initializers?.ToArray(), otherArrayCreation.Initializers?.ToArray());
        }

        public override string ToString()
        {
            return $"new {Type}[{string.Join(", ", Sizes)}]";
        }
    }
}
