using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternAny : PatternUst<Ust>
    {
        public static PatternAny Instance = new PatternAny();

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            if (ust == null)
            {
                return context.MakeSuccess();
            }

            return context.AddMatch(ust);
        }
    }
}
