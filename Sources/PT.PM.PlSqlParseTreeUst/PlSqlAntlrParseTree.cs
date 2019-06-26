using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.PlSqlParseTreeUst
{
    public class PlSqlAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.PlSql;

        public PlSqlAntlrParseTree()
        {
        }

        public PlSqlAntlrParseTree(PlSqlParser.Sql_scriptContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
