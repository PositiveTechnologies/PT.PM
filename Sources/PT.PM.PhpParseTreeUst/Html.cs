using PT.PM.Common;
using PT.PM.JavaScriptParseTreeUst;

namespace PT.PM.PhpParseTreeUst
{
    public static class Html
    {
        public readonly static LanguageInfo Language =
            new LanguageInfo("Html", ".html", true, "HTML", new[] { JavaScript.Language });
    }
}
