using Esprima.Ast;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using UstTokens = PT.PM.Common.Nodes.Tokens;
using UstExprs = PT.PM.Common.Nodes.Expressions;
using UstSpecific = PT.PM.Common.Nodes.Specific;
using PT.PM.Common.Nodes.TypeMembers;
using Esprima;
using System.Threading;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.JavaScriptParseTreeUst
{
    public partial class JavaScriptEsprimaParseTreeConverter : IParseTreeToUstConverter
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => JavaScript.Language;

        public TextFile SourceFile { get; set; }

        public HashSet<Language> AnalyzedLanguages { get; set; }

        public RootUst ParentRoot { get; set; }

        public int Offset { get; set; }

        public RootUst Convert(ParseTree langParseTree)
        {
            try
            {
                var esprimaParseTree = (JavaScriptEsprimaParseTree)langParseTree;
                if (SourceFile == null)
                {
                    SourceFile = esprimaParseTree.SourceFile;
                }
                var program = VisitProgram(esprimaParseTree.SyntaxTree);

                var rootUst = new RootUst(SourceFile, JavaScript.Language)
                {
                    Nodes = new Ust[] { program },
                };

                var comments = new List<CommentLiteral>(esprimaParseTree.Comments.Count);
                foreach (Comment comment in esprimaParseTree.Comments)
                {
                    TextSpan textSpan = GetTextSpan(comment);
                    comments.Add(new CommentLiteral(SourceFile.GetSubstring(textSpan), textSpan)
                    {
                        Root = rootUst,
                    });
                }

                rootUst.Comments = comments.ToArray();
                rootUst.Root = ParentRoot;
                rootUst.TextSpan = GetTextSpan(esprimaParseTree.SyntaxTree);
                rootUst.FillAscendants();

                return rootUst;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ConversionException(SourceFile, ex));
                return null;
            }
        }

        private Ust Visit(INode node)
        {
            try
            {
                if (node is Statement statement)
                    return VisitStatement(statement);
                else if (node is Expression expression)
                    return VisitExpression(expression);
                else if (node is ArrayPattern arrayPattern)
                    return VisitArrayPattern(arrayPattern);
                else if (node is ObjectPattern objectPattern)
                    return VisitObjectPattern(objectPattern);
                else
                    return VisitUnknownNode(node);
            }
            catch (Exception ex)
            {
                Logger?.LogError(new ConversionException(SourceFile, ex));
                return null;
            }
        }

        private UstSpecific.ArrayPatternExpression VisitArrayPattern(ArrayPattern arrayPattern)
        {
            var elements = new List<ParameterDeclaration>(arrayPattern.Elements.Count);

            foreach (ArrayPatternElement elem in arrayPattern.Elements)
            {
                ParameterDeclaration paramDecl = VisitArrayPatternElement(elem);
                elements.Add(paramDecl);
            }

            return new UstSpecific.ArrayPatternExpression(elements, GetTextSpan(arrayPattern));
        }

        private ParameterDeclaration VisitArrayPatternElement(ArrayPatternElement arrayPatternElement)
        {
            ParameterDeclaration paramDecl;

            if (arrayPatternElement is Identifier identifier)
            {
                var name = VisitIdentifier(identifier);
                paramDecl = new ParameterDeclaration(null, null, name, name.TextSpan);
            }
            else if (arrayPatternElement is AssignmentPattern assignmentPattern)
            {
                var assignExpr = VisitAssignmentPattern(assignmentPattern);
                UstTokens.IdToken name;

                if (assignExpr.Left is UstTokens.IdToken idToken)
                {
                    name = idToken;
                }
                else
                {
                    Logger?.LogDebug($"Not {nameof(UstTokens.IdToken)} nodes are not support as parameter names"); // TODO
                    name = null;
                }

                paramDecl = new ParameterDeclaration(null, null, name, assignExpr.TextSpan)
                {
                    Initializer = assignExpr.Right
                };
            }
            else if (arrayPatternElement == null)
            {
                paramDecl = null;
            }
            else
            {
                // TODO: other types
                paramDecl = null;
            }

            return paramDecl;
        }

        private UstExprs.AnonymousObjectExpression VisitObjectPattern(ObjectPattern objectPattern)
        {
            // TODO: maybe add new UST type ObjectPattern?
            var properties = VisitProperties(objectPattern.Properties);
            return new UstExprs.AnonymousObjectExpression(properties, GetTextSpan(objectPattern));
        }

        private Ust VisitUnknownNode(INode node)
        {
            string message = node == null
                ? $"{nameof(node)} can not be null"
                : $"Unknow {nameof(INode)} type {node.Type}";
            Logger.LogError(new ConversionException(SourceFile, message: message));
            return null;
        }

        private TextSpan GetTextSpan(INode node)
        {
            return TextSpan.FromBounds(node.Range[0] + Offset, node.Range[1] + Offset);
        }

        private TextSpan GetTextSpan(Comment token)
        {
            return TextSpan.FromBounds(token.Start + Offset, token.End + Offset);
        }
    }
}
