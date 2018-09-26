using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Specific
{
    public class ArrayPatternExpression : Expression
    {
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
