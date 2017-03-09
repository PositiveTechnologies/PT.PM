using System;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.PhpUstConversion;
using PT.PM.AntlrUtils;

namespace PT.PM.UstConversion
{
    public class PhpAntlrParseTreeConverter : AntlrConverter
    {
        public override Language MainLanguage => Language.Php;

        protected override FileNode CreateVisitorAndVisit(Antlr4.Runtime.ParserRuleContext ruleContext, string filePath, string fileData, ILogger logger)
        {
            PhpAntlrParseTreeConverterVisitor visitor;
            if (UstType == UstType.Common)
            {
                visitor = new PhpAntlrParseTreeConverterVisitor(filePath, fileData);
                visitor.ConvertedLanguages = ConvertedLanguages;
            }
            else
            {
                throw new NotImplementedException();
            }

            visitor.Logger = logger;
            FileNode fileNode = (FileNode)visitor.Visit(ruleContext);
            return fileNode;
        }
    }
}
