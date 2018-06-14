using PT.PM.Common;

namespace PT.PM.CLangsParseTreeUst
{
    public static class Cplusplus
    {
        public readonly static Language Language =
            new Language("Cplusplus", new[] { ".cpp", ".hpp", ".cc", ".cxx" }, false, "Cplusplus", new[] { C.Language });
    }
}