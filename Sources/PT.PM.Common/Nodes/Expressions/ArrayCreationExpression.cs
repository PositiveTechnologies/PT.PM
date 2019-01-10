using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class ArrayCreationExpression : Expression
    {
        [Key(UstFieldOffset)]
        public IdToken Type { get; set; }

        [Key(UstFieldOffset + 1)]
        public TypeToken KeywordNew { get; set; }

        [Key(UstFieldOffset + 2)]
        public List<Expression> Sizes { get; set; }

        [Key(UstFieldOffset + 3)]
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
            if (KeywordNew != null)
            {
                result.Add(KeywordNew);
            }
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
            var result = new List<Expression>();
            if (KeywordNew != null)
            {
                result.Add(KeywordNew);
            }
            result.Add(Type);
            result.AddRange(Sizes);
            return result.ToArray();
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return KindId;
            }

            var nodeTypeCompareResult = KindId - other.KindId;
            if (nodeTypeCompareResult != 0)
            {
                return nodeTypeCompareResult;
            }

            var otherArrayCreation = (ArrayCreationExpression)other;

            int compareKeywordNewResult = (KeywordNew == null && otherArrayCreation.KeywordNew == null) ? 0 : KeywordNew?.CompareTo(otherArrayCreation.KeywordNew) ?? -1;

            if (compareKeywordNewResult != 0)
            {
                return compareKeywordNewResult;
            }

            int compareSizesResult = Sizes.CompareTo(otherArrayCreation.Sizes);
            if (compareSizesResult != 0)
            {
                return compareSizesResult;
            }

            return Initializers.CompareTo(otherArrayCreation.Initializers);
        }

        public override string ToString()
        {
            string keywordNew = KeywordNew == null ? "" : "new";
            return $"{keywordNew} {Type}[{string.Join(", ", Sizes)}]";
        }
    }
}
