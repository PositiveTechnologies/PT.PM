using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Specific;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Utils;
using PT.PM.JavaScriptParseTreeUst;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PT.PM.Common.Files;

namespace PT.PM.PhpParseTreeUst
{
    public partial class PhpAntlrParseTreeConverter : AntlrConverter, IPhpParserVisitor<Ust>
    {
        private const string namespacePrefix = CommonUtils.Prefix + "ns";
        private const string elementNamespacePrefix = CommonUtils.Prefix + "elemNs";
        private const string contentNamespacePrefix = CommonUtils.Prefix + "contentNs";
        private const string attrNamespacePrefix = CommonUtils.Prefix + "attrNs";
        private const string inlineHtmlNamespacePrefix = CommonUtils.Prefix + "inlineHtml";

        private int jsStartCodeInd = 0;

        public override Language Language => Language.Php;

        public JavaScriptType JavaScriptType { get; set; }

        public static PhpAntlrParseTreeConverter Create() => new PhpAntlrParseTreeConverter();

        public PhpAntlrParseTreeConverter()
        {
        }

        public Ust VisitHtmlDocument(PhpParser.HtmlDocumentContext context)
        {
            IEnumerable<Ust> phpBlocks = context.htmlElementOrPhpBlock()
                .Select(block => Visit(block))
                .Where(phpBlock => phpBlock != null);
            root.Nodes = phpBlocks.SelectMany(block => block.SelectAnalyzedNodes(Language.Php, AnalyzedLanguages)).ToArray();

            return root;
        }

        public Ust VisitHtmlElementOrPhpBlock(PhpParser.HtmlElementOrPhpBlockContext context)
        {
            Ust result = null;
            if (context.htmlElements() != null)
            {
                result = Visit(context.htmlElements());
            }
            else if (context.phpBlock() != null)
            {
                result = (NamespaceDeclaration)Visit(context.phpBlock());
            }
            else
            {
                result = Visit(context.scriptTextPart());
            }
            return result;
        }

        public Ust VisitHtmlElements(PhpParser.HtmlElementsContext context)
        {
            var text = new StringBuilder();
            for (int i = context.Start.TokenIndex; i < context.Stop.TokenIndex; i++)
            {
                IToken token = Tokens[i];

                if (AnalyzedLanguages.Contains(Language.JavaScript))
                {
                    if (token.Type == PhpLexer.HtmlScriptOpen)
                    {
                        jsStartCodeInd = i;
                    }
                    else if (token.Type == PhpLexer.ScriptClose)
                    {
                        return ConvertJavaScript(context);
                    }
                }

                text.Append(token.Text);
            }

            var result = AnalyzedLanguages.Contains(Language.Html)
                ? new RootUst(root.SourceFile, Language.Html)
                {
                    Node = new StringLiteral(text.ToString(), context.GetTextSpan(), 0)
                }
                : null;

            return result;
        }

        private RootUst ConvertJavaScript(PhpParser.HtmlElementsContext context)
        {
            int jsStopCodeInd = context.start.TokenIndex;
            var jsCode = new StringBuilder();
            for (int j = jsStartCodeInd; j < jsStopCodeInd; j++)
            {
                if (Tokens[j].Type == PhpLexer.ScriptText)
                {
                    jsCode.Append(Tokens[j].Text);
                }
                else
                {
                    if (GetLastNotWhitespace(jsCode) == '=')
                    {
                        jsCode.Append('_');
                        jsCode.Append(' ', Tokens[j].Text.Length - 1);
                    }
                    else
                    {
                        jsCode.Append(' ', Tokens[j].Text.Length);
                    }
                }
            }

            int offset = Tokens[jsStartCodeInd].StartIndex;

            var sourceFile = new TextFile(jsCode.ToString())
            {
                Name = root.SourceFile.Name,
                RelativePath = root.SourceFile.RelativePath,
            };

            var javaScriptParser = new JavaScriptEsprimaParser
            {
                Logger = Logger,
                JavaScriptType = JavaScriptType,
                Offset = offset,
                OriginFile = root.SourceFile
            };

            var parseTree = (JavaScriptEsprimaParseTree)javaScriptParser.Parse(sourceFile, out TimeSpan _);
            if (parseTree == null)
            {
                return null;
            }

            var javaScriptConverter = new JavaScriptEsprimaParseTreeConverter
            {
                Logger = Logger,
                SourceFile = root.SourceFile,
                ParentRoot = root,
                Offset = offset
            };

            RootUst result = javaScriptConverter.Convert(parseTree);

            return result;
        }

        private char GetLastNotWhitespace(StringBuilder builder)
        {
            int ind = builder.Length - 1;
            while (ind > 0 && char.IsWhiteSpace(builder[ind]))
                ind--;

            return ind > 0 ? builder[ind] : '\0';
        }

        public Ust VisitHtmlElement(PhpParser.HtmlElementContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitScriptTextPart(PhpParser.ScriptTextPartContext context)
        {
            string javaScriptCode = string.Join("", context.ScriptText().Select(text => text.GetText()));
            // Process JavaScript at close script tag </script>
            return AnalyzedLanguages.Contains(Language.JavaScript)
                ? null
                : new StringLiteral(javaScriptCode, context.GetTextSpan(), 0);
        }

        public Ust VisitPhpBlock(PhpParser.PhpBlockContext context)
        {
            TextSpan textSpan = context.GetTextSpan();
            var namespaceName = new StringLiteral(namespacePrefix, textSpan, 0);
            UsingDeclaration[] usingDeclarations = context.importStatement()
                .Select(importStatement => (UsingDeclaration)Visit(importStatement))
                .Where(stmt => stmt != null)
                .ToArray();

            Statement[] topStatements = context.topStatement()
                .Select(topStatement => (Statement)Visit(topStatement))
                .Where(stmt => stmt != null)
                .ToArray();
            var statementsNode = new BlockStatement(topStatements);
            var members = new List<Ust>();
            members.AddRange(usingDeclarations);
            members.Add(statementsNode);

            var result = new NamespaceDeclaration(namespaceName, members, textSpan);
            return result;
        }

        public Ust VisitImportStatement(PhpParser.ImportStatementContext context)
        {
            var namespaceName = (StringLiteral)Visit(context.namespaceNameList());

            var result = new UsingDeclaration(namespaceName, context.GetTextSpan());
            return result;
        }

        public Ust VisitTopStatement(PhpParser.TopStatementContext context)
        {
            Statement result;
            if (context.classDeclaration() != null)
            {
                var typeDeclaration = (TypeDeclaration)Visit(context.classDeclaration());
                result = new TypeDeclarationStatement(typeDeclaration, typeDeclaration.TextSpan);
            }
            else
            {
                result = Visit(context.GetChild(0)).ToStatementIfRequired();
            }

            return result;
        }

        public Ust VisitUseDeclaration(PhpParser.UseDeclarationContext context)
        {
            var result = new UsingDeclaration(
                new StringLiteral(context.useDeclarationContentList().GetTextSpan(), root, 0),
                context.GetTextSpan());
            return result;
        }

        public Ust VisitUseDeclarationContentList(PhpParser.UseDeclarationContentListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitUseDeclarationContent(PhpParser.UseDeclarationContentContext context)
        {
            var namespaceNameListUst = (StringLiteral)Visit(context.namespaceNameList());

            var result = new UsingDeclaration(namespaceNameListUst, context.GetTextSpan());
            return result;
        }

        public Ust VisitNamespaceDeclaration(PhpParser.NamespaceDeclarationContext context)
        {
            StringLiteral name;
            if (context.namespaceNameList() != null)
            {
                name = (StringLiteral)Visit(context.namespaceNameList());
            }
            else
            {
                name = new StringLiteral(CommonUtils.Prefix + "unnamed", default, 0);
            }

            Ust[] members = context.namespaceStatement()
                .Select(statement => Visit(statement))
                .Where(statement => statement != null)
                .ToArray();

            var result = new NamespaceDeclaration(name, members, context.GetTextSpan());
            return result;
        }

        public Ust VisitNamespaceStatement(PhpParser.NamespaceStatementContext context)
        {
            return Visit(context.GetChild(0));
        }

        public Ust VisitFunctionDeclaration(PhpParser.FunctionDeclarationContext context)
        {
            TypeToken returnType = null;
            if (context.typeParameterListInBrackets() != null)
            {
                returnType = (TypeToken)Visit(context.typeParameterListInBrackets());
            }
            var id = (IdToken)Visit(context.identifier());
            var body = (BlockStatement)Visit(context.blockStatement());
            var parameters = ConvertParameters(context.formalParameterList());

            var result = new MethodDeclaration(id, parameters, body, context.GetTextSpan())
            {
                ReturnType = returnType
            };
            return result;
        }

        public Ust VisitClassDeclaration(PhpParser.ClassDeclarationContext context)
        {
            TypeTypeLiteral typeTypeToken;
            if (context.classEntryType() != null)
            {
                typeTypeToken = new TypeTypeLiteral(TypeType.Class, context.classEntryType().GetTextSpan());
            }
            else
            {
                typeTypeToken = new TypeTypeLiteral(TypeType.Interface, context.Interface().GetTextSpan());
            }

            EntityDeclaration[] members = context.classStatement()
                .Select(statement => (EntityDeclaration)Visit(statement))
                .Where(statement => statement != null)
                .ToArray();

            var id = (IdToken)Visit(context.identifier());
            var result = new TypeDeclaration(typeTypeToken, id, members, context.GetTextSpan());
            return result;
        }

        public Ust VisitClassEntryType(PhpParser.ClassEntryTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInterfaceList(PhpParser.InterfaceListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypeParameterListInBrackets(PhpParser.TypeParameterListInBracketsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypeParameterList(PhpParser.TypeParameterListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypeParameterWithDefaultsList(PhpParser.TypeParameterWithDefaultsListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypeParameterDecl(PhpParser.TypeParameterDeclContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypeParameterWithDefaultDecl(PhpParser.TypeParameterWithDefaultDeclContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGenericDynamicArgs(PhpParser.GenericDynamicArgsContext context)
        {
            string[] typeRefs = context.typeRef()
                .Select(type => ((TypeToken)Visit(type))?.TypeText)
                .Where(type => type != null).ToArray();

            var str = context.GetChild(0).GetText() + string.Join(",", typeRefs) + context.GetChild(context.ChildCount - 1).GetText();
            var result = new TypeToken(str, context.GetTextSpan());
            return result;
        }

        public Ust VisitAttributes(PhpParser.AttributesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAttributesGroup(PhpParser.AttributesGroupContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAttribute(PhpParser.AttributeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAttributeArgList(PhpParser.AttributeArgListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAttributeNamedArgList(PhpParser.AttributeNamedArgListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAttributeNamedArg(PhpParser.AttributeNamedArgContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInnerStatementList(PhpParser.InnerStatementListContext context)
        {
            List<Statement> innerStatementUsts = context.innerStatement()
                .Select(c => (Statement)Visit(c))
                .Where(c => c != null)
                .ToList();

            var result = new BlockStatement(innerStatementUsts,
                context.innerStatement().Length > 0 ? context.GetTextSpan() : default(TextSpan));
            return result;
        }

        public Ust VisitInnerStatement(PhpParser.InnerStatementContext context)
        {
            Statement result;
            if (context.statement() != null)
            {
                result = (Statement)Visit(context.statement());
            }
            else if (context.functionDeclaration() != null)
            {
                var funcDeclaraion = (MethodDeclaration)Visit(context.functionDeclaration());
                result = new WrapperStatement(funcDeclaraion, funcDeclaraion.TextSpan);
            }
            else
            {
                var typeDeclaration = (TypeDeclaration)Visit(context.classDeclaration());
                result = new TypeDeclarationStatement(typeDeclaration, typeDeclaration.TextSpan);
            }
            return result;
        }

        public Ust VisitStatement(PhpParser.StatementContext context)
        {
            Statement result;
            if (context.identifier() != null)
            {
                return new EmptyStatement(context.GetTextSpan());
            }

            if (context.yieldExpression() != null)
            {
                result = new ExpressionStatement((Expression)Visit(context.yieldExpression()),
                    context.GetTextSpan());
                return result;
            }

            result = Visit(context.GetChild(0)).ToStatementIfRequired();

            return result;
        }

        private int GetFirstHiddenTokenOrDefaultToLeft(int index)
        {
            int i = index;
            while (i > 0 && Tokens[i].Channel != Lexer.DefaultTokenChannel)
            {
                i--;
            }
            return i + 1;
        }

        private int GetLastHiddenTokenOrDefaultToRight(int index)
        {
            int i = index;
            while (i < Tokens.Count && Tokens[i].Channel != Lexer.DefaultTokenChannel)
            {
                i++;
            }
            return i - 1;
        }

        public Ust VisitEmptyStatement(PhpParser.EmptyStatementContext context)
        {
            return new EmptyStatement(context.GetTextSpan());
        }

        public Ust VisitBlockStatement(PhpParser.BlockStatementContext context)
        {
            var innerStatementListUst = (BlockStatement)Visit(context.innerStatementList());

            var result = new BlockStatement(innerStatementListUst.Statements, context.GetTextSpan());
            return result;
        }

        public Ust VisitIfStatement(PhpParser.IfStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            Statement trueStatement;
            List<IfElseStatement> ifElseStatements;
            Statement elseStatement = null;
            if (context.statement() != null)
            {
                trueStatement = (Statement)Visit(context.statement());
                ifElseStatements = context.elseIfStatement()
                    .Select(statement => (IfElseStatement)Visit(statement))
                    .Where(statement => statement != null).ToList();
                if (context.elseStatement() != null)
                {
                    elseStatement = (Statement)Visit(context.elseStatement());
                }
            }
            else
            {
                trueStatement = (Statement)Visit(context.innerStatementList());
                ifElseStatements = context.elseIfColonStatement()
                    .Select(statement => (IfElseStatement)Visit(statement))
                    .Where(statement => statement != null).ToList();
                if (context.elseColonStatement() != null)
                {
                    elseStatement = (Statement)Visit(context.elseColonStatement());
                }
            }

            var result = new IfElseStatement(condition, trueStatement, context.GetTextSpan());
            IfElseStatement s = result;
            foreach (var elseIfStatement in ifElseStatements)
            {
                s.FalseStatement = elseIfStatement;
                s = elseIfStatement;
            }
            s.FalseStatement = elseStatement;
            return result;
        }

        public Ust VisitElseIfStatement(PhpParser.ElseIfStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            var statement = (Statement)Visit(context.statement());

            var result = new IfElseStatement(condition, statement, context.GetTextSpan());
            return result;
        }

        public Ust VisitElseIfColonStatement(PhpParser.ElseIfColonStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            var statement = (Statement)Visit(context.innerStatementList());

            var result = new IfElseStatement(condition, statement, context.GetTextSpan());
            return result;
        }

        public Ust VisitElseStatement(PhpParser.ElseStatementContext context)
        {
            var statement = (Statement)Visit(context.statement());
            return statement;
        }

        public Ust VisitElseColonStatement(PhpParser.ElseColonStatementContext context)
        {
            var statement = (Statement)Visit(context.innerStatementList());
            return statement;
        }

        public Ust VisitWhileStatement(PhpParser.WhileStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            Statement statement = context.statement() != null
                ? (Statement)Visit(context.statement())
                : (Statement)Visit(context.innerStatementList());

            var result = new WhileStatement(condition, statement, context.GetTextSpan());
            return result;
        }

        public Ust VisitDoWhileStatement(PhpParser.DoWhileStatementContext context)
        {
            var statement = (Statement)Visit(context.statement());
            var condition = (Expression)Visit(context.parenthesis());

            var result = new DoWhileStatement(statement, condition, context.GetTextSpan());
            return result;
        }

        public Ust VisitForStatement(PhpParser.ForStatementContext context)
        {
            List<Statement> initializer = new List<Statement>();
            if (context.forInit() != null)
            {
                initializer.AddRange(context.forInit().expressionList().expression()
                    .Select(e =>
                    {
                        var expr = (Expression)Visit(e);
                        return expr != null ? new ExpressionStatement(expr) : null;
                    })
                    .Where(s => s != null));
            }

            Expression condition = null;
            if (context.expressionList() != null)
            {
                condition = (MultichildExpression)Visit(context.expressionList());
            }

            List<Expression> iterators = new List<Expression>();
            if (context.forUpdate() != null)
            {
                iterators.AddRange(context.forUpdate().expressionList().expression()
                    .Select(e => (Expression)Visit(e))
                    .Where(e => e != null));
            }

            Statement statement = context.statement() != null
                ? (Statement)Visit(context.statement())
                : (Statement)Visit(context.innerStatementList());

            var result = new ForStatement(initializer, condition, iterators, statement, context.GetTextSpan());
            return result;
        }

        public Ust VisitForInit(PhpParser.ForInitContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitForUpdate(PhpParser.ForUpdateContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitSwitchStatement(PhpParser.SwitchStatementContext context)
        {
            var expression = (Expression)Visit(context.parenthesis());
            SwitchSection[] switchBlocks = context.switchBlock()
                .Select(block => (SwitchSection)Visit(block))
                .Where(block => block != null)
                .ToArray();

            var result = new SwitchStatement(expression, switchBlocks, context.GetTextSpan());
            return result;
        }

        public Ust VisitSwitchBlock(PhpParser.SwitchBlockContext context)
        {
            Expression[] caseLabels = context.expression().Select(e => (Expression)Visit(e))
                .Where(e => e != null).ToArray();
            var innerStatement = (BlockStatement)Visit(context.innerStatementList());
            var statements = innerStatement.Statements.ToList();
            if (context.Default().Length > 0)
                statements.Add(null);

            var result = new SwitchSection(caseLabels, statements, context.GetTextSpan());
            return result;
        }

        public Ust VisitBreakStatement(PhpParser.BreakStatementContext context)
        {
            var result = new BreakStatement(context.GetTextSpan())
            {
                Expression = context.expression() == null ? null : (Expression)Visit(context.expression())
            };
            return result;
        }

        public Ust VisitContinueStatement(PhpParser.ContinueStatementContext context)
        {
            var result = new ContinueStatement(context.GetTextSpan())
            {
                Expression = context.expression() == null ? null : (Expression)Visit(context.expression())
            };
            return result;
        }

        public Ust VisitReturnStatement(PhpParser.ReturnStatementContext context)
        {
            var returnExpression = context.expression() == null ? null : (Expression)Visit(context.expression());
            var result = new ReturnStatement(returnExpression, context.GetTextSpan());
            return result;
        }

        public Ust VisitExpressionStatement(PhpParser.ExpressionStatementContext context)
        {
            var expression = (Expression)Visit(context.expression());
            var result = new ExpressionStatement(expression, context.GetTextSpan());
            return result;
        }

        public Ust VisitUnsetStatement(PhpParser.UnsetStatementContext context)
        {
            var args = (ArgsUst)Visit(context.chainList());
            var invocation = new InvocationExpression(
                new IdToken(context.Unset().GetText(), context.Unset().GetTextSpan()),
                args, context.GetTextSpan());
            var result = new ExpressionStatement(invocation, context.GetTextSpan());
            return result;
        }

        public Ust VisitForeachStatement(PhpParser.ForeachStatementContext context)
        {
            var expressions = new List<Expression>();
            if (context.expression() != null)
            {
                expressions.Add((Expression)Visit(context.expression()));
            }
            expressions.AddRange(context.chain().Select(c => (Expression)Visit(c))
                .Where(c => c != null));
            if (context.assignmentList() != null)
            {
                var assignmentList = (ArgsUst)Visit(context.assignmentList());
                expressions.AddRange(assignmentList.Collection);
            }

            var inExpression = new MultichildExpression(expressions, context.GetTextSpan()); // TODO: Spans union

            Statement statement = context.statement() != null
                ? (Statement)Visit(context.statement())
                : (Statement)Visit(context.innerStatementList());

            var result = new ForeachStatement(null, null, inExpression, statement, context.GetTextSpan());
            return result;
        }

        public Ust VisitTryCatchFinally(PhpParser.TryCatchFinallyContext context)
        {
            var statement = (BlockStatement)Visit(context.blockStatement());

            List<CatchClause> catchClauses = context.catchClause()
                .Select(clause => (CatchClause)Visit(clause)).Where(c => c != null).ToList();
            if (context.catchClause().Length == 0)
                catchClauses = null;

            BlockStatement finallyBlock = null;
            if (context.finallyStatement() != null)
            {
                finallyBlock = (BlockStatement)Visit(context.finallyStatement());
            }

            var result = new TryCatchStatement(statement, context.GetTextSpan())
            {
                CatchClauses = catchClauses,
                FinallyBlock = finallyBlock
            };
            return result;
        }

        public Ust VisitCatchClause(PhpParser.CatchClauseContext context)
        {
            var type = (TypeToken)Visit(context.qualifiedStaticTypeRef());
            var varName = (IdToken)ConvertVar(context.VarName());
            var body = (BlockStatement)Visit(context.blockStatement());

            var result = new CatchClause(type, varName, body, context.GetTextSpan());
            return result;
        }

        public Ust VisitFinallyStatement(PhpParser.FinallyStatementContext context)
        {
            var result = (BlockStatement)Visit(context.blockStatement());
            return result;
        }

        public Ust VisitThrowStatement(PhpParser.ThrowStatementContext context)
        {
            var expression = (Expression)Visit(context.expression());
            var result = new ThrowStatement(expression, context.GetTextSpan());
            return result;
        }

        public Ust VisitGotoStatement(PhpParser.GotoStatementContext context)
        {
            return new EmptyStatement(context.GetTextSpan());
        }

        public Ust VisitDeclareStatement(PhpParser.DeclareStatementContext context)
        {
            AssignmentExpression[] variables = context.declareList().identifierInititalizer().Select(
                id => (AssignmentExpression)Visit(id))
                .Where(id => id != null).ToArray();

            var decl = new FieldDeclaration(variables, context.declareList().GetTextSpan());

            Statement statement;
            if (context.statement() != null)
            {
                statement = (Statement)Visit(context.statement());
            }
            else
            {
                statement = (Statement)Visit(context.innerStatementList());
            }

            var result = new WithStatement(decl, statement, context.GetTextSpan());
            return result;
        }

        public Ust VisitDeclareList(PhpParser.DeclareListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitInlineHtmlStatement([NotNull] PhpParser.InlineHtmlStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInlineHtml(PhpParser.InlineHtmlContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFormalParameterList(PhpParser.FormalParameterListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitFormalParameter(PhpParser.FormalParameterContext context)
        {
            TypeToken type = null;
            if (context.typeHint() != null)
                type = (TypeToken)Visit(context.typeHint());

            var varInit = (AssignmentExpression)Visit(context.variableInitializer());

            var result = new ParameterDeclaration(null, type, (IdToken)varInit.Left, context.GetTextSpan());
            result.Initializer = varInit.Right;
            return result;
        }

        public Ust VisitTypeHint(PhpParser.TypeHintContext context)
        {
            if (context.Callable() != null)
                return new TypeToken(context.GetText(), context.GetTextSpan());

            return (TypeToken)Visit(context.GetChild(0));
        }

        public Ust VisitGlobalStatement(PhpParser.GlobalStatementContext context)
        {
            Expression[] globalVars = context.globalVar()
                .Select(globalVar => (Expression)Visit(globalVar))
                .Where(v => v != null).ToArray();
            var multichild = new MultichildExpression(globalVars, context.GetTextSpan());

            var result = new ExpressionStatement(multichild, context.GetTextSpan());
            return result;
        }

        public Ust VisitGlobalVar(PhpParser.GlobalVarContext context)
        {
            if (context.VarName() != null)
                return (IdToken)ConvertVar(context.VarName());

            if (context.chain() != null)
                return (Expression)Visit(context.chain());

            return (Expression)Visit(context.expression());
        }

        public Ust VisitEchoStatement(PhpParser.EchoStatementContext context)
        {
            var name = new IdToken(context.Echo().GetText(), context.Echo().GetTextSpan());
            var args = new ArgsUst(((MultichildExpression)Visit(context.expressionList())).Expressions,
                context.expressionList().GetTextSpan());
            var invocation = new InvocationExpression(name, args, name.TextSpan.Union(args.TextSpan));
            var result = new ExpressionStatement(invocation, context.GetTextSpan());
            return result;
        }

        public Ust VisitStaticVariableStatement(PhpParser.StaticVariableStatementContext context)
        {
            AssignmentExpression[] variables = context.variableInitializer()
                .Select(varInit => (AssignmentExpression)Visit(varInit))
                .Where(v => v != null).ToArray();
            var type = new TypeToken(context.Static().GetText(), context.Static().GetTextSpan());

            var result = new VariableDeclarationExpression(type, variables, context.GetTextSpan());
            return result;
        }

        public Ust VisitClassStatement(PhpParser.ClassStatementContext context)
        {
            EntityDeclaration result = null;

            if (context.variableInitializer().Length > 0)
            {
                AssignmentExpression[] properties = context.variableInitializer().Select(decl =>
                    (AssignmentExpression)Visit(decl))
                    .Where(decl => decl != null).ToArray();

                result = new FieldDeclaration(properties, context.GetTextSpan());
            }
            else if (context.identifierInititalizer().Length > 0)
            {
                AssignmentExpression[] variables = context.identifierInititalizer().Select(decl =>
                    (AssignmentExpression)Visit(decl))
                    .Where(decl => decl != null).ToArray();

                result = new FieldDeclaration(variables, context.GetTextSpan());
            }
            else if (context.Function() != null)
            {
                var id = (IdToken)VisitIdentifier(context.identifier());
                var parameters = (ParameterDeclaration[])ConvertParameters(context.formalParameterList());
                var block = (BlockStatement)Visit(context.methodBody());

                result = new MethodDeclaration(id, parameters, block, context.GetTextSpan());
            }
            else if (context.Use() != null)
            {
                // TODO: UsingDeclaraion (EntityWrapper).
                result = null;
            }

            return result;
        }

        public Ust VisitTraitAdaptations(PhpParser.TraitAdaptationsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTraitAdaptationStatement(PhpParser.TraitAdaptationStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTraitPrecedence(PhpParser.TraitPrecedenceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTraitAlias(PhpParser.TraitAliasContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTraitMethodReference(PhpParser.TraitMethodReferenceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBaseCtorCall(PhpParser.BaseCtorCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMethodBody(PhpParser.MethodBodyContext context)
        {
            if (context.blockStatement() != null)
                return (BlockStatement)Visit(context.blockStatement());

            return null;
        }

        public Ust VisitPropertyModifiers(PhpParser.PropertyModifiersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMemberModifiers(PhpParser.MemberModifiersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVariableInitializer(PhpParser.VariableInitializerContext context)
        {
            IdToken name = (IdToken)ConvertVar(context.VarName());
            Expression initializer = null;
            if (context.constantInititalizer() != null)
            {
                initializer = (Expression)Visit(context.constantInititalizer());
            }

            var result = new AssignmentExpression(name, initializer, context.GetTextSpan());
            return result;
        }

        public Ust VisitIdentifierInititalizer(PhpParser.IdentifierInititalizerContext context)
        {
            var id = (IdToken)Visit(context.identifier());
            var initializer = (Expression)Visit(context.constantInititalizer());

            var result = new AssignmentExpression(id, initializer, context.GetTextSpan());
            return result;
        }

        public Ust VisitGlobalConstantDeclaration(PhpParser.GlobalConstantDeclarationContext context)
        {
            AssignmentExpression[] identifiers = context.identifierInititalizer()
                .Select(id => (AssignmentExpression)Visit(id))
                .Where(id => id != null).ToArray();

            var result = new FieldDeclaration(identifiers, context.GetTextSpan());
            return result;
        }

        public Ust VisitExpressionList(PhpParser.ExpressionListContext context)
        {
            Expression[] expressions = context.expression().Select(c => (Expression)Visit(c))
                .Where(c => c != null).ToArray();

            return new MultichildExpression(expressions, context.GetTextSpan());
        }

        public Ust VisitParenthesis(PhpParser.ParenthesisContext context)
        {
            if (context.expression() != null)
                return Visit(context.expression());

            return Visit(context.yieldExpression());
        }

        public Ust VisitExpression([NotNull] PhpParser.ExpressionContext context)
        {
            return Visit(context);
        }

        public Ust VisitNewExpr(PhpParser.NewExprContext context)
        {
            var type = (TypeToken)Visit(context.typeRef());
            ArgsUst args = context.arguments() != null
                ? (ArgsUst)Visit(context.arguments())
                : new ArgsUst();

            var result = new ObjectCreateExpression(type, args, context.GetTextSpan());
            return result;
        }

        public Ust VisitConditionalExpression([NotNull] PhpParser.ConditionalExpressionContext context)
        {
            var expression0 = (Expression)Visit(context.expression(0));
            var expression1 = (Expression)(context.expression().Length == 3 ? Visit(context.expression(1)) : null);
            var expression2 = (Expression)Visit(context.expression().Last());
            var result = new ConditionalExpression(expression0, expression1, expression2, context.GetTextSpan());
            return result;
        }

        public Ust VisitLogicalExpression([NotNull] PhpParser.LogicalExpressionContext context)
        {
            return CreateBinaryOperatorExpression(context.expression(0), context.op, context.expression(1));
        }

        public Ust VisitArithmeticExpression([NotNull] PhpParser.ArithmeticExpressionContext context)
        {
            return CreateBinaryOperatorExpression(context.expression(0), context.op, context.expression(1));
        }

        public Ust VisitInstanceOfExpression([NotNull] PhpParser.InstanceOfExpressionContext context)
        {
            return (Expression)Visit(context.expression()); // TODO: InstanceOf
        }

        public Ust VisitBitwiseExpression([NotNull] PhpParser.BitwiseExpressionContext context)
        {
            Expression result = CreateBinaryOperatorExpression(context.expression(0), context.op, context.expression(1));
            return result;
        }

        public Ust VisitComparisonExpression([NotNull] PhpParser.ComparisonExpressionContext context)
        {
            Expression result = CreateBinaryOperatorExpression(context.expression(0), context.op, context.expression(1));
            return result;
        }

        public Ust VisitCloneExpression(PhpParser.CloneExpressionContext context)
        {
            return CreateSpecialInvocation(context.Clone(), context.expression(), context.GetTextSpan());
        }

        public Ust VisitNewExpression(PhpParser.NewExpressionContext context)
        {
            return (Expression)Visit(context.newExpr());
        }

        public Ust VisitIndexerExpression(PhpParser.IndexerExpressionContext context)
        {
            var target = (Expression)Visit(context.stringConstant());
            var arg = (Expression)Visit(context.expression());

            var result = new IndexerExpression(target, new ArgsUst(new[] { arg }), context.GetTextSpan());
            return result;
        }

        public Ust VisitPrefixIncDecExpression(PhpParser.PrefixIncDecExpressionContext context)
        {
            ITerminalNode operatorTerminal = context.GetChild<ITerminalNode>(0);
            string unaryOperatorText = operatorTerminal.GetText();
            UnaryOperator unaryOperator = UnaryOperatorLiteral.PrefixTextUnaryOperator[unaryOperatorText];
            var expression = (Expression)Visit(context.chain());
            var result = new UnaryOperatorExpression(
                new UnaryOperatorLiteral(unaryOperator, operatorTerminal.GetTextSpan()),
                expression,
                context.GetTextSpan());

            return result;
        }

        public Ust VisitPostfixIncDecExpression(PhpParser.PostfixIncDecExpressionContext context)
        {
            ITerminalNode operatorTerminal = context.GetChild<ITerminalNode>(0);
            string unaryOperatorText = operatorTerminal.GetText();
            UnaryOperator unaryOperator = UnaryOperatorLiteral.PostfixTextUnaryOperator[unaryOperatorText];
            var expression = (Expression)Visit(context.chain());
            var result = new UnaryOperatorExpression(
                new UnaryOperatorLiteral(unaryOperator, operatorTerminal.GetTextSpan()),
                expression,
                context.GetTextSpan());

            return result;
        }

        public Ust VisitCastExpression(PhpParser.CastExpressionContext context)
        {
            var castType = (TypeToken)Visit(context.castOperation());
            var expression = (Expression)Visit(context.expression());

            var result = new CastExpression(castType, expression, context.GetTextSpan());
            return result;
        }

        public Ust VisitUnaryOperatorExpression(PhpParser.UnaryOperatorExpressionContext context)
        {
            UnaryOperator unaryOperator;
            ITerminalNode operatorTerminal = context.GetChild<ITerminalNode>(0);
            string unaryOperatorText = operatorTerminal.GetText();
            if (unaryOperatorText == "@")
            {
                return (Expression)Visit(context.expression());
            }
            else
            {
                unaryOperator = UnaryOperatorLiteral.PrefixTextUnaryOperator[unaryOperatorText];
            }
            var expression0 = (Expression)Visit(context.expression());
            var result = new UnaryOperatorExpression(
                new UnaryOperatorLiteral(unaryOperator, operatorTerminal.GetTextSpan()),
                expression0,
                context.GetTextSpan());

            return result;
        }

        public Ust VisitAssignmentExpression(PhpParser.AssignmentExpressionContext context)
        {
            var left = (Expression)Visit(context.chain(0));

            Expression result;
            if (context.ChildCount == 3)
            {
                var right = (Expression)Visit(context.expression());
                return CreateAssignExpr(left, right, context, context.assignmentOperator());
            }
            else
            {
                result = new AssignmentExpression(left,
                    (Expression)Visit(context.GetChild(3)), context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitAssignmentOperator(PhpParser.AssignmentOperatorContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitPrintExpression(PhpParser.PrintExpressionContext context)
        {
            return (Expression)CreateSpecialInvocation(context.Print(), context.expression(), context.GetTextSpan());
        }

        public Ust VisitChainExpression(PhpParser.ChainExpressionContext context)
        {
            return (Expression)VisitChildren(context);
        }

        public Ust VisitScalarExpression(PhpParser.ScalarExpressionContext context)
        {
            if (context.Label() != null)
            {
                return new IdToken(context.GetText(), context.GetTextSpan());
            }

            return (Expression)Visit(context.GetChild(0));
        }

        public Ust VisitBackQuoteStringExpression(PhpParser.BackQuoteStringExpressionContext context)
        {
            return Visit(context.BackQuoteString());
        }

        public Ust VisitParenthesisExpression(PhpParser.ParenthesisExpressionContext context)
        {
            return (Expression)Visit(context.parenthesis());
        }

        public Ust VisitArrayCreationExpression(PhpParser.ArrayCreationExpressionContext context)
        {
            List<Expression> inits = context.arrayItemList()?.arrayItem()
                .Select(item => (Expression)Visit(item))
                .Where(item => item != null).ToList()
                ?? new List<Expression>();
            if (context.expression() != null)
                inits.Add((Expression)Visit(context.expression()));

            var result = new ArrayCreationExpression(null, Enumerable.Empty<Expression>(), inits,
                context.GetTextSpan());
            return result;
        }

        public Ust VisitSpecialWordExpression(PhpParser.SpecialWordExpressionContext context)
        {
            TextSpan textSpan = context.GetTextSpan();
            Expression expression, result;

            if (context.Yield() != null)
            {
                result = new InvocationExpression(
                    new IdToken(context.Yield().GetText(), context.Yield().GetTextSpan()),
                    new ArgsUst(), context.GetTextSpan());
            }
            else if (context.List() != null)
            {
                var args = (ArgsUst)Visit(context.assignmentList());
                expression = (Expression)Visit(context.expression());

                TextSpan listTextSpan = context.List().GetTextSpan();
                var invoke = new InvocationExpression(new IdToken(context.List().GetText(), listTextSpan),
                    args, listTextSpan.Union(context.GetChild<ITerminalNode>(2).GetTextSpan()));
                result = new AssignmentExpression(invoke, expression, textSpan);
            }
            else if (context.IsSet() != null)
            {
                var args = (ArgsUst)Visit(context.chainList());
                result = new InvocationExpression(
                    new IdToken(context.IsSet().GetText(), context.IsSet().GetTextSpan()),
                    args, textSpan);
            }
            else if (context.Empty() != null)
            {
                var args = new ArgsUst(new[] { (Expression)Visit(context.chain()) });
                result = new InvocationExpression(
                    new IdToken(context.Empty().GetText(), context.Empty().GetTextSpan()),
                    args, textSpan);
                return result;
            }
            else if (context.Exit() != null)
            {
                var exprs = new List<Expression>();
                if (context.parenthesis() != null)
                    exprs.Add((Expression)Visit(context.parenthesis()));
                result = new InvocationExpression(
                    new IdToken(context.Exit().GetText(), context.Exit().GetTextSpan()),
                    new ArgsUst(exprs), textSpan);
            }
            else
            {
                expression = (Expression)Visit(context.expression());
                result = new InvocationExpression(
                    new IdToken(context.GetChild(0).GetText()),
                    new ArgsUst(new Expression[] { expression }), textSpan);
            }
            return result;
        }

        public Ust VisitLambdaFunctionExpression(PhpParser.LambdaFunctionExpressionContext context)
        {
            ParameterDeclaration[] parameters = ConvertParameters(context.formalParameterList());
            var body = (BlockStatement)Visit(context.blockStatement());

            var result = new AnonymousMethodExpression(parameters, body, context.GetTextSpan());
            return result;
        }

        public Ust VisitYieldExpression(PhpParser.YieldExpressionContext context)
        {
            var expressions = context.expression()
                .Select(expression => (Expression)Visit(expression))
                .Where(e => e != null)
                .ToArray();

            var result = new MultichildExpression(expressions, context.GetTextSpan());
            return result;
        }

        public Ust VisitArrayItemList(PhpParser.ArrayItemListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitArrayItem(PhpParser.ArrayItemContext context)
        {
            Expression result;
            TextSpan textSpan = context.GetTextSpan();
            if (context.chain() != null)
            {
                Expression chainExpression = (Expression)Visit(context.chain());
                if (context.expression().Length == 1)
                {
                    Expression expression = (Expression)Visit(context.expression(0));
                    result = new AssignmentExpression(expression, chainExpression, textSpan);
                }
                else
                {
                    result = chainExpression;
                    result.TextSpan = textSpan;
                }
            }
            else
            {
                if (context.expression().Length == 2)
                {
                    Expression expression0 = (Expression)Visit(context.expression(0));
                    Expression expression1 = (Expression)Visit(context.expression(1));
                    result = new AssignmentExpression(expression0, expression1, textSpan);
                }
                else
                {
                    result = (Expression)Visit(context.expression(0));
                    result.TextSpan = textSpan;
                }
            }
            return result;
        }

        public Ust VisitLambdaFunctionUseVars(PhpParser.LambdaFunctionUseVarsContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitLambdaFunctionUseVar(PhpParser.LambdaFunctionUseVarContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitQualifiedStaticTypeRef(PhpParser.QualifiedStaticTypeRefContext context)
        {
            TextSpan textSpan = context.GetTextSpan();
            if (context.qualifiedNamespaceName() != null)
            {
                var typeStr = (StringLiteral)Visit(context.qualifiedNamespaceName());
                string genericStr = "";
                if (context.genericDynamicArgs() != null)
                {
                    genericStr = ((TypeToken)Visit(context.genericDynamicArgs())).TypeText;
                }
                return new TypeToken(typeStr.TextValue + genericStr, textSpan);
            }

            var result = new TypeToken(context.Static().GetText(), textSpan);
            return result;
        }

        public Ust VisitTypeRef(PhpParser.TypeRefContext context)
        {
            TypeToken result;
            TextSpan textSpan = context.GetTextSpan();

            string genericStr = "";
            if (context.genericDynamicArgs() != null)
            {
                genericStr = ((TypeToken)Visit(context.genericDynamicArgs())).TypeText;
            }

            if (context.qualifiedNamespaceName() != null)
            {
                var str = (StringLiteral)Visit(context.qualifiedNamespaceName());
                result = new TypeToken(str.TextValue + genericStr, textSpan);
            }
            else if (context.indirectTypeRef() != null)
            {
                var indirectTypeRef = (Expression)Visit(context.indirectTypeRef());
                result = new TypeToken(indirectTypeRef + genericStr, textSpan);
            }
            else if (context.primitiveType() != null)
            {
                result = (TypeToken)Visit(context.primitiveType());
            }
            else
            {
                result = new TypeToken(context.Static().GetText(), textSpan);
            }
            return result;
        }

        public Ust VisitIndirectTypeRef(PhpParser.IndirectTypeRefContext context)
        {
            var chainBase = (Expression)Visit(context.chainBase());

            Expression result;
            if (context.keyedFieldName().Length > 0)
            {
                IdToken[] memberAccesses = context.keyedFieldName()
                    .Select(keyedFieldName => (IdToken)Visit(keyedFieldName))
                    .Where(keyedFieldName => keyedFieldName != null).ToArray();

                MemberReferenceExpression memberRefExpr = new MemberReferenceExpression();
                for (int i = memberAccesses.Length - 1; i >= 0; i--)
                {
                    IdToken memberAccess = memberAccesses[i];
                    memberRefExpr.Name = memberAccess;
                    if (i > 0)
                    {
                        memberRefExpr.TextSpan = memberAccess.TextSpan;
                        memberRefExpr.Target = new MemberReferenceExpression();
                        memberRefExpr = (MemberReferenceExpression)memberRefExpr.Target;
                    }
                }

                memberRefExpr.Target = chainBase;
                memberRefExpr.TextSpan = context.GetTextSpan();

                result = memberRefExpr;
            }
            else
            {
                result = chainBase;
            }
            return result;
        }

        public Ust VisitQualifiedNamespaceName(PhpParser.QualifiedNamespaceNameContext context)
        {
            return (StringLiteral)Visit(context.namespaceNameList());
        }

        public Ust VisitNamespaceNameList(PhpParser.NamespaceNameListContext context)
        {
            return new StringLiteral(context.GetTextSpan(), root, 0);
        }

        public Ust VisitQualifiedNamespaceNameList(PhpParser.QualifiedNamespaceNameListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitArguments(PhpParser.ArgumentsContext context)
        {
            var args = new List<Expression>();
            if (context.yieldExpression() != null)
            {
                args.Add((Expression)Visit(context.yieldExpression()));
            }
            else
            {
                args.AddRange(context.actualArgument()
                    .Select(arg => (Expression)Visit(arg)));
            }
            var result = new ArgsUst(args, context.GetTextSpan());
            return result;
        }

        public Ust VisitActualArgument(PhpParser.ActualArgumentContext context)
        {
            if (context.expression() != null)
            {
                return (Expression)Visit(context.expression());
            }
            else
            {
                return (Expression)Visit(context.chain());
            }
        }

        public Ust VisitConstantInititalizer(PhpParser.ConstantInititalizerContext context)
        {
            if (context.constantArrayItemList() != null)
            {
                IEnumerable<Expression> inits = context.constantArrayItemList().constantArrayItem()
                    .Select(item => (Expression)Visit(item))
                    .Where(item => item != null);
                var result = new ArrayCreationExpression(null, Enumerable.Empty<Expression>(), inits,
                    context.GetTextSpan());
                return result;
            }

            if (context.Array() != null)
            {
                return new ArrayCreationExpression(null, Enumerable.Empty<Expression>(), Enumerable.Empty<Expression>(), context.GetTextSpan());
            }

            if (context.constantInititalizer() != null)
            {
                ITerminalNode opNode = context.GetChild<ITerminalNode>(0);
                var opLiteralType = UnaryOperatorLiteral.PrefixTextUnaryOperator[opNode.GetText()];
                var opLiteral = new UnaryOperatorLiteral(opLiteralType, opNode.GetTextSpan());
                var expression = (Expression)Visit(context.constantInititalizer());
                var result = new UnaryOperatorExpression(opLiteral, expression, context.GetTextSpan());
                return result;
            }

            return Visit(context.GetChild(0));
        }

        public Ust VisitConstantArrayItemList(PhpParser.ConstantArrayItemListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitConstantArrayItem(PhpParser.ConstantArrayItemContext context)
        {
            if (context.constantInititalizer().Length == 1)
            {
                return (Expression)Visit(context.constantInititalizer(0));
            }
            else
            {
                var result = new AssignmentExpression(
                    (Expression)Visit(context.constantInititalizer(0)),
                    (Expression)Visit(context.constantInititalizer(1)),
                    context.GetTextSpan());
                return result;
            }
        }

        public Ust VisitConstant(PhpParser.ConstantContext context)
        {
            if (context.Null() != null)
                return new NullLiteral(context.GetTextSpan());

            var result = Visit(context.GetChild(0));
            return result;
        }

        public Ust VisitLiteralConstant(PhpParser.LiteralConstantContext context)
        {
            Token result;
            var contextText = context.GetText();
            var contextSpan = context.GetTextSpan();

            if (context.Real() != null)
            {
                double value;
                if (!double.TryParse(contextText, out value))
                {
                    value = double.PositiveInfinity;
                }
                result = new FloatLiteral(value, contextSpan);
            }
            else if (context.BooleanConstant() != null)
            {
                bool value = bool.Parse(contextText);
                result = new BooleanLiteral(value, contextSpan);
            }
            else
            {
                result = (Token)Visit(context.GetChild(0));
            }

            return result;
        }

        public Ust VisitNumericConstant([NotNull] PhpParser.NumericConstantContext context)
        {
            ReadOnlySpan<char> span = ExtractSpan(context.GetChild<ITerminalNode>(0).Symbol, out TextSpan textSpan);

            int fromBase = context.Decimal() != null
                ? 10
                : context.Hex() != null
                    ? 16
                    : context.Octal() != null
                        ? 8
                        : 2;

            TryParseNumeric(span, textSpan, fromBase, out Literal numeric);
            return numeric;
        }

        public Ust VisitStringConstant(PhpParser.StringConstantContext context)
        {
            return new IdToken(context.GetText(), context.GetTextSpan());
        }

        public Ust VisitString(PhpParser.StringContext context)
        {
            Expression result;
            if (context.StartHereDoc() != null || context.StartNowDoc() != null)
            {
                IEnumerable<string> hereDocText = context.HereDocText().Select(c => c.GetText());
                var str = string.Join("", hereDocText).Trim();
                result = new StringLiteral(str, context.GetTextSpan(), 0);
            }
            else if (context.SingleQuoteString() != null)
            {
                result = TextUtils.GetStringLiteralWithoutQuotes(context.GetTextSpan(), root);
            }
            else
            {
                if (context.interpolatedStringPart().Length == 0)
                {
                    result = new StringLiteral("", context.GetTextSpan(), 0);
                }
                else
                {
                    result = (Expression)Visit(context.interpolatedStringPart(0));
                    for (int i = 1; i < context.interpolatedStringPart().Length; i++)
                    {
                        var right = (Expression)Visit(context.interpolatedStringPart(i));
                        result = new BinaryOperatorExpression(result,
                            new BinaryOperatorLiteral(BinaryOperator.Plus, default(TextSpan)),
                            right, result.TextSpan.Union(right.TextSpan));
                    }
                }
            }
            return result;
        }

        public Ust VisitInterpolatedStringPart([NotNull] PhpParser.InterpolatedStringPartContext context)
        {
            Expression result;
            if (context.StringPart() != null)
            {
                result = new StringLiteral(context.GetTextSpan(), root, 0); // TODO: escape length should be 1
            }
            else
            {
                result = (Expression)Visit(context.chain());
            }
            return result;
        }

        public Ust VisitClassConstant(PhpParser.ClassConstantContext context)
        {
            var target = (Token)Visit(context.GetChild(0));
            var targetId = new IdToken(target.TextValue, target.TextSpan);
            var name = (IdToken)Visit(context.GetChild(2));
            var result = new MemberReferenceExpression(targetId, name, context.GetTextSpan());
            return result;
        }

        public Ust VisitChainList(PhpParser.ChainListContext context)
        {
            Expression[] expressions = context.chain()
                .Select(c => (Expression)Visit(c))
                .Where(c => c != null).ToArray();
            var result = new ArgsUst(expressions, context.GetTextSpan());
            return result;
        }

        public Ust VisitChain(PhpParser.ChainContext context)
        {
            Expression result;
            Expression target;
            if (context.newExpr() != null)
            {
                target = (Expression)Visit(context.newExpr());
            }
            else
            {
                target = (Expression)Visit(context.GetChild(0));
            }

            if (context.memberAccess().Length > 0)
            {
                Expression[] memberAccesses = context.memberAccess().Select(memberAccess =>
                    (Expression)Visit(memberAccess))
                    .Where(m => m != null).ToArray();

                MemberReferenceExpression memberRefExpr = new MemberReferenceExpression();
                result = memberRefExpr;
                for (int i = memberAccesses.Length - 1; i >= 0; i--)
                {
                    Expression memberAccess = memberAccesses[i];
                    memberRefExpr.Name = memberAccess;
                    memberRefExpr.TextSpan = memberAccess.TextSpan;
                    if (i > 0)
                    {
                        memberRefExpr.Target = new MemberReferenceExpression();
                        memberRefExpr = (MemberReferenceExpression)memberRefExpr.Target;
                    }
                    else
                    {
                        memberRefExpr.Target = target;
                    }
                }

                result.TextSpan = context.GetTextSpan();
            }
            else
            {
                result = target;
            }
            return result;
        }

        public Ust VisitMemberAccess(PhpParser.MemberAccessContext context)
        {
            var fieldName = (Expression)Visit(context.keyedFieldName());
            if (context.actualArguments() == null)
            {
                return fieldName;
            }

            var arguments = (ArgsUst)Visit(context.actualArguments());
            return new InvocationExpression(fieldName, arguments, context.GetTextSpan());
        }

        public Ust VisitFunctionCall(PhpParser.FunctionCallContext context)
        {
            var target = (Expression)Visit(context.functionCallName());
            var args = (ArgsUst)Visit(context.actualArguments());

            var result = new InvocationExpression(target, args, context.GetTextSpan());
            return result;
        }

        public Ust VisitFunctionCallName(PhpParser.FunctionCallNameContext context)
        {
            Expression result;
            if (context.qualifiedNamespaceName() != null) // TODO: Fix QualifiedNamespaceName Type.
            {
                var strLit = (StringLiteral)Visit(context.qualifiedNamespaceName());
                result = new IdToken(strLit.TextValue, strLit.TextSpan);
            }
            else
            {
                result = (Expression)Visit(context.GetChild(0));
            }
            return result;
        }

        public Ust VisitActualArguments(PhpParser.ActualArgumentsContext context)
        {
            TypeToken genericArgs;
            if (context.genericDynamicArgs() != null)
            {
                genericArgs = (TypeToken)Visit(context.genericDynamicArgs());
            }
            var arguments = (ArgsUst)Visit(context.arguments());
            var exprs = ConvertSquareCurlyExpressions(context.squareCurlyExpression());
            exprs.InsertRange(0, arguments.Collection);

            var result = new ArgsUst(exprs, context.GetTextSpan());
            return result;
        }

        public Ust VisitChainBase(PhpParser.ChainBaseContext context)
        {
            Expression result;
            TextSpan textSpan = context.GetTextSpan();
            Expression target;
            if (context.qualifiedStaticTypeRef() != null)
            {
                target = (TypeToken)Visit(context.qualifiedStaticTypeRef());
                var name = (Expression)Visit(context.keyedVariable(0));
                result = new MemberReferenceExpression(target, name, textSpan);
            }
            else
            {
                target = (Expression)Visit(context.keyedVariable(0));
                if (context.keyedVariable().Length == 1)
                {
                    if (target is IdToken idToken && idToken.TextValue == "this")
                    {
                        return new ThisReferenceToken(textSpan);
                    }
                    result = target;
                }
                else
                {
                    var name = (Expression)Visit(context.keyedVariable(1));
                    result = new MemberReferenceExpression(target, name, textSpan);
                }
            }
            return result;
        }

        public Ust VisitKeyedFieldName(PhpParser.KeyedFieldNameContext context)
        {
            return Visit(context.GetChild(0));
        }

        public Ust VisitKeyedSimpleFieldName(PhpParser.KeyedSimpleFieldNameContext context)
        {
            List<Expression> exprs = ConvertSquareCurlyExpressions(context.squareCurlyExpression());

            IdToken result;
            if (context.identifier() != null)
            {
                result = (IdToken)Visit(context.identifier());
            }
            else
            {
                result = new IdToken(CommonUtils.Prefix + "expressionId", context.GetTextSpan());
                exprs.Insert(0, (Expression)Visit(context.expression()));
            }

            return result;
        }

        public Ust VisitKeyedVariable(PhpParser.KeyedVariableContext context)
        {
            Expression left;
            if (context.VarName() != null)
            {
                left = ConvertVar(context.VarName());
            }
            else
            {
                left = (Expression)Visit(context.expression());
            }

            Expression result;
            if (context.squareCurlyExpression().Length == 0)
            {
                result = left;
            }
            else
            {
                List<Expression> exprs = ConvertSquareCurlyExpressions(context.squareCurlyExpression());
                var args = new ArgsUst(exprs);
                result = new IndexerExpression(left, args, context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitSquareCurlyExpression(PhpParser.SquareCurlyExpressionContext context)
        {
            return context.expression() != null ? Visit(context.expression()) : null;
        }

        public Ust VisitAssignmentList(PhpParser.AssignmentListContext context)
        {
            Expression[] exps = context.assignmentListElement()
                .Select(elem => (Expression)Visit(elem))
                .Where(elem => elem != null)
                .ToArray();
            var result = new ArgsUst(exps, context.GetTextSpan());
            return result;
        }

        public Ust VisitAssignmentListElement(PhpParser.AssignmentListElementContext context)
        {
            if (context.chain() != null)
            {
                return (Expression)Visit(context.chain());
            }
            else
            {
                var target = new IdToken(context.List().GetText(), context.List().GetTextSpan());
                var args = (ArgsUst)Visit(context.assignmentList());
                var result = new InvocationExpression(target, args, context.GetTextSpan());
                return result;
            }
        }

        public Ust VisitModifier(PhpParser.ModifierContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIdentifier(PhpParser.IdentifierContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan());
            return result;
        }

        public Ust VisitMemberModifier(PhpParser.MemberModifierContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMagicConstant(PhpParser.MagicConstantContext context)
        {
            return new IdToken(context.GetText(), context.GetTextSpan());
        }

        public Ust VisitMagicMethod(PhpParser.MagicMethodContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan());
            return result;
        }

        public Ust VisitPrimitiveType(PhpParser.PrimitiveTypeContext context)
        {
            var result = new TypeToken(context.GetText(), context.GetTextSpan());
            return result;
        }

        public Ust VisitCastOperation(PhpParser.CastOperationContext context)
        {
            var result = new TypeToken(context.GetText(), context.GetTextSpan());
            return result;
        }

        public override Ust VisitTerminal(ITerminalNode node)
        {
            return new StringLiteral(node.GetTextSpan(), root, 0);
        }
    }
}
