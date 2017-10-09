using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PT.PM.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PT.PM.Common.Exceptions;

namespace PT.PM.CSharpParseTreeUst
{
    public class CSharpRoslynParser : ILanguageParser
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => CSharp.Language;

        public ParseTree Parse(SourceCodeFile sourceCodeFile)
        {
            CSharpRoslynParseTree result = null;

            var filePath = Path.Combine(sourceCodeFile.RelativePath, sourceCodeFile.Name);
            if (sourceCodeFile.Code != null)
            {
                SyntaxNode root = null;
                try
                {
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCodeFile.Code, null, filePath);
                    result = new CSharpRoslynParseTree(syntaxTree);
                    root = syntaxTree.GetRoot();
                    result.Comments = root.DescendantTrivia().Where(node =>
                    {
                        SyntaxKind kind = node.Kind();
                        return kind == SyntaxKind.SingleLineCommentTrivia ||
                               kind == SyntaxKind.MultiLineCommentTrivia ||
                               kind == SyntaxKind.SingleLineDocumentationCommentTrivia ||
                               kind == SyntaxKind.MultiLineDocumentationCommentTrivia ||
                               kind == SyntaxKind.DocumentationCommentExteriorTrivia ||
                               kind == SyntaxKind.XmlComment;
                    }).ToArray();

                    IEnumerable<Diagnostic> diagnostics = root.GetDiagnostics();
                    foreach (var diagnostic in diagnostics)
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error &&
                            diagnostic.Id != "CS1029")
                        {
                            var textSpan = diagnostic.Location.ToTextSpan();
                            Logger.LogError(new ParsingException(filePath, message: diagnostic.ToString())
                            {
                                TextSpan = textSpan
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ParsingException(filePath, ex));
                    result = new CSharpRoslynParseTree();
                }
            }
            else
            {
                result = new CSharpRoslynParseTree();
            }
            result.SourceCodeFile = sourceCodeFile;

            return result;
        }

        public void ClearCache()
        {
        }
    }
}
