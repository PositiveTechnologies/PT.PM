using PT.PM.Common;

namespace PT.PM.CSharpParseTreeUst
{
    public static class Aspx
    {
        public readonly static Language Language =
            new Language("Aspx", new[] { ".asax", ".aspx", ".ascx", ".master" },
                false, "Aspx", new[] { CSharp.Language }, false, false);
    }
}
