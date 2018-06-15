using PT.PM.Common;

namespace PT.PM.JavaParseTreeUst
{
    public static class Java
    {
        public readonly static Language Language =
            new Language(nameof(Java), ".java", false, "Java");
    }
}
