using PT.PM.Common;

namespace PT.PM.CSharpParseTreeUst
{
    public static class CSharp
    {
        public readonly static LanguageInfo Language =
            new LanguageInfo("CSharp", ".cs", false, "C#", haveAntlrParser: false);
    }
}
