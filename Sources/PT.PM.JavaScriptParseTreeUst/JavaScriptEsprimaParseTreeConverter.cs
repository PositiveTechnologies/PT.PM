using Esprima.Ast;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using UstTokens = PT.PM.Common.Nodes.Tokens;
using UstExprs = PT.PM.Common.Nodes.Expressions;
using UstSpecific = PT.PM.Common.Nodes.Specific;
using UstStmts = PT.PM.Common.Nodes.Statements;
using Collections = System.Collections.Generic;
using PT.PM.Common.Nodes.TypeMembers;
using Esprima;
using System.Threading;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes.Tokens.Literals;
using Comment = PT.PM.Common.Nodes.Tokens.Literals.Comment;

namespace PT.PM.JavaScriptParseTreeUst
{
    public partial class JavaScriptEsprimaParseTreeConverter : IParseTreeToUstConverter
    {
        private RootUst root;
        private ConvertHelper convertHelper;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => Language.JavaScript;

        public TextFile SourceFile { get; set; }

        public HashSet<Language> AnalyzedLanguages { get; set; }

        public RootUst ParentRoot { get; set; }

        public int Offset { get; set; }

        public static JavaScriptEsprimaParseTreeConverter Create() => new JavaScriptEsprimaParseTreeConverter();

        public RootUst Convert(ParseTree langParseTree)
        {
            try
            {
                var esprimaParseTree = (JavaScriptEsprimaParseTree)langParseTree;
                if (SourceFile == null)
                {
                    SourceFile = esprimaParseTree.SourceFile;
                }

                root = new RootUst(SourceFile, Language.JavaScript, GetTextSpan(esprimaParseTree.SyntaxTree));
                convertHelper = new ConvertHelper(root) {Logger = Logger};

                var program = VisitProgram(esprimaParseTree.SyntaxTree);

                root.Nodes = new Ust[] {program};

                var comments = new Collections.List<Comment>(esprimaParseTree.Comments.Count);
                foreach (Esprima.Comment comment in esprimaParseTree.Comments)
                {
                    TextSpan textSpan = GetTextSpan(comment);
                    comments.Add(new Comment(textSpan)
                    {
                        Root = root,
                    });
                }

                root.Comments = comments.ToArray();
                root.Root = ParentRoot;
                root.FillAscendants();

                return root;
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
                switch (node)
                {
                    case Statement statement:
                        return VisitStatement(statement);

                    case Expression expression:
                        return VisitExpression(expression);

                    case ArrayPattern arrayPattern:
                        return VisitArrayPattern(arrayPattern);

                    case ObjectPattern objectPattern:
                        return VisitObjectPattern(objectPattern);
                }

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
            var elements = new Collections.List<ParameterDeclaration>(arrayPattern.Elements.Count);

            foreach (IArrayPatternElement elem in arrayPattern.Elements)
            {
                ParameterDeclaration paramDecl = VisitArrayPatternElement(elem);
                elements.Add(paramDecl);
            }

            return new UstSpecific.ArrayPatternExpression(elements, GetTextSpan(arrayPattern));
        }

        private ParameterDeclaration VisitArrayPatternElement(IArrayPatternElement arrayPatternElement)
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

        private UstStmts.BlockStatement ConvertToBlockStatementIfRequired(INode functionBody)
        {
            var body = Visit(functionBody).ToStatementIfRequired();
            var blockStatement = body is UstStmts.BlockStatement localBlockStatement
                ? localBlockStatement
                : new UstStmts.BlockStatement(new[] {body});
            return blockStatement;
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
            return TextSpan.FromBounds(node.Range.Start + Offset, node.Range.End + Offset);
        }

        private TextSpan GetTextSpan(Esprima.Comment token)
        {
            return TextSpan.FromBounds(token.Start + Offset, token.End + Offset);
        }
    }
}
