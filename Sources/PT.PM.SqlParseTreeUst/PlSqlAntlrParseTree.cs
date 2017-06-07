using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.PlSqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.PlSql;

        public PlSqlAntlrParseTree()
        {
        }

        public PlSqlAntlrParseTree(PlSqlParser.Compilation_unitContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
