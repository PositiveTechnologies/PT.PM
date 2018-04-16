using PT.PM.Common;

namespace PT.PM.SqlParseTreeUst
{
    public static class PlSql
    {
        public readonly static Language Language =
             new Language("PlSql", new[] { ".sql", ".pks", ".pkb", ".tps", ".vw" }, true, "PL/SQL", isSql: true);
    }
}
