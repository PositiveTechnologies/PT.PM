using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;

namespace PT.PM.SqlParseTreeUst
{
    public class TSqlAntlrConverter : AntlrConverter
    {
        public override Language MainLanguage => Language.TSql;

        protected override FileNode CreateVisitorAndVisit(IList<IToken> tokens, ParserRuleContext ruleContext,
            string filePath, string fileData, ILogger logger)
        {
            var visitor = new TSqlConverterVisitor(filePath, fileData);
            visitor.Tokens = tokens;
            visitor.Logger = logger;
            var fileNode = (FileNode)visitor.Visit(ruleContext);
            return fileNode;
        }
    }
}
