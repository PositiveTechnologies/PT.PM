using System;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.JavaParseTreeUst.Converter;
using PT.PM.AntlrUtils;
using Antlr4.Runtime;
using System.Collections.Generic;

namespace PT.PM.JavaParseTreeUst
{
    public class JavaAntlrParseTreeConverter : AntlrConverter
    {
        public override Language MainLanguage => Language.Java;

        public JavaAntlrParseTreeConverter()
        {
            ConvertedLanguages = Language.Java.GetLanguageWithDependentLanguages();
        }

        protected override FileNode CreateVisitorAndVisit(IList<IToken> tokens, ParserRuleContext ruleContext,
            string filePath, string fileData, ILogger logger)
        {
            JavaAntlrUstConverterVisitor visitor;
            if (UstType == UstType.Common)
            {
                visitor = new JavaAntlrUstConverterVisitor(filePath, fileData);
                visitor.Tokens = tokens;
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
