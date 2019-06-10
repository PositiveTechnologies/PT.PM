using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    public class ArrayPatternExpression : Expression
    {
        [Key(0)] public override UstType UstType => UstType.ArrayPatternExpression;

        [Key(UstFieldOffset)]
        public List<ParameterDeclaration> Elements { get; set; } = new List<ParameterDeclaration>();

        public ArrayPatternExpression(IEnumerable<ParameterDeclaration> elements, TextSpan textSpan)
            : base(textSpan)
        {
            Elements = elements as List<ParameterDeclaration> ?? elements.ToList();
        }

        public ArrayPatternExpression()
        {
        }

        public override Expression[] GetArgs() => new Expression[] { this };

        public override Ust[] GetChildren() => Elements.ToArray();

        public override string ToString() => $"[ {string.Join(", ", Elements).ToStringWithTrailSpace()}]";
    }
}
