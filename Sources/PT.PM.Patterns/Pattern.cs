using PT.PM.Patterns.Nodes;

namespace PT.PM.Patterns
{
    public class Pattern : PatternBase
    {
        public PatternNode Data { get; set; }

        public Pattern(PatternDto patternDto, PatternNode data)
            : base(patternDto.Key, patternDto.Description, patternDto.Languages)
        {
            Data = data;
        }

        public Pattern()
        {
        }
    }
}
