using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using Antlr4.Runtime;
using System.Collections.Generic;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptParseTreeConverter: AntlrConverter
    {
        public override Language MainLanguage => Language.JavaScript;

        public JavaScriptParseTreeConverter()
        {
            ConvertedLanguages = Language.JavaScript.GetLanguageWithDependentLanguages();
        }

        protected override FileNode CreateVisitorAndVisit(IList<IToken> tokens, ParserRuleContext ruleContext,
            string filePath, string fileData, ILogger logger)
        {
            JavaScriptAntlrUstConverterVisitor visitor = new JavaScriptAntlrUstConverterVisitor(filePath, fileData);
            visitor.Tokens = tokens;
            visitor.Logger = logger;
            FileNode fileNode = (FileNode)visitor.Visit(ruleContext);
            return fileNode;
        }
    }
}
