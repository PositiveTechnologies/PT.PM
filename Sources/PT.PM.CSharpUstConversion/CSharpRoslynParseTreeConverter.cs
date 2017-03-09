using System;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.CSharpUstConversion.RoslynUstVisitor;
using Microsoft.CodeAnalysis;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.CSharpUstConversion
{
    public class CSharpRoslynParseTreeConverter : IParseTreeToUstConverter
    {
        public UstType UstType { get; set; }

        public Language MainLanguage => Language.CSharp;

        public LanguageFlags ConvertedLanguages { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public SemanticsInfo SemanticsInfo { get; set; }

        public CSharpRoslynParseTreeConverter()
        {
            ConvertedLanguages = MainLanguage.GetLanguageWithDependentLanguages();
        }

        public Ust Convert(ParseTree langParseTree)
        {
            var roslynParseTree = (CSharpRoslynParseTree)langParseTree;
            SyntaxTree syntaxTree = roslynParseTree.SyntaxTree;
            Ust result;
            if (syntaxTree != null)
            {
                string filePath = syntaxTree.FilePath;
                try
                {
                    var visitor = new RoslynUstCommonConverterVisitor(syntaxTree, filePath);
                    if (SemanticsInfo != null)
                    {
                        visitor.SemanticsInfo = (CSharpRoslynSemanticsInfo)SemanticsInfo;
                    }
                    visitor.Logger = Logger;
                    FileNode fileNode = visitor.Walk();
                    
                    result = new MostCommonUst(fileNode, ConvertedLanguages);
                    result.Comments = roslynParseTree.Comments.Select(c =>
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
            result.FileName = langParseTree.FileName;

            return result;
        }
    }
}
