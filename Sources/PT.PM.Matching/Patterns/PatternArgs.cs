using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternArgs : PatternBase, IPatternUst
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
            MatchingContext match;

            if (ust is ArgsUst argsUst)
            {
                List<Expression> otherArgs = argsUst.Collection;

                match = context;
                if (Args.Count != otherArgs.Count)
                {
                    return match.Fail();
                }

                for (int i = 0; i < Args.Count; i++)
                {
                    match = Args[i].Match(otherArgs[i], match);
                    if (!match.Success)
                    {
                        return match;
                    }
                }
            }
            else
            {
                match = context.Fail();
            }

            return match;
        }
    }
}
