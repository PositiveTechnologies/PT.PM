using System;
using System.Linq;
using PT.PM.AntlrUtils;
using PT.PM.Common.Nodes;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.TypeMembers;
using Antlr4.Runtime.Misc;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Text;

namespace PT.PM.PhpParseTreeUst
{
    public partial class PhpAntlrParseTreeConverter : AntlrConverter, IPhpParserVisitor<UstNode>
    {
        protected const string namespacePrefix = Helper.Prefix + "ns";
        protected const string elementNamespacePrefix = Helper.Prefix + "elemNs";
        protected const string contentNamespacePrefix = Helper.Prefix + "contentNs";
        protected const string attrNamespacePrefix = Helper.Prefix + "attrNs";
        protected const string inlineHtmlNamespacePrefix = Helper.Prefix + "inlineHtml";

        protected int jsStartCodeInd = 0;
        protected int namespaceDepth;

        public override Language Language => Language.Php;

        public PhpAntlrParseTreeConverter()
            : base()
        {
            namespaceDepth = 0;
        }

        public UstNode VisitHtmlDocument(PhpParser.HtmlDocumentContext context)
        {
            IEnumerable<UstNode> phpBlocks = context.htmlElementOrPhpBlock()
                .Select(block => Visit(block))
                .Where(phpBlock => phpBlock != null);
            root.Nodes = phpBlocks.SelectMany(block => block.SelectAnalyzedNodes(Language.Php, AnalyzedLanguages)).ToArray();

            return root;
        }

        public UstNode VisitHtmlElementOrPhpBlock(PhpParser.HtmlElementOrPhpBlockContext context)
        {
            UstNode result = null;
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

        public UstNode VisitHtmlElements(PhpParser.HtmlElementsContext context)
        {
            var text = new StringBuilder();
            for (int i = context.Start.TokenIndex; i < context.Stop.TokenIndex; i++)
            {
                IToken token = Tokens[i];

                if (AnalyzedLanguages.Contains(Language.JavaScript))
                {
                    if (token.Type == PhpLexer.HtmlScriptOpen)
                    {
                        jsStartCodeInd = context.Start.TokenIndex;
                    }
                    else if (token.Type == PhpLexer.ScriptClose)
                    {
                        return ConvertJavaScript(context);
                    }
                }

                text.Append(token.Text);
            }

            var result = AnalyzedLanguages.Contains(Language.Html)
                ? new RootNode(root.SourceCodeFile, Language.Html) { Node = new StringLiteral(text.ToString(), context.GetTextSpan()) }
                : null;
            return result;
        }

        private RootNode ConvertJavaScript(PhpParser.HtmlElementsContext context)
        {
            int jsStopCodeInd = context.start.TokenIndex;
            var jsCode = new StringBuilder();
            int wsLength = 0;
            for (int j = jsStartCodeInd; j < jsStopCodeInd; j++)
            {
                if (Tokens[j].Type == PhpLexer.ScriptText)
                {
                    jsCode.Append(Tokens[j].Text);
                }
                else
                {
                    wsLength = Tokens[j].Text.Length;
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

            var javaScriptParser = new JavaScriptAntlrParser
            {
                Logger = Logger,
                LineOffset = Tokens[jsStartCodeInd].Line - 1
            };
            var sourceCodeFile = new SourceCodeFile
            {
                Name = root.SourceCodeFile.Name,
                RelativePath = root.SourceCodeFile.RelativePath,
                Code = jsCode.ToString()
            };
            var parseTree = (JavaScriptAntlrParseTree)javaScriptParser.Parse(sourceCodeFile);

            var javaScriptConverter = new JavaScriptParseTreeConverter() { Logger = Logger, ParentRoot = root };
            RootNode result = javaScriptConverter.Convert(parseTree);
            result.SourceCodeFile = root.SourceCodeFile;
            result.Root = root;
            result.Parent = root;
            int jsCodeOffset = Tokens[jsStartCodeInd].StartIndex;
            result.ApplyActionToDescendants(ustNode => ustNode.TextSpan = ustNode.TextSpan.AddOffset(jsCodeOffset));
            return result;
        }

        private char GetLastNotWhitespace(StringBuilder builder)
        {
            int ind = builder.Length - 1;
            while (ind > 0 && char.IsWhiteSpace(builder[ind]))
                ind--;

            return ind > 0 ? builder[ind] : '\0';
        }

        public UstNode VisitHtmlElement(PhpParser.HtmlElementContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitScriptTextPart(PhpParser.ScriptTextPartContext context)
        {
            string javaScriptCode = string.Join("", context.ScriptText().Select(text => text.GetText()));
            UstNode result;
            if (AnalyzedLanguages.Contains(Language.JavaScript))
            {
                // Process JavaScript at close script tag </script>
                result = null;
            }
            else
            {
                result = new StringLiteral(javaScriptCode, context.GetTextSpan());
            }
            return result;
        }

        public UstNode VisitPhpBlock(PhpParser.PhpBlockContext context)
        {
            TextSpan textSpan = context.GetTextSpan();
            var namespaceName = new StringLiteral(Helper.Prefix + "default", textSpan);
            UsingDeclaration[] usingDeclarations = context.importStatement()
                .Select(importStatement => (UsingDeclaration)Visit(importStatement))
                .Where(stmt => stmt != null)
                .ToArray();

            Statement[] topStatements = context.topStatement()
                .Select(topStatement => (Statement)Visit(topStatement))
                .Where(stmt => stmt != null)
                .ToArray();
            var statementsNode = new BlockStatement(topStatements, default(TextSpan));
            var members = new List<UstNode>();
            members.AddRange(usingDeclarations);
            members.Add(statementsNode);

            var result = new NamespaceDeclaration(namespaceName, members, textSpan);
            return result;
        }

        public UstNode VisitImportStatement(PhpParser.ImportStatementContext context)
        {
            var namespaceName = (StringLiteral)Visit(context.namespaceNameList());

            var result = new UsingDeclaration(namespaceName, context.GetTextSpan());
            return result;
        }

        public UstNode VisitTopStatement(PhpParser.TopStatementContext context)
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

        public UstNode VisitUseDeclaration(PhpParser.UseDeclarationContext context)
        {
            var result = new UsingDeclaration(
                new StringLiteral(context.useDeclarationContentList().GetText(), context.useDeclarationContentList().GetTextSpan()),
                context.GetTextSpan());
            return result;
        }

        public UstNode VisitUseDeclarationContentList(PhpParser.UseDeclarationContentListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitUseDeclarationContent(PhpParser.UseDeclarationContentContext context)
        {
            var namespaceNameListUstNode = (StringLiteral)Visit(context.namespaceNameList());

            var result = new UsingDeclaration(namespaceNameListUstNode, context.GetTextSpan());
            return result;
        }

        public UstNode VisitNamespaceDeclaration(PhpParser.NamespaceDeclarationContext context)
        {
            StringLiteral name;
            if (context.namespaceNameList() != null)
            {
                name = (StringLiteral)Visit(context.namespaceNameList());
            }
            else
            {
                name = new StringLiteral(Helper.Prefix + "unnamed", default(TextSpan));
            }

            UstNode[] members = context.namespaceStatement()
                .Select(statement => Visit(statement))
                .Where(statement => statement != null)
                .ToArray();

            var result = new NamespaceDeclaration(name, members, context.GetTextSpan());
            return result;
        }

        public UstNode VisitNamespaceStatement(PhpParser.NamespaceStatementContext context)
        {
            return Visit(context.GetChild(0));
        }

        public UstNode VisitFunctionDeclaration(PhpParser.FunctionDeclarationContext context)
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

        public UstNode VisitClassDeclaration(PhpParser.ClassDeclarationContext context)
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

        public UstNode VisitClassEntryType(PhpParser.ClassEntryTypeContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitInterfaceList(PhpParser.InterfaceListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterListInBrackets(PhpParser.TypeParameterListInBracketsContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterList(PhpParser.TypeParameterListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterWithDefaultsList(PhpParser.TypeParameterWithDefaultsListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterDecl(PhpParser.TypeParameterDeclContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterWithDefaultDecl(PhpParser.TypeParameterWithDefaultDeclContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitGenericDynamicArgs(PhpParser.GenericDynamicArgsContext context)
        {
            string[] typeRefs = context.typeRef()
                .Select(type => ((TypeToken)Visit(type))?.TypeText)
                .Where(type => type != null).ToArray();

            var str = context.GetChild(0).GetText() + string.Join(",", typeRefs) + context.GetChild(context.ChildCount - 1).GetText();
            var result = new TypeToken(str, context.GetTextSpan());
            return result;
        }

        public UstNode VisitAttributes(PhpParser.AttributesContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttributesGroup(PhpParser.AttributesGroupContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttribute(PhpParser.AttributeContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttributeArgList(PhpParser.AttributeArgListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttributeNamedArgList(PhpParser.AttributeNamedArgListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttributeNamedArg(PhpParser.AttributeNamedArgContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitInnerStatementList(PhpParser.InnerStatementListContext context)
        {
            List<Statement> innerStatementUstNodes = context.innerStatement()
                .Select(c => (Statement)Visit(c))
                .Where(c => c != null)
                .ToList();

            var result = new BlockStatement(innerStatementUstNodes, 
                context.innerStatement().Length > 0 ? context.GetTextSpan() : default(TextSpan));
            return result;
        }

        public UstNode VisitInnerStatement(PhpParser.InnerStatementContext context)
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

        public UstNode VisitStatement(PhpParser.StatementContext context)
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

        public UstNode VisitEmptyStatement(PhpParser.EmptyStatementContext context)
        {
            return new EmptyStatement(context.GetTextSpan());
        }

        public UstNode VisitBlockStatement(PhpParser.BlockStatementContext context)
        {
            var innerStatementListUstNode = (BlockStatement)Visit(context.innerStatementList());

            var result = new BlockStatement(innerStatementListUstNode.Statements, context.GetTextSpan());
            return result;
        }

        public UstNode VisitIfStatement(PhpParser.IfStatementContext context)
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

        public UstNode VisitElseIfStatement(PhpParser.ElseIfStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            var statement = (Statement)Visit(context.statement());

            var result = new IfElseStatement(condition, statement, context.GetTextSpan());
            return result;
        }

        public UstNode VisitElseIfColonStatement(PhpParser.ElseIfColonStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            var statement = (Statement)Visit(context.innerStatementList());

            var result = new IfElseStatement(condition, statement, context.GetTextSpan());
            return result;
        }

        public UstNode VisitElseStatement(PhpParser.ElseStatementContext context)
        {
            var statement = (Statement)Visit(context.statement());
            return statement;
        }

        public UstNode VisitElseColonStatement(PhpParser.ElseColonStatementContext context)
        {
            var statement = (Statement)Visit(context.innerStatementList());
            return statement;
        }

        public UstNode VisitWhileStatement(PhpParser.WhileStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            Statement statement = context.statement() != null
                ? (Statement)Visit(context.statement())
                : (Statement)Visit(context.innerStatementList());

            var result = new WhileStatement(condition, statement, context.GetTextSpan());
            return result;
        }

        public UstNode VisitDoWhileStatement(PhpParser.DoWhileStatementContext context)
        {
            var statement = (Statement)Visit(context.statement());
            var condition = (Expression)Visit(context.parenthesis());

            var result = new DoWhileStatement(statement, condition, context.GetTextSpan());
            return result;
        }

        public UstNode VisitForStatement(PhpParser.ForStatementContext context)
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

        public UstNode VisitForInit(PhpParser.ForInitContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitForUpdate(PhpParser.ForUpdateContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitSwitchStatement(PhpParser.SwitchStatementContext context)
        {
            var expression = (Expression)Visit(context.parenthesis());
            SwitchSection[] switchBlocks = context.switchBlock()
                .Select(block => (SwitchSection)Visit(block))
                .Where(block => block != null)
                .ToArray();

            var result = new SwitchStatement(expression, switchBlocks, context.GetTextSpan());
            return result;
        }

        public UstNode VisitSwitchBlock(PhpParser.SwitchBlockContext context)
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

        public UstNode VisitBreakStatement(PhpParser.BreakStatementContext context)
        {
            var result = new BreakStatement(context.GetTextSpan())
            {
                Expression = context.expression() == null ? null : (Expression)Visit(context.expression())
            };
            return result;
        }

        public UstNode VisitContinueStatement(PhpParser.ContinueStatementContext context)
        {
            var result = new ContinueStatement(context.GetTextSpan())
            {
                Expression = context.expression() == null ? null : (Expression)Visit(context.expression())
            };
            return result;
        }

        public UstNode VisitReturnStatement(PhpParser.ReturnStatementContext context)
        {
            var returnExpression = context.expression() == null ? null : (Expression)Visit(context.expression());
            var result = new ReturnStatement(returnExpression, context.GetTextSpan());
            return result;
        }

        public UstNode VisitExpressionStatement(PhpParser.ExpressionStatementContext context)
        {
            var expression = (Expression)Visit(context.expression());
            var result = new ExpressionStatement(expression, context.GetTextSpan());
            return result;
        }

        public UstNode VisitUnsetStatement(PhpParser.UnsetStatementContext context)
        {
            var args = (ArgsNode)Visit(context.chainList());
            var invocation = new InvocationExpression(
                new IdToken(context.Unset().GetText(), context.Unset().GetTextSpan()),
                args, context.GetTextSpan());
            var result = new ExpressionStatement(invocation, context.GetTextSpan());
            return result;
        }

        public UstNode VisitForeachStatement(PhpParser.ForeachStatementContext context)
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
                 var assignmentList = (ArgsNode)Visit(context.assignmentList());
                 expressions.AddRange(assignmentList.Collection);
            }

            var inExpression = new MultichildExpression(expressions, context.GetTextSpan()); // TODO: Spans union

            Statement statement = context.statement() != null
                ? (Statement)Visit(context.statement())
                : (Statement)Visit(context.innerStatementList());

            var result = new ForeachStatement(null, null, inExpression, statement, context.GetTextSpan());
            return result;
        }

        public UstNode VisitTryCatchFinally(PhpParser.TryCatchFinallyContext context)
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

        public UstNode VisitCatchClause(PhpParser.CatchClauseContext context)
        {
            var type = (TypeToken)Visit(context.qualifiedStaticTypeRef());
            var varName = (IdToken)ConvertVar(context.VarName());
            var body = (BlockStatement)Visit(context.blockStatement());

            var result = new CatchClause(type, varName, body, context.GetTextSpan());
            return result;
        }

        public UstNode VisitFinallyStatement(PhpParser.FinallyStatementContext context)
        {
            var result = (BlockStatement)Visit(context.blockStatement());
            return result;
        }

        public UstNode VisitThrowStatement(PhpParser.ThrowStatementContext context)
        {
            var expression = (Expression)Visit(context.expression());
            var result = new ThrowStatement(expression, context.GetTextSpan());
            return result;
        }

        public UstNode VisitGotoStatement(PhpParser.GotoStatementContext context)
        {
            return new EmptyStatement(context.GetTextSpan());
        }

        public UstNode VisitDeclareStatement(PhpParser.DeclareStatementContext context)
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

        public UstNode VisitDeclareList(PhpParser.DeclareListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitInlineHtmlStatement([NotNull] PhpParser.InlineHtmlStatementContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitInlineHtml(PhpParser.InlineHtmlContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFormalParameterList(PhpParser.FormalParameterListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitFormalParameter(PhpParser.FormalParameterContext context)
        {
            TypeToken type = null;
            if (context.typeHint() != null)
                type = (TypeToken)Visit(context.typeHint());

            var varInit = (AssignmentExpression)Visit(context.variableInitializer());

            var result = new ParameterDeclaration(type, (IdToken)varInit.Left, context.GetTextSpan());
            result.Initializer = varInit.Right;
            return result;
        }

        public UstNode VisitTypeHint(PhpParser.TypeHintContext context)
        {
            if (context.Callable() != null)
                return new TypeToken(context.GetText(), context.GetTextSpan());

            return (TypeToken)Visit(context.GetChild(0));
        }

        public UstNode VisitGlobalStatement(PhpParser.GlobalStatementContext context)
        {
            Expression[] globalVars = context.globalVar()
                .Select(globalVar => (Expression)Visit(globalVar))
                .Where(v => v != null).ToArray();
            var multichild = new MultichildExpression(globalVars, context.GetTextSpan());

            var result = new ExpressionStatement(multichild, context.GetTextSpan());
            return result;
        }

        public UstNode VisitGlobalVar(PhpParser.GlobalVarContext context)
        {
            if (context.VarName() != null)
                return (IdToken)ConvertVar(context.VarName());

            if (context.chain() != null)
                return (Expression)Visit(context.chain());

            return (Expression)Visit(context.expression());
        }

        public UstNode VisitEchoStatement(PhpParser.EchoStatementContext context)
        {
            var name = new IdToken(context.Echo().GetText(), context.GetTextSpan());
            var args = new ArgsNode(((MultichildExpression)Visit(context.expressionList())).Expressions.ToList(),
                context.expressionList().GetTextSpan());
            var invocation = new InvocationExpression(name, args, context.GetTextSpan());
            var result = new ExpressionStatement(invocation, context.GetTextSpan());
            return result;
        }

        public UstNode VisitStaticVariableStatement(PhpParser.StaticVariableStatementContext context)
        {
            AssignmentExpression[] variables = context.variableInitializer()
                .Select(varInit => (AssignmentExpression)Visit(varInit))
                .Where(v => v != null).ToArray();
            var type = new TypeToken(context.Static().GetText(), context.Static().GetTextSpan());

            var result = new VariableDeclarationExpression(type, variables, context.GetTextSpan());
            return result;
        }

        public UstNode VisitClassStatement(PhpParser.ClassStatementContext context)
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

        public UstNode VisitTraitAdaptations(PhpParser.TraitAdaptationsContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTraitAdaptationStatement(PhpParser.TraitAdaptationStatementContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTraitPrecedence(PhpParser.TraitPrecedenceContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTraitAlias(PhpParser.TraitAliasContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTraitMethodReference(PhpParser.TraitMethodReferenceContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitBaseCtorCall(PhpParser.BaseCtorCallContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitMethodBody(PhpParser.MethodBodyContext context)
        {
            if (context.blockStatement() != null)
                return (BlockStatement)Visit(context.blockStatement());

            return null;
        }

        public UstNode VisitPropertyModifiers(PhpParser.PropertyModifiersContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitMemberModifiers(PhpParser.MemberModifiersContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitVariableInitializer(PhpParser.VariableInitializerContext context)
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

        public UstNode VisitIdentifierInititalizer(PhpParser.IdentifierInititalizerContext context)
        {
            var id = (IdToken)Visit(context.identifier());
            var initializer = (Expression)Visit(context.constantInititalizer());

            var result = new AssignmentExpression(id, initializer, context.GetTextSpan());
            return result;
        }

        public UstNode VisitGlobalConstantDeclaration(PhpParser.GlobalConstantDeclarationContext context)
        {
            AssignmentExpression[] identifiers = context.identifierInititalizer()
                .Select(id => (AssignmentExpression)Visit(id))
                .Where(id => id != null).ToArray();

            var result = new FieldDeclaration(identifiers, context.GetTextSpan());
            return result;
        }

        public UstNode VisitExpressionList(PhpParser.ExpressionListContext context)
        {
            Expression[] expressions = context.expression().Select(c => (Expression)Visit(c))
                .Where(c => c != null).ToArray();

            return new MultichildExpression(expressions, context.GetTextSpan());
        }

        public UstNode VisitParenthesis(PhpParser.ParenthesisContext context)
        {
            if (context.expression() != null)
                return Visit(context.expression());

            return Visit(context.yieldExpression());
        }

        public UstNode VisitExpression([NotNull] PhpParser.ExpressionContext context)
        {
            return Visit(context);
        }

        public UstNode VisitNewExpr(PhpParser.NewExprContext context)
        {
            var type = (TypeToken)Visit(context.typeRef());
            ArgsNode args = context.arguments() != null
                ? (ArgsNode)Visit(context.arguments())
                : new ArgsNode();

            var result = new ObjectCreateExpression(type, args, context.GetTextSpan());
            return result;
        }

        public UstNode VisitConditionalExpression([NotNull] PhpParser.ConditionalExpressionContext context)
        {
            var expression0 = (Expression)Visit(context.expression(0));
            var expression1 = (Expression)(context.expression().Length == 3 ? Visit(context.expression(1)) : null);
            var expression2 = (Expression)Visit(context.expression().Last());
            var result = new ConditionalExpression(expression0, expression1, expression2, context.GetTextSpan());
            return result;
        }

        public UstNode VisitLogicalExpression([NotNull] PhpParser.LogicalExpressionContext context)
        {
            return CreateBinaryOperatorExpression(context.expression(0), context.op, context.expression(1));
        }

        public UstNode VisitArithmeticExpression([NotNull] PhpParser.ArithmeticExpressionContext context)
        {
            return CreateBinaryOperatorExpression(context.expression(0), context.op, context.expression(1));
        }

        public UstNode VisitInstanceOfExpression([NotNull] PhpParser.InstanceOfExpressionContext context)
        {
            return (Expression)Visit(context.expression()); // TODO: InstanceOf
        }

        public UstNode VisitBitwiseExpression([NotNull] PhpParser.BitwiseExpressionContext context)
        {
            Expression result = CreateBinaryOperatorExpression(context.expression(0), context.op, context.expression(1));
            return result;
        }

        public UstNode VisitComparisonExpression([NotNull] PhpParser.ComparisonExpressionContext context)
        {
            Expression result = CreateBinaryOperatorExpression(context.expression(0), context.op, context.expression(1));
            return result;
        }

        public UstNode VisitCloneExpression(PhpParser.CloneExpressionContext context)
        {
            return CreateSpecialInvocation(context.Clone(), context.expression(), context.GetTextSpan());
        }

        public UstNode VisitNewExpression(PhpParser.NewExpressionContext context)
        {
            return (Expression)Visit(context.newExpr());
        }

        public UstNode VisitIndexerExpression(PhpParser.IndexerExpressionContext context)
        {
            var target = (Expression)Visit(context.stringConstant());
            var arg = (Expression)Visit(context.expression());

            var result = new IndexerExpression(target, new ArgsNode(new[] { arg }), context.GetTextSpan());
            return result;
        }

        public UstNode VisitPrefixIncDecExpression(PhpParser.PrefixIncDecExpressionContext context)
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

        public UstNode VisitPostfixIncDecExpression(PhpParser.PostfixIncDecExpressionContext context)
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
        
        public UstNode VisitCastExpression(PhpParser.CastExpressionContext context)
        {
            var castType = (TypeToken)Visit(context.castOperation());
            var expression = (Expression)Visit(context.expression());

            var result = new CastExpression(castType, expression, context.GetTextSpan());
            return result;
        }
        
        public UstNode VisitUnaryOperatorExpression(PhpParser.UnaryOperatorExpressionContext context)
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
        
        public UstNode VisitAssignmentExpression(PhpParser.AssignmentExpressionContext context)
        {
            var left = (Expression)Visit(context.chain(0));

            Expression result;
            if (context.ChildCount == 3)
            {
                ParserRuleContext operatorTerminal = (ParserRuleContext)context.GetChild(1);
                string binaryOperatorText = operatorTerminal.GetText();
                var right = (Expression)Visit(context.expression());

                if (binaryOperatorText == "=")
                {
                    result = new AssignmentExpression(left, right, context.GetTextSpan());
                }
                else
                {
                    BinaryOperator binaryOperator;
                    if (binaryOperatorText == ".=")
                    {
                        binaryOperator = BinaryOperator.Plus;
                    }
                    else if (binaryOperatorText == "**=")
                    {
                        binaryOperator = BinaryOperator.Multiply; // TODO: fix
                    }
                    else
                    {
                        binaryOperator = BinaryOperatorLiteral.TextBinaryOperator[binaryOperatorText.Remove(binaryOperatorText.Length - 1)];
                    }

                    // TODO: implement assignment + operator
                    result = new AssignmentExpression(left, right, context.GetTextSpan());
                }
            }
            else
            {
                result = new AssignmentExpression(left,
                    (Expression)Visit(context.GetChild(3)), context.GetTextSpan());
            }
            return result;
        }

        public UstNode VisitAssignmentOperator(PhpParser.AssignmentOperatorContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitPrintExpression(PhpParser.PrintExpressionContext context)
        {
            return (Expression)CreateSpecialInvocation(context.Print(), context.expression(), context.GetTextSpan());
        }
        
        public UstNode VisitChainExpression(PhpParser.ChainExpressionContext context)
        {
            return (Expression)Visit(context.GetChild(0));
        }
        
        public UstNode VisitScalarExpression(PhpParser.ScalarExpressionContext context)
        {
            if (context.Label() != null)
            {
                return new IdToken(context.GetText(), context.GetTextSpan());
            }

            return (Expression)Visit(context.GetChild(0));
        }
        
        public UstNode VisitBackQuoteStringExpression(PhpParser.BackQuoteStringExpressionContext context)
        {
            return Visit(context.BackQuoteString());
        }
        
        public UstNode VisitParenthesisExpression(PhpParser.ParenthesisExpressionContext context)
        {
            return (Expression)Visit(context.parenthesis());
        }

        public UstNode VisitArrayCreationExpression(PhpParser.ArrayCreationExpressionContext context)
        {
            List<Expression> inits = context.arrayItemList().arrayItem()
                .Select(item => (Expression)Visit(item))
                .Where(item => item != null).ToList();
            if (context.expression() != null)
                inits.Add((Expression)Visit(context.expression()));

            var result = new ArrayCreationExpression(null, Enumerable.Empty<Expression>(), inits,
                context.GetTextSpan());
            return result;
        }

        public UstNode VisitSpecialWordExpression(PhpParser.SpecialWordExpressionContext context)
        {
            TextSpan textSpan = context.GetTextSpan();
            Expression expression, result;

            if (context.Yield() != null)
            {
                result = new InvocationExpression(
                    new IdToken(context.Yield().GetText(), context.Yield().GetTextSpan()),
                    new ArgsNode(), context.GetTextSpan());
            }
            else if (context.List() != null)
            {
                var args = (ArgsNode)Visit(context.assignmentList());
                expression = (Expression)Visit(context.expression());

                TextSpan listTextSpan = context.List().GetTextSpan();
                var invoke = new InvocationExpression(new IdToken(context.List().GetText(), listTextSpan),
                    args, listTextSpan.Union(context.GetChild<ITerminalNode>(2).GetTextSpan()));
                result = new AssignmentExpression(invoke, expression, textSpan);
            }
            else if (context.IsSet() != null)
            {
                var args = (ArgsNode)Visit(context.chainList());
                result = new InvocationExpression(
                    new IdToken(context.IsSet().GetText(), context.IsSet().GetTextSpan()),
                    args, textSpan);
            }
            else if (context.Empty() != null)
            {
                var args = new ArgsNode(new[] { (Expression)Visit(context.chain()) });
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
                    new ArgsNode(exprs), textSpan);
            }
            else
            {
                expression = (Expression)Visit(context.expression());
                result = new InvocationExpression(
                    new IdToken(context.GetChild(0).GetText()),
                    new ArgsNode(new Expression[] { expression }), textSpan);
            }
            return result;
        }

        public UstNode VisitLambdaFunctionExpression(PhpParser.LambdaFunctionExpressionContext context)
        {
            ParameterDeclaration[] parameters = ConvertParameters(context.formalParameterList());
            var body = (BlockStatement)Visit(context.blockStatement());

            var result = new AnonymousMethodExpression(parameters, body, context.GetTextSpan());
            return result;
        }

        public UstNode VisitYieldExpression(PhpParser.YieldExpressionContext context)
        {
            var expressions = context.expression()
                .Select(expression => (Expression)Visit(expression))
                .Where(e => e != null)
                .ToArray();

            var result = new MultichildExpression(expressions, context.GetTextSpan());
            return result;
        }

        public UstNode VisitArrayItemList(PhpParser.ArrayItemListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitArrayItem(PhpParser.ArrayItemContext context)
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

        public UstNode VisitLambdaFunctionUseVars(PhpParser.LambdaFunctionUseVarsContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitLambdaFunctionUseVar(PhpParser.LambdaFunctionUseVarContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitQualifiedStaticTypeRef(PhpParser.QualifiedStaticTypeRefContext context)
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
                return new TypeToken(typeStr.Text + genericStr, textSpan);
            }

            var result = new TypeToken(context.Static().GetText(), textSpan);
            return result;
        }

        public UstNode VisitTypeRef(PhpParser.TypeRefContext context)
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
                result = new TypeToken(str.Text + genericStr, textSpan);
            }
            else if (context.indirectTypeRef() != null)
            {
                var indirectTypeRef = (Expression)Visit(context.indirectTypeRef());
                result = new TypeToken(indirectTypeRef.ToString() + genericStr, textSpan);
                result.Expression = indirectTypeRef;
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

        public UstNode VisitIndirectTypeRef(PhpParser.IndirectTypeRefContext context)
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

        public UstNode VisitQualifiedNamespaceName(PhpParser.QualifiedNamespaceNameContext context)
        {
            var result = (StringLiteral)Visit(context.namespaceNameList());
            return result;
        }

        public UstNode VisitNamespaceNameList(PhpParser.NamespaceNameListContext context)
        {
            var result = new StringLiteral(context.GetText(), context.GetTextSpan());
            return result;
        }

        public UstNode VisitQualifiedNamespaceNameList(PhpParser.QualifiedNamespaceNameListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitArguments(PhpParser.ArgumentsContext context)
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
            var result = new ArgsNode(args, context.GetTextSpan());
            return result;
        }

        public UstNode VisitActualArgument(PhpParser.ActualArgumentContext context)
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

        public UstNode VisitConstantInititalizer(PhpParser.ConstantInititalizerContext context)
        {
            if (context.constantArrayItemList() != null)
            {
                List<Expression> inits = context.constantArrayItemList().constantArrayItem()
                    .Select(item => (Expression)Visit(item))
                    .Where(item => item != null).ToList();
                var result = new ArrayCreationExpression(null, Enumerable.Empty<Expression>(), inits,
                    context.GetTextSpan());
                return result;
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
        
        public UstNode VisitConstantArrayItemList(PhpParser.ConstantArrayItemListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitConstantArrayItem(PhpParser.ConstantArrayItemContext context)
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

        public UstNode VisitConstant(PhpParser.ConstantContext context)
        {
            if (context.Null() != null)
                return new NullLiteral(context.GetTextSpan());

            var result = Visit(context.GetChild(0));
            return result;
        }

        public UstNode VisitLiteralConstant(PhpParser.LiteralConstantContext context)
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

        public UstNode VisitNumericConstant([NotNull] PhpParser.NumericConstantContext context)
        {
            string text = context.GetText();
            Token result;
            try
            {
                long resultValue = 0;
                if (context.Octal() != null)
                {
                    resultValue = System.Convert.ToInt64(text, 8);
                }
                else if (context.Decimal() != null)
                {
                    resultValue = long.Parse(text);
                }
                else if (context.Hex() != null)
                {
                    resultValue = System.Convert.ToInt64(text, 16);
                }
                else if (context.Binary() != null)
                {
                    resultValue = System.Convert.ToInt64(text.Substring(2), 2);
                }
                result = new IntLiteral(resultValue, context.GetTextSpan());
            }
            catch
            {
                result = new FloatLiteral(double.PositiveInfinity, context.GetTextSpan());
            }
            return result;
        }

        public UstNode VisitStringConstant(PhpParser.StringConstantContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan());
            return result;
        }

        public UstNode VisitString(PhpParser.StringContext context)
        {
            Expression result;
            if (context.StartHereDoc() != null || context.StartNowDoc() != null)
            {
                IEnumerable<string> hereDocText = context.HereDocText().Select(c => c.GetText());
                var str = string.Join("", hereDocText).Trim();
                result = new StringLiteral(str, context.GetTextSpan());
            }
            else if (context.SingleQuoteString() != null)
            {
                var text = context.GetText();
                result = new StringLiteral(text.Substring(1, text.Length - 2), context.GetTextSpan());
            }
            else
            {
                if (context.interpolatedStringPart().Length == 0)
                {
                    result = new StringLiteral("", context.GetTextSpan());
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

        public UstNode VisitInterpolatedStringPart([NotNull] PhpParser.InterpolatedStringPartContext context)
        {
            Expression result;
            if (context.StringPart() != null)
            {
                result = new StringLiteral(context.StringPart().GetText(), context.GetTextSpan());
            }
            else
            {
                result = (Expression)Visit(context.chain());
            }
            return result;
        }

        public UstNode VisitClassConstant(PhpParser.ClassConstantContext context)
        {
            var target = (Token)Visit(context.GetChild(0));
            var targetId = new IdToken(target.TextValue, target.TextSpan);
            var name = (IdToken)Visit(context.GetChild(2));
            var result = new MemberReferenceExpression(targetId, name, context.GetTextSpan());
            return result;
        }

        public UstNode VisitChainList(PhpParser.ChainListContext context)
        {
            Expression[] expressions = context.chain()
                .Select(c => (Expression)Visit(c))
                .Where(c => c != null).ToArray();
            var result = new ArgsNode(expressions, context.GetTextSpan());
            return result;
        }

        public UstNode VisitChain(PhpParser.ChainContext context)
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
                IdToken[] memberAccesses = context.memberAccess().Select(memberAccess =>
                    (IdToken)Visit(memberAccess))
                    .Where(m => m != null).ToArray();

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

                memberRefExpr.Target = target;
                memberRefExpr.TextSpan = context.GetTextSpan();

                result = memberRefExpr;
            }
            else
            {
                result = target;
            }
            return result;
        }

        public UstNode VisitMemberAccess(PhpParser.MemberAccessContext context)
        {
            MultichildExpression expression = null;
            if (context.actualArguments() != null)
            {
                var args = (ArgsNode)Visit(context.actualArguments());
                expression = new MultichildExpression(args.Collection, args.TextSpan);
            }
            var result = (IdToken)Visit(context.keyedFieldName());
            result.Expression = expression;
            return result;
        }

        public UstNode VisitFunctionCall(PhpParser.FunctionCallContext context)
        {
            var target = (Expression)Visit(context.functionCallName());
            var args = (ArgsNode)Visit(context.actualArguments());

            var result = new InvocationExpression(target, args, context.GetTextSpan());
            return result;
        }

        public UstNode VisitFunctionCallName(PhpParser.FunctionCallNameContext context)
        {
            Expression result;
            if (context.qualifiedNamespaceName() != null) // TODO: Fix QualifiedNamespaceName Type.
            {
                var strLit = (StringLiteral)Visit(context.qualifiedNamespaceName());
                result = new IdToken(strLit.Text, strLit.TextSpan);
            }
            else
            {
                result = (Expression)Visit(context.GetChild(0));
            }
            return result;
        }

        public UstNode VisitActualArguments(PhpParser.ActualArgumentsContext context)
        {
            TypeToken genericArgs;
            if (context.genericDynamicArgs() != null)
            {
                genericArgs = (TypeToken)Visit(context.genericDynamicArgs());
            }
            var arguments = (ArgsNode)Visit(context.arguments());
            var exprs = ConvertSquareCurlyExpressions(context.squareCurlyExpression());
            exprs.InsertRange(0, arguments.Collection);

            var result = new ArgsNode(exprs, context.GetTextSpan());
            return result;
        }

        public UstNode VisitChainBase(PhpParser.ChainBaseContext context)
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
                    var idToken = target as IdToken;
                    if (idToken != null && idToken.TextValue == "this")
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

        public UstNode VisitKeyedFieldName(PhpParser.KeyedFieldNameContext context)
        {
            return (IdToken)Visit(context.GetChild(0));
        }

        public UstNode VisitKeyedSimpleFieldName(PhpParser.KeyedSimpleFieldNameContext context)
        {
            List<Expression> exprs = ConvertSquareCurlyExpressions(context.squareCurlyExpression());

            IdToken result;
            if (context.identifier() != null)
            {
                result = (IdToken)Visit(context.identifier());
            }
            else
            {
                result = new IdToken(Helper.Prefix + "expressionId", context.GetTextSpan());
                exprs.Insert(0, (Expression)Visit(context.expression()));
            }

            result.Expression = new MultichildExpression(exprs, context.GetTextSpan());
            return result;
        }

        public UstNode VisitKeyedVariable(PhpParser.KeyedVariableContext context)
        {
            Expression left;
            if (context.VarName() != null)
            {
                left = (IdToken)ConvertVar(context.VarName());
                left.TextSpan = context.GetTextSpan();
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
                var args = new ArgsNode(exprs);
                result = new IndexerExpression(left, args, context.GetTextSpan());
            }
            return result;
        }

        public UstNode VisitSquareCurlyExpression(PhpParser.SquareCurlyExpressionContext context)
        {
            return context.expression() != null ? Visit(context.expression()) : null;
        }

        public UstNode VisitAssignmentList(PhpParser.AssignmentListContext context)
        {
            Expression[] exps = context.assignmentListElement()
                .Select(elem => (Expression)Visit(elem))
                .Where(elem => elem != null)
                .ToArray();
            var result = new ArgsNode(exps, context.GetTextSpan());
            return result;
        }

        public UstNode VisitAssignmentListElement(PhpParser.AssignmentListElementContext context)
        {
            if (context.chain() != null)
            {
                return (Expression)Visit(context.chain());
            }
            else
            {
                var target = new IdToken(context.List().GetText(), context.List().GetTextSpan());
                var args = (ArgsNode)Visit(context.assignmentList());
                var result = new InvocationExpression(target, args, context.GetTextSpan());
                return result;
            }
        }

        public UstNode VisitModifier(PhpParser.ModifierContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitIdentifier(PhpParser.IdentifierContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan());
            return result;
        }

        public UstNode VisitMemberModifier(PhpParser.MemberModifierContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitMagicConstant(PhpParser.MagicConstantContext context)
        {
            return new IdToken(context.GetText(), context.GetTextSpan());
        }

        public UstNode VisitMagicMethod(PhpParser.MagicMethodContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan());
            return result;
        }
        
        public UstNode VisitPrimitiveType(PhpParser.PrimitiveTypeContext context)
        {
            var result = new TypeToken(context.GetText(), context.GetTextSpan());
            return result;
        }
        
        public UstNode VisitCastOperation(PhpParser.CastOperationContext context)
        {
            var result = new TypeToken(context.GetText(), context.GetTextSpan());
            return result;
        }

        public override UstNode VisitTerminal(ITerminalNode node)
        {
            var nodeText = node.GetText();
            var result = new StringLiteral(nodeText, node.GetTextSpan());
            return result;
        }
    }
}
