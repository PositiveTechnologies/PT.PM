using System;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.PhpParseTreeUst;
using PT.PM.AntlrUtils;
using Antlr4.Runtime;
using System.Collections.Generic;

namespace PT.PM.ParseTreeUst
{
    public class PhpAntlrParseTreeConverter : AntlrConverter
    {
        public override Language MainLanguage => Language.Php;

        protected override FileNode CreateVisitorAndVisit(IList<IToken> tokens, ParserRuleContext ruleContext,
            string filePath, string fileData, ILogger logger)
        {
            var visitor = new PhpAntlrParseTreeConverterVisitor(filePath, fileData);
            visitor.Tokens = tokens;
            visitor.ConvertedLanguages = ConvertedLanguages;
            visitor.Logger = logger;
            FileNode fileNode = (FileNode)visitor.Visit(ruleContext);
            return fileNode;
        }
    }
}
