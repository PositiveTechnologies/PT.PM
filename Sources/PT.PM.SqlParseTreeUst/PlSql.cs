using PT.PM.Common;

namespace PT.PM.SqlParseTreeUst
{
    public static class PlSql
    {
        public readonly static LanguageInfo Language =
             new LanguageInfo("PlSql", new[] { ".sql", ".pks", ".pkb", ".tps", ".vw" }, true, "PL/SQL", isSql: true);
    }
}
