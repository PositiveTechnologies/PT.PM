using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternArgs : PatternUst
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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is ArgsUst argsUst)
            {
                List<Expression> args = argsUst.Collection;

                newContext = MatchingContext.CreateWithInputParamsAndVars(context);
                var matchedTextSpans = new List<TextSpan>();
                int patternArgInd = 0;
                int argInd = 0;
                while (argInd < args.Count)
                {
                    if (patternArgInd >= Args.Count)
                    {
                        break;
                    }

                    newContext = MatchingContext.CreateWithInputParamsAndVars(newContext);
                    if (Args[patternArgInd] is PatternMultipleExpressions multiExprArg)
                    {
                        if (patternArgInd + 1 < Args.Count)
                        {
                            newContext = Args[patternArgInd + 1].Match(args[argInd], newContext);
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
                        newContext = Args[patternArgInd].Match(args[argInd], newContext);
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
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddUstIfSuccess(ust);
        }
    }
}
