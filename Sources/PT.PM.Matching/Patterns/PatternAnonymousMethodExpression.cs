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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.AnonymousMethodExpression)
            {
                return context.Fail();
            }

            var anonymousMethodExpression = (AnonymousMethodExpression)ust;
            if (Parameters.Count != anonymousMethodExpression.Parameters.Count)
            {
                return context.Fail();
            }

            MatchingContext match = context;
            for (int i = 0; i < Parameters.Count; i++)
            {
                match = Parameters[i].Match(anonymousMethodExpression.Parameters[i], match);
                if (!match.Success)
                {
                    return match;
                }
            }

            match = Body.Match(anonymousMethodExpression.Body, match);
            return match;
        }
    }
}
