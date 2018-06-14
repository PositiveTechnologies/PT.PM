using PT.PM.Common;
using PT.PM.JavaScriptParseTreeUst;

namespace PT.PM.PhpParseTreeUst
{
    public static class Php
    {
        public readonly static Language Language =
            new Language(nameof(Php), new[] { ".php" }, true, "PHP", new [] { JavaScript.Language, Html.Language });
    }
}
