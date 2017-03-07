﻿using PT.PM.AntlrUtils;
using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using Antlr4.Runtime;
using PT.PM.SqlAstConversion.Parser;

namespace PT.PM.SqlAstConversion
{
    public class PlSqlAntlrConverter : AntlrConverter
    {
        public override Language MainLanguage => Language.PlSql;

        protected override FileNode CreateVisitorAndVisit(ParserRuleContext ruleContext, string filePath, string fileData, ILogger logger)
        {
            PlSqlConverterVisitor visitor;
            if (AstType == Common.Ust.UstType.Common)
            {
                visitor = new PlSqlConverterVisitor(filePath, fileData);
            }
            else
            {
                throw new NotImplementedException();
            }

            visitor.Logger = logger;
            var fileNode = (FileNode)visitor.Visit((plsqlParser.Compilation_unitContext)ruleContext);
            return fileNode;
        }
    }
}
