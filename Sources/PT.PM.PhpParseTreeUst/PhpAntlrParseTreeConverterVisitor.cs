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

namespace PT.PM.PhpParseTreeUst
{
    public partial class PhpAntlrParseTreeConverterVisitor : AntlrDefaultVisitor, IPHPParserVisitor<UstNode>
    {
        protected const string namespacePrefix = Helper.Prefix + "ns";
        protected const string elementNamespacePrefix = Helper.Prefix + "elemNs";
        protected const string contentNamespacePrefix = Helper.Prefix + "contentNs";
        protected const string attrNamespacePrefix = Helper.Prefix + "attrNs";
        protected const string inlineHtmlNamespacePrefix = Helper.Prefix + "inlineHtml";

        protected int namespaceDepth;

        public LanguageFlags ConvertedLanguages { get; set; }

        public PhpAntlrParseTreeConverterVisitor(string fileName, string fileData)
            : base(fileName, fileData)
        {
            namespaceDepth = 0;
        }

        public UstNode VisitHtmlDocument(PHPParser.HtmlDocumentContext context)
        {
            UstNode[] phpBlocks = context.htmlElementOrPhpBlock()
                .Select(block => Visit(block))
                .Where(phpBlock => phpBlock != null)
                .ToArray();

            UstNode root = phpBlocks.CreateRootNamespace(Language.Php, FileNode);
            FileNode.Root = root.RemoveNotIncludedNodes(Language.Php, ConvertedLanguages);

            return FileNode;
        }

        public UstNode VisitHtmlElementOrPhpBlock(PHPParser.HtmlElementOrPhpBlockContext context)
        {
            UstNode result = null;
            if (context.htmlElements() != null)
            {
                var stringLiteral = (StringLiteral)Visit(context.htmlElements());
                result = NodeHelpers.CreateLanguageNamespace(stringLiteral, Language.Html, FileNode);
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

        public UstNode VisitHtmlElements(PHPParser.HtmlElementsContext context)
        {
            return new StringLiteral(context.GetText(), context.GetTextSpan(), FileNode);
        }

        public UstNode VisitHtmlElement(PHPParser.HtmlElementContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitScriptTextPart(PHPParser.ScriptTextPartContext context)
        {
            string javaScriptCode = string.Join("", context.ScriptText().Select(text => text.GetText()));
            UstNode result;
            if (ConvertedLanguages.Is(Language.JavaScript))
            {
                var javaScriptParser = new JavaScriptAntlrParser();
                javaScriptParser.Logger = Logger;
                var sourceCodeFile = new SourceCodeFile()
                {
                    Name = FileNode.FileName.Text,
                    Code = javaScriptCode
                };
                var parseTree = (JavaScriptAntlrParseTree)javaScriptParser.Parse(sourceCodeFile);

                var javaScriptConverter = new JavaScriptAntlrUstConverterVisitor(FileNode.FileName.Text, FileNode.FileData);
                javaScriptConverter.Logger = Logger;
                result = javaScriptConverter.Visit(parseTree.SyntaxTree);
                var resultFileNode = result as FileNode;
                if (resultFileNode != null)
                {
                    result = resultFileNode.Root.CreateLanguageNamespace(Language.JavaScript, FileNode);
                }
                TextSpan contextTextSpan = context.GetTextSpan();
                result.ApplyActionToDescendants(ustNode => ustNode.TextSpan = ustNode.TextSpan.AddOffset(contextTextSpan.Start));
            }
            else
            {
                result = new StringLiteral(javaScriptCode, context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitPhpBlock(PHPParser.PhpBlockContext context)
        {
            var namespaceName = new StringLiteral(Helper.Prefix + "default", context.GetTextSpan(), FileNode);
            UsingDeclaration[] usingDeclarations = context.importStatement()
                .Select(importStatement => (UsingDeclaration)Visit(importStatement))
                .Where(stmt => stmt != null)
                .ToArray();

            Statement[] topStatements = context.topStatement()
                .Select(topStatement => (Statement)Visit(topStatement))
                .Where(stmt => stmt != null)
                .ToArray();
            var statementsNode = new BlockStatement(topStatements,
                topStatements.First().TextSpan.Union(topStatements.Last().TextSpan), FileNode);
            var members = new List<UstNode>();
            members.AddRange(usingDeclarations);
            members.Add(statementsNode);

            var result = new NamespaceDeclaration(namespaceName, members, Language.Php, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitImportStatement(PHPParser.ImportStatementContext context)
        {
            var namespaceName = (StringLiteral)Visit(context.namespaceNameList());

            var result = new UsingDeclaration(namespaceName, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitTopStatement(PHPParser.TopStatementContext context)
        {
            Statement result;
            if (context.classDeclaration() != null)
            {
                var typeDeclaration = (TypeDeclaration)Visit(context.classDeclaration());
                result = new TypeDeclarationStatement(typeDeclaration, typeDeclaration.TextSpan, FileNode);
            }
            else
            {
                UstNode statement = Visit(context.GetChild(0));
                var resultStatmenet = statement as Statement;
                if (resultStatmenet != null)
                {
                    result = resultStatmenet;
                }
                else
                {
                    result = new WrapperStatement(statement, statement.TextSpan, FileNode);
                }
            }
            return result;
        }

        public UstNode VisitUseDeclaration(PHPParser.UseDeclarationContext context)
        {
            var result = new UsingDeclaration(
                new StringLiteral(context.useDeclarationContentList().GetText(), context.useDeclarationContentList().GetTextSpan(), FileNode),
                context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitUseDeclarationContentList(PHPParser.UseDeclarationContentListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitUseDeclarationContent(PHPParser.UseDeclarationContentContext context)
        {
            var namespaceNameListUstNode = (StringLiteral)Visit(context.namespaceNameList());

            var result = new UsingDeclaration(namespaceNameListUstNode, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitNamespaceDeclaration(PHPParser.NamespaceDeclarationContext context)
        {
            StringLiteral name;
            if (context.namespaceNameList() != null)
            {
                name = (StringLiteral)Visit(context.namespaceNameList());
            }
            else
            {
                name = new StringLiteral(Helper.Prefix + "unnamed", default(TextSpan), FileNode);
            }

            UstNode[] members = context.namespaceStatement()
                .Select(statement => Visit(statement))
                .Where(statement => statement != null)
                .ToArray();

            var result = new NamespaceDeclaration(name, members, Language.Php, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitNamespaceStatement(PHPParser.NamespaceStatementContext context)
        {
            return Visit(context.GetChild(0));
        }

        public UstNode VisitFunctionDeclaration(PHPParser.FunctionDeclarationContext context)
        {
            TypeToken returnType = null;
            if (context.typeParameterListInBrackets() != null)
            {
                returnType = (TypeToken)Visit(context.typeParameterListInBrackets());
            }
            var id = (IdToken)Visit(context.identifier());
            var body = (BlockStatement)Visit(context.blockStatement());
            var parameters = ConvertParameters(context.formalParameterList());

            var result = new MethodDeclaration(id, parameters, body, context.GetTextSpan(), FileNode)
            {
                ReturnType = returnType
            };
            return result;
        }

        public UstNode VisitClassDeclaration(PHPParser.ClassDeclarationContext context)
        {
            TypeTypeLiteral typeTypeToken;
            if (context.classEntryType() != null)
            {
                typeTypeToken = new TypeTypeLiteral(TypeType.Class, context.classEntryType().GetTextSpan(), FileNode);
            }
            else
            {
                typeTypeToken = new TypeTypeLiteral(TypeType.Interface, context.Interface().GetTextSpan(), FileNode);
            }

            EntityDeclaration[] members = context.classStatement()
                .Select(statement => (EntityDeclaration)Visit(statement))
                .Where(statement => statement != null)
                .ToArray();

            var id = (IdToken)Visit(context.identifier());
            var result = new TypeDeclaration(typeTypeToken, id, members, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitClassEntryType(PHPParser.ClassEntryTypeContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitInterfaceList(PHPParser.InterfaceListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterListInBrackets(PHPParser.TypeParameterListInBracketsContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterList(PHPParser.TypeParameterListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterWithDefaultsList(PHPParser.TypeParameterWithDefaultsListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterDecl(PHPParser.TypeParameterDeclContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTypeParameterWithDefaultDecl(PHPParser.TypeParameterWithDefaultDeclContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitGenericDynamicArgs(PHPParser.GenericDynamicArgsContext context)
        {
            string[] typeRefs = context.typeRef()
                .Select(type => ((TypeToken)Visit(type))?.TypeText)
                .Where(type => type != null).ToArray();

            var str = context.GetChild(0).GetText() + string.Join(",", typeRefs) + context.GetChild(context.ChildCount - 1).GetText();
            var result = new TypeToken(str, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitAttributes(PHPParser.AttributesContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttributesGroup(PHPParser.AttributesGroupContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttribute(PHPParser.AttributeContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttributeArgList(PHPParser.AttributeArgListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttributeNamedArgList(PHPParser.AttributeNamedArgListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitAttributeNamedArg(PHPParser.AttributeNamedArgContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitInnerStatementList(PHPParser.InnerStatementListContext context)
        {
            List<Statement> innerStatementUstNodes = context.innerStatement()
                .Select(c => (Statement)Visit(c))
                .Where(c => c != null)
                .ToList();

            var result = new BlockStatement(innerStatementUstNodes, 
                context.innerStatement().Length > 0 ? context.GetTextSpan() : default(TextSpan), FileNode);
            return result;
        }

        public UstNode VisitInnerStatement(PHPParser.InnerStatementContext context)
        {
            Statement result;
            if (context.statement() != null)
            {
                result = (Statement)Visit(context.statement());
            }
            else if (context.functionDeclaration() != null)
            {
                var funcDeclaraion = (MethodDeclaration)Visit(context.functionDeclaration());
                result = new WrapperStatement(funcDeclaraion, funcDeclaraion.TextSpan, FileNode);
            }
            else
            {
                var typeDeclaration = (TypeDeclaration)Visit(context.classDeclaration());
                result = new TypeDeclarationStatement(typeDeclaration, typeDeclaration.TextSpan, FileNode);
            }
            return result;
        }

        public UstNode VisitStatement(PHPParser.StatementContext context)
        {
            Statement result;
            UstNode visitResult = Visit(context.GetChild(0));
            Expression expr = visitResult as Expression;
            if (expr != null)
            {
                result = new ExpressionStatement(expr);
            }
            else
            {
                result = (Statement)visitResult;
            }
            return result;
        }

        public UstNode VisitEmptyStatement(PHPParser.EmptyStatementContext context)
        {
            return new EmptyStatement(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitNonEmptyStatement(PHPParser.NonEmptyStatementContext context)
        {
            if (context.identifier() != null)
            {
                return new EmptyStatement(context.GetTextSpan(), FileNode);
            }

            if (context.yieldExpression() != null)
            {
                var result = new ExpressionStatement((Expression)Visit(context.yieldExpression()),
                    context.GetTextSpan(), FileNode);
                return result;
            }

            return Visit(context.GetChild(0));
        }

        public UstNode VisitBlockStatement(PHPParser.BlockStatementContext context)
        {
            var innerStatementListUstNode = (BlockStatement)Visit(context.innerStatementList());

            var result = new BlockStatement(innerStatementListUstNode.Statements, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitIfStatement(PHPParser.IfStatementContext context)
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

            var result = new IfElseStatement(condition, trueStatement, context.GetTextSpan(), FileNode);
            IfElseStatement s = result;
            foreach (var elseIfStatement in ifElseStatements)
            {
                s.FalseStatement = elseIfStatement;
                s = elseIfStatement;
            }
            s.FalseStatement = elseStatement;
            return result;
        }

        public UstNode VisitElseIfStatement(PHPParser.ElseIfStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            var statement = (Statement)Visit(context.statement());

            var result = new IfElseStatement(condition, statement, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitElseIfColonStatement(PHPParser.ElseIfColonStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            var statement = (Statement)Visit(context.innerStatementList());

            var result = new IfElseStatement(condition, statement, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitElseStatement(PHPParser.ElseStatementContext context)
        {
            var statement = (Statement)Visit(context.statement());
            return statement;
        }

        public UstNode VisitElseColonStatement(PHPParser.ElseColonStatementContext context)
        {
            var statement = (Statement)Visit(context.innerStatementList());
            return statement;
        }

        public UstNode VisitWhileStatement(PHPParser.WhileStatementContext context)
        {
            var condition = (Expression)Visit(context.parenthesis());
            Statement statement = context.statement() != null
                ? (Statement)Visit(context.statement())
                : (Statement)Visit(context.innerStatementList());

            var result = new WhileStatement(condition, statement, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitDoWhileStatement(PHPParser.DoWhileStatementContext context)
        {
            var statement = (Statement)Visit(context.statement());
            var condition = (Expression)Visit(context.parenthesis());

            var result = new DoWhileStatement(statement, condition, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitForStatement(PHPParser.ForStatementContext context)
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

            var result = new ForStatement(initializer, condition, iterators, statement, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitForInit(PHPParser.ForInitContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitForUpdate(PHPParser.ForUpdateContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitSwitchStatement(PHPParser.SwitchStatementContext context)
        {
            var expression = (Expression)Visit(context.parenthesis());
            SwitchSection[] switchBlocks = context.switchBlock()
                .Select(block => (SwitchSection)Visit(block))
                .Where(block => block != null)
                .ToArray();

            var result = new SwitchStatement(expression, switchBlocks, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitSwitchBlock(PHPParser.SwitchBlockContext context)
        {
            Expression[] caseLabels = context.expression().Select(e => (Expression)Visit(e))
                .Where(e => e != null).ToArray();
            var innerStatement = (BlockStatement)Visit(context.innerStatementList());
            var statements = innerStatement.Statements.ToList();
            if (context.Default().Length > 0)
                statements.Add(null);

            var result = new SwitchSection(caseLabels, statements, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitBreakStatement(PHPParser.BreakStatementContext context)
        {
            var result = new BreakStatement(context.GetTextSpan(), FileNode)
            {
                Expression = context.expression() == null ? null : (Expression)Visit(context.expression())
            };
            return result;
        }

        public UstNode VisitContinueStatement(PHPParser.ContinueStatementContext context)
        {
            var result = new ContinueStatement(context.GetTextSpan(), FileNode)
            {
                Expression = context.expression() == null ? null : (Expression)Visit(context.expression())
            };
            return result;
        }

        public UstNode VisitReturnStatement(PHPParser.ReturnStatementContext context)
        {
            var returnExpression = context.expression() == null ? null : (Expression)Visit(context.expression());
            var result = new ReturnStatement(returnExpression, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitExpressionStatement(PHPParser.ExpressionStatementContext context)
        {
            var expression = (Expression)Visit(context.expression());
            var result = new ExpressionStatement(expression, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitUnsetStatement(PHPParser.UnsetStatementContext context)
        {
            var args = (ArgsNode)Visit(context.chainList());
            var invocation = new InvocationExpression(
                new IdToken(context.Unset().GetText(), context.Unset().GetTextSpan(), FileNode),
                args, context.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(invocation, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitForeachStatement(PHPParser.ForeachStatementContext context)
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

            var inExpression = new MultichildExpression(expressions, context.GetTextSpan(), FileNode); // TODO: Spans union

            Statement statement = context.statement() != null
                ? (Statement)Visit(context.statement())
                : (Statement)Visit(context.innerStatementList());

            var result = new ForeachStatement(null, null, inExpression, statement, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitTryCatchFinally(PHPParser.TryCatchFinallyContext context)
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

            var result = new TryCatchStatement(statement, context.GetTextSpan(), FileNode)
            {
                CatchClauses = catchClauses,
                FinallyBlock = finallyBlock
            };
            return result;
        }

        public UstNode VisitCatchClause(PHPParser.CatchClauseContext context)
        {
            var type = (TypeToken)Visit(context.qualifiedStaticTypeRef());
            var varName = (IdToken)ConvertVar(context.VarName());
            var body = (BlockStatement)Visit(context.blockStatement());

            var result = new CatchClause(type, varName, body, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitFinallyStatement(PHPParser.FinallyStatementContext context)
        {
            var result = (BlockStatement)Visit(context.blockStatement());
            return result;
        }

        public UstNode VisitThrowStatement(PHPParser.ThrowStatementContext context)
        {
            var expression = (Expression)Visit(context.expression());
            var result = new ThrowStatement(expression, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitGotoStatement(PHPParser.GotoStatementContext context)
        {
            return new EmptyStatement(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitDeclareStatement(PHPParser.DeclareStatementContext context)
        {
            AssignmentExpression[] variables = context.declareList().identifierInititalizer().Select(
                id => (AssignmentExpression)Visit(id))
                .Where(id => id != null).ToArray();

            var decl = new FieldDeclaration(variables, context.declareList().GetTextSpan(), FileNode);

            Statement statement;
            if (context.statement() != null)
            {
                statement = (Statement)Visit(context.statement());
            }
            else
            {
                statement = (Statement)Visit(context.innerStatementList());
            }

            var result = new WithStatement(decl, statement, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitDeclareList(PHPParser.DeclareListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitInlineHtml(PHPParser.InlineHtmlContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFormalParameterList(PHPParser.FormalParameterListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitFormalParameter(PHPParser.FormalParameterContext context)
        {
            TypeToken type = null;
            if (context.typeHint() != null)
                type = (TypeToken)Visit(context.typeHint());

            var varInit = (AssignmentExpression)Visit(context.variableInitializer());

            var result = new ParameterDeclaration(type, (IdToken)varInit.Left, context.GetTextSpan(), FileNode);
            result.Initializer = varInit.Right;
            return result;
        }

        public UstNode VisitTypeHint(PHPParser.TypeHintContext context)
        {
            if (context.Callable() != null)
                return new TypeToken(context.GetText(), context.GetTextSpan(), FileNode);

            return (TypeToken)Visit(context.GetChild(0));
        }

        public UstNode VisitGlobalStatement(PHPParser.GlobalStatementContext context)
        {
            Expression[] globalVars = context.globalVar()
                .Select(globalVar => (Expression)Visit(globalVar))
                .Where(v => v != null).ToArray();
            var multichild = new MultichildExpression(globalVars, context.GetTextSpan(), FileNode);

            var result = new ExpressionStatement(multichild, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitGlobalVar(PHPParser.GlobalVarContext context)
        {
            if (context.VarName() != null)
                return (IdToken)ConvertVar(context.VarName());

            if (context.chain() != null)
                return (Expression)Visit(context.chain());

            return (Expression)Visit(context.expression());
        }

        public UstNode VisitEchoStatement(PHPParser.EchoStatementContext context)
        {
            var name = new IdToken(context.Echo().GetText(), context.GetTextSpan(), FileNode);
            var args = new ArgsNode(((MultichildExpression)Visit(context.expressionList())).Expressions.ToList(),
                context.expressionList().GetTextSpan(), FileNode);
            var invocation = new InvocationExpression(name, args, context.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(invocation, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitStaticVariableStatement(PHPParser.StaticVariableStatementContext context)
        {
            AssignmentExpression[] variables = context.variableInitializer()
                .Select(varInit => (AssignmentExpression)Visit(varInit))
                .Where(v => v != null).ToArray();
            var type = new TypeToken(context.Static().GetText(), context.Static().GetTextSpan(), FileNode);

            var result = new VariableDeclarationExpression(type, variables, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitClassStatement(PHPParser.ClassStatementContext context)
        {
            EntityDeclaration result = null;

            if (context.variableInitializer().Length > 0)
            {
                AssignmentExpression[] properties = context.variableInitializer().Select(decl =>
                    (AssignmentExpression)Visit(decl))
                    .Where(decl => decl != null).ToArray();

                result = new FieldDeclaration(properties, context.GetTextSpan(), FileNode);
            }
            else if (context.identifierInititalizer().Length > 0)
            {
                AssignmentExpression[] variables = context.identifierInititalizer().Select(decl =>
                    (AssignmentExpression)Visit(decl))
                    .Where(decl => decl != null).ToArray();

                result = new FieldDeclaration(variables, context.GetTextSpan(), FileNode);
            }
            else if (context.Function() != null)
            {
                var id = (IdToken)VisitIdentifier(context.identifier());
                var parameters = (ParameterDeclaration[])ConvertParameters(context.formalParameterList());
                var block = (BlockStatement)Visit(context.methodBody());

                result = new MethodDeclaration(id, parameters, block, context.GetTextSpan(), FileNode);
            }
            else if (context.Use() != null)
            {
                // TODO: UsingDeclaraion (EntityWrapper).
                result = null;
            }

            return result;
        }

        public UstNode VisitTraitAdaptations(PHPParser.TraitAdaptationsContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTraitAdaptationStatement(PHPParser.TraitAdaptationStatementContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTraitPrecedence(PHPParser.TraitPrecedenceContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTraitAlias(PHPParser.TraitAliasContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitTraitMethodReference(PHPParser.TraitMethodReferenceContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitBaseCtorCall(PHPParser.BaseCtorCallContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitMethodBody(PHPParser.MethodBodyContext context)
        {
            if (context.blockStatement() != null)
                return (BlockStatement)Visit(context.blockStatement());

            return null;
        }

        public UstNode VisitPropertyModifiers(PHPParser.PropertyModifiersContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitMemberModifiers(PHPParser.MemberModifiersContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitVariableInitializer(PHPParser.VariableInitializerContext context)
        {
            IdToken name = (IdToken)ConvertVar(context.VarName());
            Expression initializer = null;
            if (context.constantInititalizer() != null)
            {
                initializer = (Expression)Visit(context.constantInititalizer());
            }

            var result = new AssignmentExpression(name, initializer, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitIdentifierInititalizer(PHPParser.IdentifierInititalizerContext context)
        {
            var id = (IdToken)Visit(context.identifier());
            var initializer = (Expression)Visit(context.constantInititalizer());

            var result = new AssignmentExpression(id, initializer, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitGlobalConstantDeclaration(PHPParser.GlobalConstantDeclarationContext context)
        {
            AssignmentExpression[] identifiers = context.identifierInititalizer()
                .Select(id => (AssignmentExpression)Visit(id))
                .Where(id => id != null).ToArray();

            var result = new FieldDeclaration(identifiers, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitExpressionList(PHPParser.ExpressionListContext context)
        {
            Expression[] expressions = context.expression().Select(c => (Expression)Visit(c))
                .Where(c => c != null).ToArray();

            return new MultichildExpression(expressions, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitParenthesis(PHPParser.ParenthesisContext context)
        {
            if (context.expression() != null)
                return Visit(context.expression());

            return Visit(context.yieldExpression());
        }

        public UstNode VisitNewExpr(PHPParser.NewExprContext context)
        {
            var type = (TypeToken)Visit(context.typeRef());
            ArgsNode args = context.arguments() != null
                ? (ArgsNode)Visit(context.arguments())
                : new ArgsNode();

            var result = new ObjectCreateExpression(type, args, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitExpression(PHPParser.ExpressionContext context)
        {
            Expression result;
            if (context.ChildCount == 1)
            {
                result = (Expression)Visit(context.children[0]);
            }
            else if (context.QuestionMark() != null)
            {
                var expression0 = (Expression)Visit(context.expression(0));
                var expression1 = (Expression)(context.expression().Length == 3 ? Visit(context.expression(1)) : null);
                var expression2 = (Expression)Visit(context.andOrExpression());
                result = new ConditionalExpression(expression0, expression1, expression2,
                    context.GetTextSpan(), FileNode);
            }
            else
            {
                result = CreateBinaryOperatorExpression(context.expression(0), context.GetChild<ITerminalNode>(0),
                    context.andOrExpression());
            }
            return result;
        }

        public UstNode VisitAndOrExpression([NotNull] PHPParser.AndOrExpressionContext context)
        {
            Expression result;
            if (context.ChildCount == 1)
            {
                result = (Expression)Visit(context.children[0]);
            }
            else
            {
                result = CreateBinaryOperatorExpression(context.andOrExpression(), context.GetChild<ITerminalNode>(0),
                    context.comparisonExpression());
            }
            return result;
        }

        public UstNode VisitComparisonExpression([NotNull] PHPParser.ComparisonExpressionContext context)
        {
            Expression result;
            if (context.ChildCount == 1)
            {
                result = (Expression)Visit(context.children[0]);
            }
            else
            {
                result = CreateBinaryOperatorExpression(context.comparisonExpression(), context.GetChild<ITerminalNode>(0),
                    context.additionExpression());
            }
            return result;
        }

        public UstNode VisitAdditionExpression([NotNull] PHPParser.AdditionExpressionContext context)
        {
            Expression result;
            if (context.ChildCount == 1)
            {
                result = (Expression)Visit(context.children[0]);
            }
            else
            {
                result = CreateBinaryOperatorExpression(context.additionExpression(), context.GetChild<ITerminalNode>(0),
                    context.multiplicationExpression());
            }
            return result;
        }

        public UstNode VisitMultiplicationExpression([NotNull] PHPParser.MultiplicationExpressionContext context)
        {
            Expression result;
            if (context.ChildCount == 1)
            {
                result = (Expression)Visit(context.children[0]);
            }
            else if (context.InstanceOf() != null)
            {
                result = (Expression)Visit(context.multiplicationExpression()); // TODO: InstanceOf
            }
            else
            {
                var terminal = context.GetChild<ITerminalNode>(0);
                ParserRuleContext left, right;
                if (terminal.GetText() == "**")
                {
                    left = context.notLeftRecursionExpression();
                    right = context.multiplicationExpression();
                }
                else
                {
                    left = context.multiplicationExpression();
                    right = context.notLeftRecursionExpression();
                }
                result = CreateBinaryOperatorExpression(left, terminal, right);
            }
            return result;
        }

        public UstNode VisitNotLeftRecursionExpression([NotNull] PHPParser.NotLeftRecursionExpressionContext context)
        {
            return (Expression)Visit(context);
        }

        public UstNode VisitCloneExpression(PHPParser.CloneExpressionContext context)
        {
            return CreateSpecialInvocation(context.Clone(), context.expression(), context.GetTextSpan());
        }

        public UstNode VisitNewExpression(PHPParser.NewExpressionContext context)
        {
            return (Expression)Visit(context.newExpr());
        }

        public UstNode VisitIndexerExpression(PHPParser.IndexerExpressionContext context)
        {
            var target = (Expression)Visit(context.stringConstant());
            var arg = (Expression)Visit(context.expression());

            var result = new IndexerExpression(target, new ArgsNode(new[] { arg }), context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitPrefixIncDecExpression(PHPParser.PrefixIncDecExpressionContext context)
        {
            ITerminalNode operatorTerminal = context.GetChild<ITerminalNode>(0);
            string unaryOperatorText = operatorTerminal.GetText();
            UnaryOperator unaryOperator = UnaryOperatorLiteral.PrefixTextUnaryOperator[unaryOperatorText];
            var expression = (Expression)Visit(context.chain());
            var result = new UnaryOperatorExpression(
                new UnaryOperatorLiteral(unaryOperator, operatorTerminal.GetTextSpan(), FileNode),
                expression,
                context.GetTextSpan(), FileNode);

            return result;
        }

        public UstNode VisitPostfixIncDecExpression(PHPParser.PostfixIncDecExpressionContext context)
        {
            ITerminalNode operatorTerminal = context.GetChild<ITerminalNode>(0);
            string unaryOperatorText = operatorTerminal.GetText();
            UnaryOperator unaryOperator = UnaryOperatorLiteral.PostfixTextUnaryOperator[unaryOperatorText];
            var expression = (Expression)Visit(context.chain());
            var result = new UnaryOperatorExpression(
                new UnaryOperatorLiteral(unaryOperator, operatorTerminal.GetTextSpan(), FileNode),
                expression,
                context.GetTextSpan(), FileNode);

            return result;
        }
        
        public UstNode VisitCastExpression(PHPParser.CastExpressionContext context)
        {
            var castType = (TypeToken)Visit(context.castOperation());
            var expression = (Expression)Visit(context.expression());

            var result = new CastExpression(castType, expression, context.GetTextSpan(), FileNode);
            return result;
        }
        
        public UstNode VisitUnaryOperatorExpression(PHPParser.UnaryOperatorExpressionContext context)
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
                new UnaryOperatorLiteral(unaryOperator, operatorTerminal.GetTextSpan(), FileNode),
                expression0,
                context.GetTextSpan(), FileNode);

            return result;
        }
        
        public UstNode VisitAssignmentExpression(PHPParser.AssignmentExpressionContext context)
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
                    result = new AssignmentExpression(left, right, context.GetTextSpan(), FileNode);
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

                    result = ConverterHelper.ConvertToAssignmentExpression(left, binaryOperator, operatorTerminal.GetTextSpan(),
                        right, context.GetTextSpan(), FileNode);
                }
            }
            else
            {
                result = new AssignmentExpression(left,
                    (Expression)Visit(context.GetChild(3)), context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitAssignmentOperator(PHPParser.AssignmentOperatorContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitPrintExpression(PHPParser.PrintExpressionContext context)
        {
            return (Expression)CreateSpecialInvocation(context.Print(), context.expression(), context.GetTextSpan());
        }
        
        public UstNode VisitChainExpression(PHPParser.ChainExpressionContext context)
        {
            return (Expression)Visit(context.GetChild(0));
        }
        
        public UstNode VisitScalarExpression(PHPParser.ScalarExpressionContext context)
        {
            if (context.Label() != null)
            {
                return new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
            }

            return (Expression)Visit(context.GetChild(0));
        }
        
        public UstNode VisitBackQuoteStringExpression(PHPParser.BackQuoteStringExpressionContext context)
        {
            return Visit(context.BackQuoteString());
        }
        
        public UstNode VisitParenthesisExpression(PHPParser.ParenthesisExpressionContext context)
        {
            return (Expression)Visit(context.parenthesis());
        }

        public UstNode VisitArrayCreationExpression(PHPParser.ArrayCreationExpressionContext context)
        {
            List<Expression> inits = context.arrayItemList().arrayItem()
                .Select(item => (Expression)Visit(item))
                .Where(item => item != null).ToList();
            if (context.expression() != null)
                inits.Add((Expression)Visit(context.expression()));

            var result = new ArrayCreationExpression(null, Enumerable.Empty<Expression>(), inits,
                context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitSpecialWordExpression(PHPParser.SpecialWordExpressionContext context)
        {
            TextSpan textSpan = context.GetTextSpan();
            Expression expression, result;

            if (context.Yield() != null)
            {
                result = new InvocationExpression(
                    new IdToken(context.Yield().GetText(), context.Yield().GetTextSpan(), FileNode),
                    new ArgsNode(), context.GetTextSpan(), FileNode);
            }
            else if (context.List() != null)
            {
                var args = (ArgsNode)Visit(context.assignmentList());
                expression = (Expression)Visit(context.expression());

                TextSpan listTextSpan = context.List().GetTextSpan();
                var invoke = new InvocationExpression(new IdToken(context.List().GetText(), listTextSpan, FileNode),
                    args, listTextSpan.Union(context.GetChild<ITerminalNode>(2).GetTextSpan()), FileNode);
                result = new AssignmentExpression(invoke, expression, textSpan, FileNode);
            }
            else if (context.IsSet() != null)
            {
                var args = (ArgsNode)Visit(context.chainList());
                result = new InvocationExpression(
                    new IdToken(context.IsSet().GetText(), context.IsSet().GetTextSpan(), FileNode),
                    args, textSpan, FileNode);
            }
            else if (context.Empty() != null)
            {
                var args = new ArgsNode(new[] { (Expression)Visit(context.chain()) });
                result = new InvocationExpression(
                    new IdToken(context.Empty().GetText(), context.Empty().GetTextSpan(), FileNode),
                    args, textSpan, FileNode);
                return result;
            }
            else if (context.Exit() != null)
            {
                var exprs = new List<Expression>();
                if (context.parenthesis() != null)
                    exprs.Add((Expression)Visit(context.parenthesis()));
                result = new InvocationExpression(
                    new IdToken(context.Exit().GetText(), context.Exit().GetTextSpan(), FileNode),
                    new ArgsNode(exprs), textSpan, FileNode);
            }
            else
            {
                expression = (Expression)Visit(context.expression());
                result = new InvocationExpression(
                    new IdToken(context.GetChild(0).GetText()),
                    new ArgsNode(new Expression[] { expression }), textSpan, FileNode);
            }
            return result;
        }

        public UstNode VisitLambdaFunctionExpression(PHPParser.LambdaFunctionExpressionContext context)
        {
            ParameterDeclaration[] parameters = ConvertParameters(context.formalParameterList());
            var body = (BlockStatement)Visit(context.blockStatement());

            var result = new AnonymousMethodExpression(parameters, body, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitYieldExpression(PHPParser.YieldExpressionContext context)
        {
            var expressions = context.expression()
                .Select(expression => (Expression)Visit(expression))
                .Where(e => e != null)
                .ToArray();

            var result = new MultichildExpression(expressions, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitArrayItemList(PHPParser.ArrayItemListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitArrayItem(PHPParser.ArrayItemContext context)
        {
            Expression result;
            TextSpan textSpan = context.GetTextSpan();
            if (context.chain() != null)
            {
                Expression chainExpression = (Expression)Visit(context.chain());
                if (context.expression().Length == 1)
                {
                    Expression expression = (Expression)Visit(context.expression(0));
                    result = new AssignmentExpression(expression, chainExpression, textSpan, FileNode);
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
                    result = new AssignmentExpression(expression0, expression1, textSpan, FileNode);
                }
                else
                {
                    result = (Expression)Visit(context.expression(0));
                    result.TextSpan = textSpan;
                }
            }
            return result;
        }

        public UstNode VisitLambdaFunctionUseVars(PHPParser.LambdaFunctionUseVarsContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitLambdaFunctionUseVar(PHPParser.LambdaFunctionUseVarContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitQualifiedStaticTypeRef(PHPParser.QualifiedStaticTypeRefContext context)
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
                return new TypeToken(typeStr.Text + genericStr, textSpan, FileNode);
            }

            var result = new TypeToken(context.Static().GetText(), textSpan, FileNode);
            return result;
        }

        public UstNode VisitTypeRef(PHPParser.TypeRefContext context)
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
                result = new TypeToken(str.Text + genericStr, textSpan, FileNode);
            }
            else if (context.indirectTypeRef() != null)
            {
                var indirectTypeRef = (Expression)Visit(context.indirectTypeRef());
                result = new TypeToken(indirectTypeRef.ToString() + genericStr, textSpan, FileNode);
                result.Expression = indirectTypeRef;
            }
            else if (context.primitiveType() != null)
            {
                result = (TypeToken)Visit(context.primitiveType());
            }
            else
            {
                result = new TypeToken(context.Static().GetText(), textSpan, FileNode);
            }
            return result;
        }

        public UstNode VisitIndirectTypeRef(PHPParser.IndirectTypeRefContext context)
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
                    memberRefExpr.FileNode = FileNode;
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

        public UstNode VisitQualifiedNamespaceName(PHPParser.QualifiedNamespaceNameContext context)
        {
            var result = (StringLiteral)Visit(context.namespaceNameList());
            return result;
        }

        public UstNode VisitNamespaceNameList(PHPParser.NamespaceNameListContext context)
        {
            var result = new StringLiteral(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitQualifiedNamespaceNameList(PHPParser.QualifiedNamespaceNameListContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitArguments(PHPParser.ArgumentsContext context)
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
            var result = new ArgsNode(args, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitActualArgument(PHPParser.ActualArgumentContext context)
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

        public UstNode VisitConstantInititalizer(PHPParser.ConstantInititalizerContext context)
        {
            if (context.constantArrayItemList() != null)
            {
                List<Expression> inits = context.constantArrayItemList().constantArrayItem()
                    .Select(item => (Expression)Visit(item))
                    .Where(item => item != null).ToList();
                var result = new ArrayCreationExpression(null, Enumerable.Empty<Expression>(), inits,
                    context.GetTextSpan(), FileNode);
                return result;
            }

            if (context.constantInititalizer() != null)
            {
                ITerminalNode opNode = context.GetChild<ITerminalNode>(0);
                var opLiteralType = UnaryOperatorLiteral.PrefixTextUnaryOperator[opNode.GetText()];
                var opLiteral = new UnaryOperatorLiteral(opLiteralType, opNode.GetTextSpan(), FileNode);
                var expression = (Expression)Visit(context.constantInititalizer());
                var result = new UnaryOperatorExpression(opLiteral, expression, context.GetTextSpan(), FileNode);
                return result;
            }

            return Visit(context.GetChild(0));
        }
        
        public UstNode VisitConstantArrayItemList(PHPParser.ConstantArrayItemListContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitConstantArrayItem(PHPParser.ConstantArrayItemContext context)
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
                    context.GetTextSpan(), FileNode);
                return result;
            }
        }

        public UstNode VisitConstant(PHPParser.ConstantContext context)
        {
            if (context.Null() != null)
                return new NullLiteral(context.GetTextSpan(), FileNode);

            var result = Visit(context.GetChild(0));
            return result;
        }

        public UstNode VisitLiteralConstant(PHPParser.LiteralConstantContext context)
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
                result = new FloatLiteral(value, contextSpan, FileNode);
            }
            else if (context.BooleanConstant() != null)
            {
                bool value = bool.Parse(contextText);
                result = new BooleanLiteral(value, contextSpan, FileNode);
            }
            else
            {
                result = (Token)Visit(context.GetChild(0));
            }

            return result;
        }

        public UstNode VisitNumericConstant([NotNull] PHPParser.NumericConstantContext context)
        {
            string text = context.GetText();
            Token result;
            try
            {
                long resultValue = 0;
                if (context.Octal() != null)
                {
                    resultValue = Convert.ToInt64(text, 8);
                }
                else if (context.Decimal() != null)
                {
                    resultValue = long.Parse(text);
                }
                else if (context.Hex() != null)
                {
                    resultValue = Convert.ToInt64(text, 16);
                }
                else if (context.Binary() != null)
                {
                    resultValue = Convert.ToInt64(text.Substring(2), 2);
                }
                result = new IntLiteral(resultValue, context.GetTextSpan(), FileNode);
            }
            catch
            {
                result = new FloatLiteral(double.PositiveInfinity, context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitStringConstant(PHPParser.StringConstantContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitString(PHPParser.StringContext context)
        {
            Expression result;
            if (context.StartHereDoc() != null || context.StartNowDoc() != null)
            {
                IEnumerable<string> hereDocText = context.HereDocText().Select(c => c.GetText());
                var str = string.Join("", hereDocText).Trim();
                result = new StringLiteral(str, context.GetTextSpan(), FileNode);
            }
            else if (context.SingleQuoteString() != null)
            {
                var text = context.GetText();
                result = new StringLiteral(text.Substring(1, text.Length - 2), context.GetTextSpan(), FileNode);
            }
            else
            {
                if (context.interpolatedStringPart().Length == 0)
                {
                    result = new StringLiteral("", context.GetTextSpan(), FileNode);
                }
                else
                {
                    result = (Expression)Visit(context.interpolatedStringPart(0));
                    for (int i = 1; i < context.interpolatedStringPart().Length; i++)
                    {
                        var right = (Expression)Visit(context.interpolatedStringPart(i));
                        result = new BinaryOperatorExpression(result,
                            new BinaryOperatorLiteral(BinaryOperator.Plus, default(TextSpan), FileNode),
                            right, result.TextSpan.Union(right.TextSpan), FileNode);
                    }
                }
            }
            return result;
        }

        public UstNode VisitInterpolatedStringPart([NotNull] PHPParser.InterpolatedStringPartContext context)
        {
            Expression result;
            if (context.StringPart() != null)
            {
                result = new StringLiteral(context.StringPart().GetText(), context.GetTextSpan(), FileNode);
            }
            else
            {
                result = (Expression)Visit(context.chain());
            }
            return result;
        }

        public UstNode VisitClassConstant(PHPParser.ClassConstantContext context)
        {
            var target = (Token)Visit(context.GetChild(0));
            var targetId = new IdToken(target.TextValue, target.TextSpan, FileNode);
            var name = (IdToken)Visit(context.GetChild(2));
            var result = new MemberReferenceExpression(targetId, name, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitChainList(PHPParser.ChainListContext context)
        {
            Expression[] expressions = context.chain()
                .Select(c => (Expression)Visit(c))
                .Where(c => c != null).ToArray();
            var result = new ArgsNode(expressions, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitChain(PHPParser.ChainContext context)
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
                    memberRefExpr.FileNode = FileNode;
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

        public UstNode VisitMemberAccess(PHPParser.MemberAccessContext context)
        {
            MultichildExpression expression = null;
            if (context.actualArguments() != null)
            {
                var args = (ArgsNode)Visit(context.actualArguments());
                expression = new MultichildExpression(args.Collection, args.TextSpan, FileNode);
            }
            var result = (IdToken)Visit(context.keyedFieldName());
            result.Expression = expression;
            return result;
        }

        public UstNode VisitFunctionCall(PHPParser.FunctionCallContext context)
        {
            var target = (Expression)Visit(context.functionCallName());
            var args = (ArgsNode)Visit(context.actualArguments());

            var result = new InvocationExpression(target, args, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitFunctionCallName(PHPParser.FunctionCallNameContext context)
        {
            Expression result;
            if (context.qualifiedNamespaceName() != null) // TODO: Fix QualifiedNamespaceName Type.
            {
                var strLit = (StringLiteral)Visit(context.qualifiedNamespaceName());
                result = new IdToken(strLit.Text, strLit.TextSpan, FileNode);
            }
            else
            {
                result = (Expression)Visit(context.GetChild(0));
            }
            return result;
        }

        public UstNode VisitActualArguments(PHPParser.ActualArgumentsContext context)
        {
            TypeToken genericArgs;
            if (context.genericDynamicArgs() != null)
            {
                genericArgs = (TypeToken)Visit(context.genericDynamicArgs());
            }
            var arguments = (ArgsNode)Visit(context.arguments());
            var exprs = ConvertSquareCurlyExpressions(context.squareCurlyExpression());
            exprs.InsertRange(0, arguments.Collection);

            var result = new ArgsNode(exprs, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitChainBase(PHPParser.ChainBaseContext context)
        {
            Expression result;
            TextSpan textSpan = context.GetTextSpan();
            Expression target;
            if (context.qualifiedStaticTypeRef() != null)
            {
                target = (TypeToken)Visit(context.qualifiedStaticTypeRef());
                var name = (Expression)Visit(context.keyedVariable(0));
                result = new MemberReferenceExpression(target, name, textSpan, FileNode);
            }
            else
            {
                target = (Expression)Visit(context.keyedVariable(0));
                if (context.keyedVariable().Length == 1)
                {
                    var idToken = target as IdToken;
                    if (idToken != null && idToken.TextValue == "this")
                    {
                        return new ThisReferenceToken(textSpan, FileNode);
                    }
                    result = target;
                }
                else
                {
                    var name = (Expression)Visit(context.keyedVariable(1));
                    result = new MemberReferenceExpression(target, name, textSpan, FileNode);
                }
            }
            return result;
        }

        public UstNode VisitKeyedFieldName(PHPParser.KeyedFieldNameContext context)
        {
            return (IdToken)Visit(context.GetChild(0));
        }

        public UstNode VisitKeyedSimpleFieldName(PHPParser.KeyedSimpleFieldNameContext context)
        {
            List<Expression> exprs = ConvertSquareCurlyExpressions(context.squareCurlyExpression());

            IdToken result;
            if (context.identifier() != null)
            {
                result = (IdToken)Visit(context.identifier());
            }
            else
            {
                result = new IdToken(Helper.Prefix + "expressionId", context.GetTextSpan(), FileNode);
                exprs.Insert(0, (Expression)Visit(context.expression()));
            }

            result.Expression = new MultichildExpression(exprs, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitKeyedVariable(PHPParser.KeyedVariableContext context)
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
                result = new IndexerExpression(left, args, context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitSquareCurlyExpression(PHPParser.SquareCurlyExpressionContext context)
        {
            return context.expression() != null ? Visit(context.expression()) : null;
        }

        public UstNode VisitAssignmentList(PHPParser.AssignmentListContext context)
        {
            Expression[] exps = context.assignmentListElement()
                .Select(elem => (Expression)Visit(elem))
                .Where(elem => elem != null)
                .ToArray();
            var result = new ArgsNode(exps, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitAssignmentListElement(PHPParser.AssignmentListElementContext context)
        {
            if (context.chain() != null)
            {
                return (Expression)Visit(context.chain());
            }
            else
            {
                var target = new IdToken(context.List().GetText(), context.List().GetTextSpan(), FileNode);
                var args = (ArgsNode)Visit(context.assignmentList());
                var result = new InvocationExpression(target, args, context.GetTextSpan(), FileNode);
                return result;
            }
        }

        public UstNode VisitModifier(PHPParser.ModifierContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitIdentifier(PHPParser.IdentifierContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitMemberModifier(PHPParser.MemberModifierContext context)
        {
            throw new NotImplementedException();
        }

        public UstNode VisitMagicConstant(PHPParser.MagicConstantContext context)
        {
            return new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
        }

        public UstNode VisitMagicMethod(PHPParser.MagicMethodContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }
        
        public UstNode VisitPrimitiveType(PHPParser.PrimitiveTypeContext context)
        {
            var result = new TypeToken(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }
        
        public UstNode VisitCastOperation(PHPParser.CastOperationContext context)
        {
            var result = new TypeToken(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitTerminal(ITerminalNode node)
        {
            var nodeText = node.GetText();
            var result = new StringLiteral(nodeText, node.GetTextSpan(), FileNode);
            return result;
        }
    }
}
