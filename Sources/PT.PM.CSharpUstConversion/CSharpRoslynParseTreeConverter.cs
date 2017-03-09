using System;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.CSharpAstConversion.RoslynAstVisitor;
using Microsoft.CodeAnalysis;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.CSharpAstConversion
{
    public class CSharpRoslynParseTreeConverter : IParseTreeToUstConverter
    {
        public UstType AstType { get; set; }

        public Language MainLanguage => Language.CSharp;

        public LanguageFlags ConvertedLanguages { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public SemanticsInfo SemanticsInfo { get; set; }

        public CSharpRoslynParseTreeConverter()
        {
            ConvertedLanguages = MainLanguage.GetLanguageWithDependentLanguages();
        }

        public Ust Convert(ParseTree langAst)
        {
            var roslynAst = (CSharpRoslynParseTree)langAst;
            SyntaxTree syntaxTree = roslynAst.SyntaxTree;
            Ust result;
            if (syntaxTree != null)
            {
                string filePath = syntaxTree.FilePath;
                try
                {
                    var visitor = new RoslynAstCommonConverterVisitor(syntaxTree, filePath);
                    if (SemanticsInfo != null)
                    {
                        visitor.SemanticsInfo = (CSharpRoslynSemanticsInfo)SemanticsInfo;
                    }
                    visitor.Logger = Logger;
                    FileNode fileNode = visitor.Walk();
                    
                    result = new MostCommonUst(fileNode, ConvertedLanguages);
                    result.Comments = roslynAst.Comments.Select(c =>
                        new CommentLiteral(c.ToString(), c.GetTextSpan(), fileNode)).ToArray();
                }
                catch (Exception ex)
                {
                    Logger.LogError(string.Format("Conversion error in \"{0}\"", filePath), ex);
                    result = new MostCommonUst();
                }
            }
            else
            {
                result = new MostCommonUst();
            }
            result.FileName = langAst.FileName;

            return result;
        }
    }
}
