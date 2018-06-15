using PT.PM.Common;

namespace PT.PM.CLangsParseTreeUst
{
    public static class CPlusPlus
    {
        public readonly static Language Language =
            new Language(nameof(CPlusPlus), new[] { ".cpp", ".hpp", ".cc", ".cxx" }, false, "C++", new[] { C.Language }, haveAntlrParser: false);
    }
}