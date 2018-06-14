using PT.PM.Common;

namespace PT.PM.CLangsParseTreeUst
{
    public static class C
    {
        public readonly static Language Language =
            new Language(nameof(C), new[] { ".c", ".h" }, false, "C", haveAntlrParser: false);
    }
}