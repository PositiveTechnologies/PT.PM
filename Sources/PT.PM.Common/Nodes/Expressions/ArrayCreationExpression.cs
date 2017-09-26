using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Expressions
{
    public class ArrayCreationExpression : Expression
    {
        public TypeToken Type { get; set; }

        public List<Expression> Sizes { get; set; }

        public List<Expression> Initializers { get; set; }

        public ArrayCreationExpression(TypeToken type, IEnumerable<Expression> sizes, IEnumerable<Expression> inits,
            TextSpan textSpan)
            : base(textSpan)
        {
            Type = type;
            Sizes = sizes as List<Expression> ?? sizes?.ToList() ?? new List<Expression>();
            Initializers = inits as List<Expression> ?? inits?.ToList() ?? new List<Expression>();
        }

        public ArrayCreationExpression()
        {
            Sizes = new List<Expression>();
            Initializers = new List<Expression>();
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
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

        public override Expression[] GetArgs()
        {
            var result = new List<Expression> { Type };
            result.AddRange(Sizes);
            return result.ToArray();
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            var nodeTypeCompareResult = Kind - other.Kind;
            if (nodeTypeCompareResult != 0)
            {
                return nodeTypeCompareResult;
            }

            var otherArrayCreation = (ArrayCreationExpression)other;
            int compareSizesResult = Sizes.CompareTo(otherArrayCreation.Sizes);
            if (compareSizesResult != 0)
            {
                return compareSizesResult;
            }

            return Initializers.CompareTo(otherArrayCreation.Initializers);
        }

        public override string ToString()
        {
            return $"new {Type}[{string.Join(", ", Sizes)}]";
        }
    }
}
