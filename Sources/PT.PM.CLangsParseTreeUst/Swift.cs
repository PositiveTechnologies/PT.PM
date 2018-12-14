using PT.PM.Common;

namespace PT.PM.CLangsParseTreeUst
{
    public static class Swift
    {
        public readonly static Language Language =
            new Language(nameof(Swift), new[] { ".swift" }, false, "Swift", haveAntlrParser: false);
    }
}
