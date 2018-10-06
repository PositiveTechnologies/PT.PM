using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternArgs : PatternUst<ArgsUst>
    {
        public List<PatternUst> Args { get; set; } = new List<PatternUst>();

        public PatternArgs()
        {
        }

        public PatternArgs(params PatternUst[] args)
        {
            Args = args.ToList();
        }

        public PatternArgs(IEnumerable<PatternUst> args)
        {
            Args = args.ToList();
        }

        public override string ToString() => string.Join(", ", Args);

        public override MatchContext Match(ArgsUst argsUst, MatchContext context)
        {
            MatchContext newContext;

            List<Expression> args = argsUst.Collection;
            newContext = MatchContext.CreateWithInputParamsAndVars(context);
            var matchedTextSpans = new List<TextSpan>();
            int patternArgInd = 0;
            int argInd = 0;
            while (argInd < args.Count)
            {
                if (patternArgInd >= Args.Count)
                {
                    break;
                }

                newContext = MatchContext.CreateWithInputParamsAndVars(newContext);
                if (Args[patternArgInd] is PatternMultipleExpressions multiExprArg)
                {
                    if (patternArgInd + 1 < Args.Count)
                    {
                        Expression arg = UstUtils.GetArgWithoutModifier(args[argInd]);
                        newContext = Args[patternArgInd + 1].MatchUst(arg, newContext);
                        matchedTextSpans.AddRange(newContext.Locations);
                        if (newContext.Success)
                        {
                            patternArgInd += 2;
                        }
                    }
                    else
                    {
                        matchedTextSpans.AddRange(newContext.Locations);
                    }
                    argInd += 1;
                }
                else
                {
                    Expression arg = UstUtils.GetArgWithoutModifier(args[argInd]);
                    newContext = Args[patternArgInd].MatchUst(arg, newContext);
                    if (!newContext.Success)
                    {
                        break;
                    }
                    else
                    {
                        matchedTextSpans.AddRange(newContext.Locations);
                    }
                    patternArgInd += 1;
                    argInd += 1;
                }
            }

            if (patternArgInd < Args.Count && Args[patternArgInd] is PatternMultipleExpressions)
            {
                patternArgInd += 1;
            }

            if (argInd != args.Count || patternArgInd != Args.Count)
            {
                newContext = context.Fail();
            }
            else
            {
                newContext = context.AddMatches(matchedTextSpans);
            }

            return newContext.AddUstIfSuccess(argsUst);
        }
    }
}
