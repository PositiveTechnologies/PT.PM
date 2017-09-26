using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternAnonymousMethodExpression : PatternBase
    {
        public List<PatternParameterDeclaration> Parameters { get; set; } = new List<PatternParameterDeclaration>();

        public PatternStatements Body { get; set; }

        public PatternAnonymousMethodExpression()
        {
        }

        public PatternAnonymousMethodExpression(IEnumerable<PatternParameterDeclaration> parameters, PatternStatements body,
            TextSpan textSpan)
            : base(textSpan)
        {
            Parameters = parameters as List<PatternParameterDeclaration> ?? parameters.ToList();
            Body = body;
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Parameters);
            result.Add(Body);
            return result.ToArray();
        }

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.AnonymousMethodExpression)
            {
                return false;
            }

            var anonymousMethodExpression = (AnonymousMethodExpression)ust;
            if (Parameters.Count != anonymousMethodExpression.Parameters.Count)
            {
                return false;
            }

            for (int i = 0; i < Parameters.Count; i++)
            {
                if (!Parameters[i].Match(anonymousMethodExpression.Parameters[i], context))
                {
                    return false;
                }
            }

            if (!Body.Match(anonymousMethodExpression.Body, context))
            {
                return false;
            }

            return true;
        }
    }
}
