using System;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.CSharpParseTreeUst.RoslynUstVisitor;
using Microsoft.CodeAnalysis;
using PT.PM.Common.Exceptions;

namespace PT.PM.CSharpParseTreeUst
{
    public class CSharpRoslynParseTreeConverter : IParseTreeToUstConverter
    {
        public Language MainLanguage => Language.CSharp;

        public LanguageFlags ConvertedLanguages { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

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
                    visitor.Logger = Logger;
                    FileNode fileNode = visitor.Walk();
                    
                    result = new Ust(fileNode, ConvertedLanguages);
                    result.Comments = roslynParseTree.Comments.Select(c =>
                        new Common.Nodes.Tokens.Literals.CommentLiteral(c.ToString(), c.GetTextSpan(), fileNode)).ToArray();
                    result.Root.FillParents();
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException(filePath, ex));
                    result = new Ust();
                }
            }
            else
            {
                result = new Ust();
            }
            result.FileName = langParseTree.FileName;

            return result;
        }
    }
}
