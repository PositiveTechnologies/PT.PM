using PT.PM.AntlrUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.PM.Common;
using PT.PM.SqlUstConversion.Parser;

namespace PT.PM.SqlUstConversion
{
    public class PlSqlAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.PlSql;

        public PlSqlAntlrParseTree()
        {
        }

        public PlSqlAntlrParseTree(plsqlParser.Compilation_unitContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
