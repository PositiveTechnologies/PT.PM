using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.MySqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    class MySqlAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.MySql;

        public MySqlAntlrParseTree()
        {
        }

        public MySqlAntlrParseTree(MySqlParser.RootContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
