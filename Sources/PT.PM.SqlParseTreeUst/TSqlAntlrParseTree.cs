using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.TSqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    public class TSqlAntlrParseTree : AntlrParseTree
    {
        public override LanguageInfo SourceLanguage => TSql.Language;

        public TSqlAntlrParseTree()
        {
        }

        public TSqlAntlrParseTree(TSqlParser.Tsql_fileContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
