using PT.PM.Common;

namespace PT.PM.CLangsParseTreeUst
{
    public static class C
    {
        public readonly static Language Language =
            new Language("C", new[] { ".c", ".h" }, false, "C");
    }
}