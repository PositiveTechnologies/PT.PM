using PT.PM.Common;
using PT.PM.JavaScriptParseTreeUst;

namespace PT.PM.PhpParseTreeUst
{
    public static class Php
    {
        public readonly static LanguageInfo Language =
            new LanguageInfo("Php", new[] { ".php" }, true, "PHP", new [] { JavaScript.Language, Html.Language });
    }
}
