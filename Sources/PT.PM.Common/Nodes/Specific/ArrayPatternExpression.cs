using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Specific
{
    public class ArrayPatternExpression : Expression
    {
        public List<Parameter> Elements { get; set; } = new List<Parameter>();

        public ArrayPatternExpression(IEnumerable<Parameter> elements, TextSpan textSpan)
            : base(textSpan)
        {
            Elements = elements as List<Parameter> ?? elements.ToList();
        }

        public ArrayPatternExpression()
        {
        }

        public override Expression[] GetArgs() => new Expression[] { this };

        public override Ust[] GetChildren() => Elements.ToArray();

        public override string ToString() => $"[ {string.Join(", ", Elements).ToStringWithTrailSpace()}]";
    }
}
