using PT.PM.Common.Files;

namespace PT.PM.Common
{
    public interface ILanguageParser : ILoggable
    {
        ParseTree Parse(TextFile sourceFile);
    }
}
