using PT.PM.Common;
using PT.PM.JavaScriptParseTreeUst;

namespace PT.PM.PhpParseTreeUst
{
    public static class Html
    {
        public readonly static Language Language =
            new Language("Html", ".html", true, "HTML", new[] { JavaScript.Language });
    }
}
