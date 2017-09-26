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

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.ArgsUst)
            {
                return false;
            }

            List<Expression> otherArgs = ((ArgsUst)ust).Collection;

            if (Args.Count != otherArgs.Count)
            {
                return false;
            }

            for (int i = 0; i < Args.Count; i++)
            {
                bool match = Args[i].Match(otherArgs[i], context);
                if (!match)
                {
                    return match;
                }
            }

            return true;
        }
    }
}
