using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using Antlr4.Runtime;
using System;

namespace PT.PM.SqlAstConversion
{
    public class TSqlAntlrConverter : AntlrConverter
    {
        public override Language MainLanguage => Language.TSql;

        protected override FileNode CreateVisitorAndVisit(ParserRuleContext ruleContext, string filePath, string fileData, ILogger logger)
        {
            TSqlConverterVisitor visitor;
            if (AstType == Common.Ust.UstType.Common)
            {
                visitor = new TSqlConverterVisitor(filePath, fileData);
            }
            else
            {
                throw new NotImplementedException();
            }

            visitor.Logger = logger;
            var fileNode = (FileNode)visitor.Visit(ruleContext);
            return fileNode;
        }
    }
}
