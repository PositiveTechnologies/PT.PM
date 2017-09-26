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
            MatchingContext match;
            if (ust is AnonymousMethodExpression anonymousMethod)
            {
                match = context;
                if (Parameters.Count != anonymousMethod.Parameters.Count)
                {
                    return match.Fail();
                }

                for (int i = 0; i < Parameters.Count; i++)
                {
                    match = Parameters[i].Match(anonymousMethod.Parameters[i], match);
                    if (!match.Success)
                    {
                        return match;
                    }
                }

                match = Body.Match(anonymousMethod.Body, match);
            }
            else
            {
                match = context.Fail();
            }

            return match.AddUstIfSuccess(ust);
        }
    }
}
