using PT.PM.Common.Files;

namespace PT.PM.Common
{
    public interface IMatchResultBase
    {
        string PatternKey { get; }

        bool Suppressed { get; }

        TextFile SourceFile { get; }
    }
}