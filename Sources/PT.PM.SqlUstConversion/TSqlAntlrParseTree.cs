using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.TSqlAstConversion.Parser;

namespace PT.PM.SqlAstConversion
{
    public class TSqlAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.TSql;

        public TSqlAntlrParseTree()
        {
        }

        public TSqlAntlrParseTree(tsqlParser.Tsql_fileContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
