using PT.PM.Common;

namespace PT.PM.SqlParseTreeUst
{
    public static class MySql
    {
        public readonly static Language Language =
            new Language(nameof(MySql), ".sql", true, "MySql", isSql: true);
    }
}
