using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.PlSqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => PlSql.Language;

        public PlSqlAntlrParseTree()
        {
        }

        public PlSqlAntlrParseTree(PlSqlParser.Sql_scriptContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
