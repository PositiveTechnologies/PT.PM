using System;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.JavaAstConversion.Converter;
using PT.PM.AntlrUtils;

namespace PT.PM.JavaAstConversion
{
    public class JavaAntlrParseTreeConverter : AntlrConverter
    {
        public override Language MainLanguage => Language.Java;

        public JavaAntlrParseTreeConverter()
        {
            ConvertedLanguages = Language.Java.GetLanguageWithDependentLanguages();
        }

        protected override FileNode CreateVisitorAndVisit(Antlr4.Runtime.ParserRuleContext ruleContext, string filePath, string fileData, ILogger logger)
        {
            JavaAntlrAstConverterVisitor visitor;
            if (AstType == UstType.Common)
            {
                visitor = new JavaAntlrAstConverterVisitor(filePath, fileData);
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
