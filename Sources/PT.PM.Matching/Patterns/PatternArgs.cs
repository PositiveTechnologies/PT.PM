using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternArgs : PatternBase
    {
        public List<PatternBase> Args { get; set; } = new List<PatternBase>();

        public PatternArgs()
        {
        }

        public PatternArgs(params PatternBase[] args)
        {
            Args = args.ToList();
        }

        public PatternArgs(IEnumerable<PatternBase> args)
        {
            Args = args.ToList();
        }

        public override Ust[] GetChildren() => Args.ToArray();

        public override string ToString() => string.Join(", ", Args);

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is ArgsUst argsUst)
            {
                List<Expression> otherArgs = argsUst.Collection;

                newContext = context;
                if (Args.Count != 1 || !(Args[0] is PatternMultipleExpressions))
                {
                    if (Args.Count != otherArgs.Count)
                    {
                        return newContext.Fail();
                    }

                    for (int i = 0; i < Args.Count; i++)
                    {
                        newContext = Args[i].Match(otherArgs[i], newContext);
                        if (!newContext.Success)
                        {
                            return newContext;
                        }
                    }
                }
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddMatchIfSuccess(ust);
        }
    }
}
