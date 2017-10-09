using PT.PM.Common;

namespace PT.PM.CSharpParseTreeUst
{
    public static class CSharp
    {
        public readonly static Language Language =
            new Language("CSharp", ".cs", false, "C#", haveAntlrParser: false);
    }
}
