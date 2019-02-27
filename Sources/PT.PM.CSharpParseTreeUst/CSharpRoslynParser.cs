using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PT.PM.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PT.PM.Common.Exceptions;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.CSharpParseTreeUst
{
    public class CSharpRoslynParser : ILanguageParser<TextFile>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => Language.CSharp;

        public static CSharpRoslynParser Create() => new CSharpRoslynParser();

        public ParseTree Parse(TextFile sourceFile, out TimeSpan parserTimeSpan)
        {
            if (sourceFile.Data == null)
            {
                return null;
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var filePath = Path.Combine(sourceFile.RelativePath, sourceFile.Name);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceFile.Data, null, filePath);
                var result = new CSharpRoslynParseTree(syntaxTree);
                SyntaxNode root = syntaxTree.GetRoot();
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
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error &&
                        diagnostic.Id != "CS1029")
                    {
                        var textSpan = diagnostic.Location.ToTextSpan();
                        Logger.LogError(new ParsingException(sourceFile, message: diagnostic.ToString())
                        {
                            TextSpan = textSpan
                        });
                    }
                }
                stopwatch.Stop();
                parserTimeSpan = stopwatch.Elapsed;

                result.SourceFile = sourceFile;
                return result;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(sourceFile, ex));
                return null;
            }
        }
    }
}
