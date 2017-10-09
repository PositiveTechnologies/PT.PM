using PT.PM.Common;

namespace PT.PM.SqlParseTreeUst
{
    public static class TSql
    {
        public readonly static Language Language =
            new Language("TSql", ".sql", true, "T-SQL", isSql: true);
    }
}
