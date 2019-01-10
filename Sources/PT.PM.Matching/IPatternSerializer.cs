using PT.PM.Common;
using PT.PM.Common.Files;

namespace PT.PM.Matching
{
    public interface IPatternSerializer : ILoggable
    {
        string Format { get; }

        string Serialize(PatternRoot patternRoot);

        PatternRoot Deserialize(CodeFile data);
    }
}
