using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using Antlr4.Runtime;

namespace PT.PM.JavaScriptAstConversion
{
    public class JavaScriptParseTreeConverter: AntlrConverter
    {
        public override Language MainLanguage => Language.JavaScript;

        public JavaScriptParseTreeConverter()
        {
            ConvertedLanguages = Language.JavaScript.GetLanguageWithDependentLanguages();
        }

        protected override FileNode CreateVisitorAndVisit(ParserRuleContext ruleContext, string filePath, string fileData, ILogger logger)
        {
            JavaScriptAntlrAstConverterVisitor visitor = new JavaScriptAntlrAstConverterVisitor(filePath, fileData);
            visitor.Logger = logger;
            FileNode fileNode = (FileNode)visitor.Visit(ruleContext);
            return fileNode;
        }
    }
}
