using PT.PM.Common;
using System.Collections.Generic;

namespace PT.PM.SqlParseTreeUst.Tests
{
    internal static class SqlTestUtils
    {
        internal static readonly Dictionary<string, Language> Languages = new Dictionary<string, Language>
        {
            ["mysql"] = MySql.Language,
            ["plsql"] = PlSql.Language,
            ["tsql"] = TSql.Language,
        };
    }
}
