using PT.PM.AntlrUtils;
using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using Antlr4.Runtime;
using PT.PM.PlSqlParseTreeUst;
using System.Collections.Generic;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrConverter : AntlrConverter
    {
        public override Language MainLanguage => Language.PlSql;

        protected override FileNode CreateVisitorAndVisit(IList<IToken> tokens, ParserRuleContext ruleContext,
            string filePath, string fileData, ILogger logger)
        {
            var visitor = new PlSqlConverterVisitor(filePath, fileData);
            visitor.Tokens = tokens;
            visitor.Logger = logger;
            var fileNode = (FileNode)visitor.Visit((PlSqlParser.Compilation_unitContext)ruleContext);
            return fileNode;
        }
    }
}
