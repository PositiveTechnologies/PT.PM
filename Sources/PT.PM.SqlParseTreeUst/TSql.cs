using PT.PM.Common;

namespace PT.PM.SqlParseTreeUst
{
    public static class TSql
    {
        public readonly static LanguageInfo Language =
            new LanguageInfo("TSql", ".sql", true, "T-SQL", isSql: true);
    }
}
