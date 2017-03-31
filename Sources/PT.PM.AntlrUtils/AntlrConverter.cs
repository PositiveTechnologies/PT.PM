using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using Antlr4.Runtime;
using System;
using System.Linq;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrConverter : IParseTreeToUstConverter
    {
        public UstType UstType { get; set; }

        public abstract Language MainLanguage { get; }

        public LanguageFlags ConvertedLanguages { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public SemanticsInfo SemanticsInfo { get; set; }

        public AntlrConverter()
        {
            ConvertedLanguages = MainLanguage.GetLanguageWithDependentLanguages();
        }

        protected abstract FileNode CreateVisitorAndVisit(ParserRuleContext ruleContext, string filePath, string fileData, ILogger logger);

        public Ust Convert(ParseTree langParseTree)
        {
            var antlrParseTree = (AntlrParseTree)langParseTree;
            ParserRuleContext tree = antlrParseTree.SyntaxTree;
            ICharStream inputStream = tree.start.InputStream ?? tree.stop?.InputStream;
            string filePath = inputStream != null ? inputStream.SourceName : "";
            Ust result = null;
            FileNode fileNode = null;
            if (tree != null && inputStream != null)
            {
                try
                {
                    fileNode = CreateVisitorAndVisit(tree, filePath, langParseTree.FileData, Logger);
                    result = new MostCommonUst(fileNode, ConvertedLanguages);
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException(filePath, ex));

                    if (result == null)
                    {
                        result = new MostCommonUst();
                        result.Comments = ArrayUtils<CommentLiteral>.EmptyArray;
                    }
                }
            }
            else
            {
                fileNode = new FileNode(filePath, langParseTree.FileData);
                result = new MostCommonUst() { Root = fileNode };
                result.Comments = ArrayUtils<CommentLiteral>.EmptyArray;
            }
            result.FileName = langParseTree.FileName;
            result.Comments = antlrParseTree.Comments.Select(c => new CommentLiteral(c.Text, c.GetTextSpan(), fileNode)).ToArray();
            return result;
        }
    }
}
