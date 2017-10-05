using PT.PM.Common;

namespace PT.PM.CSharpParseTreeUst
{
    public static class Aspx
    {
        public readonly static LanguageInfo Language =
            new LanguageInfo("Aspx", new[] { ".asax", ".aspx", ".ascx", ".master" }, false, "Aspx", new[] { CSharp.Language }, isPattern: false);
    }
}
