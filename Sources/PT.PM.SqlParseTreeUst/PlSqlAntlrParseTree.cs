using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.PlSqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrParseTree : AntlrParseTree
    {
        public override LanguageInfo SourceLanguage => PlSql.Language;

        public PlSqlAntlrParseTree()
        {
        }

        public PlSqlAntlrParseTree(PlSqlParser.Compilation_unitContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
