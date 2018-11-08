using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.MySqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    class MySqlAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => MySql.Language;

        public MySqlAntlrParseTree()
        {
        }

        public MySqlAntlrParseTree(MySqlParser.RootContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
