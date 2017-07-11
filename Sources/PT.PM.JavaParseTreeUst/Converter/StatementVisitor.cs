using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.JavaParseTreeUst.Parser;
using PT.PM.AntlrUtils;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes.Tokens.Literals;
using Antlr4.Runtime.Tree;

namespace PT.PM.JavaParseTreeUst.Converter
{
    public partial class JavaAntlrUstConverterVisitor
    {
        public UstNode VisitBlock(JavaParser.BlockContext context)
        {
            Statement[] statements = context.blockStatement()
                .Select(s => (Statement)Visit(s))
                .Where(s => s != null).ToArray();

            var result = new BlockStatement(statements, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitBlockStatement(JavaParser.BlockStatementContext context)
        {
            Statement result;

            var localVariableDeclaration = context.localVariableDeclaration();
            if (localVariableDeclaration != null)
            {
                result = new ExpressionStatement((Expression)Visit(localVariableDeclaration), context.GetTextSpan(), FileNode);
                return result;
            }

            var statement = context.statement();
            if (statement != null)
            {
                result = (Statement)Visit(statement);
                return result;
            }

            var typeDec = context.typeDeclaration();
            if (typeDec != null)
            {
                var typeDeclaration = (TypeDeclaration)Visit(typeDec);
                result = new TypeDeclarationStatement(typeDeclaration, typeDeclaration.TextSpan, FileNode);
                return result;
            }

            return VisitChildren(context);
        }

        public UstNode VisitLocalVariableDeclaration(JavaParser.LocalVariableDeclarationContext context)
        {
            var type = (TypeToken)Visit(context.typeType());
            AssignmentExpression[] initializers = context.variableDeclarators().variableDeclarator()
                .Select(varDec => (AssignmentExpression)Visit(varDec))
                .Where(initializer => initializer != null).ToArray();

            var result = new VariableDeclarationExpression(type, initializers, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitStatement(JavaParser.StatementContext context)
        {
            var textSpan = context.GetTextSpan();
            Statement result;
            Expression expression;
            Statement statement;
            List<Statement> statements;

            if (context.blockLabel != null)
            {
                result = (Statement)Visit(context.blockLabel);
                return result;
            }

            if (context.statementExpression != null)
            {
                result = Visit(context.statementExpression).ToStatementIfRequired();
                return result;
            }

            if (context.identifierLabel != null)
            {
                var defaultResult = VisitChildren(context);
                return new WrapperStatement(defaultResult, textSpan, FileNode);
            }

            int firstTokenType = context.GetChild<ITerminalNode>(0).Symbol.Type;
            if (firstTokenType == JavaLexer.ASSERT)
            {
                var defaultResult = VisitChildren(context);
                return new WrapperStatement(defaultResult, textSpan, FileNode);
            }

            if (firstTokenType == JavaLexer.IF)
            {
                var condition = (Expression)Visit(context.parExpression());
                var trueStatement = (Statement)Visit(context.statement(0));
                var falseStatment = context.ELSE() == null
                    ? null
                    : (Statement)Visit(context.statement(1));

                result = new IfElseStatement(condition, trueStatement, textSpan, FileNode)
                {
                    FalseStatement = falseStatment
                };
                return result;
            }

            if (firstTokenType == JavaLexer.FOR)
            {
                result = (Statement)Visit(context.forControl());
                return result;
            }

            if (firstTokenType == JavaLexer.WHILE)
            {
                var conditionWhile = (Expression)Visit(context.parExpression());
                statement = (Statement)Visit(context.statement(0));

                result = new WhileStatement(conditionWhile, statement, textSpan, FileNode);
                return result;
            }

            if (firstTokenType == JavaLexer.DO)
            {
                statement = (Statement)Visit(context.statement(0));
                expression = (Expression)Visit(context.parExpression());

                result = new DoWhileStatement(statement, expression, textSpan, FileNode);
                return result;
            }

            if (firstTokenType == JavaLexer.TRY)
            {
                // TODO: implement 'try' resourceSpecification block catchClause* finallyBlock? (C# using)

                var block = (BlockStatement)Visit(context.block());
                JavaParser.ResourceSpecificationContext resSpec = context.resourceSpecification();

                List<CatchClause> catchClauses = context.catchClause() == null ? null
                    : context.catchClause()
                    .Select(cc => (CatchClause)Visit(cc))
                    .Where(cc => cc != null).ToList();

                var finallyBlock = context.finallyBlock() == null ? null
                    : (BlockStatement)Visit(context.finallyBlock());

                if (resSpec == null)
                {
                    result = new TryCatchStatement(block, textSpan, FileNode)
                    {
                        CatchClauses = catchClauses,
                        FinallyBlock = finallyBlock
                    };
                }
                else
                {
                    // C# using conversion to tryCatch
                    statements = new List<Statement>();
                    statements.AddRange(resSpec.resources().resource()
                        .Select(res =>
                        {
                            var e = (VariableDeclarationExpression)Visit(res);
                            return e == null ? null : new ExpressionStatement(e);
                        })
                        .Where(res => res != null));
                    statements.AddRange(block.Statements);
                    var blockStatement = new BlockStatement(statements, context.GetTextSpan(), FileNode);

                    result = new TryCatchStatement(block, textSpan, FileNode)
                    {
                        CatchClauses = catchClauses,
                        FinallyBlock = finallyBlock
                    };
                }
                return result;
            }

            if (firstTokenType == JavaLexer.SWITCH)
            {
                expression = (Expression)Visit(context.parExpression());
                SwitchSection[] switchSections = context.switchBlockStatementGroup()
                    .Select(group => (SwitchSection)Visit(group))
                    .Where(group => group != null).ToArray();

                result = new SwitchStatement(expression, switchSections, textSpan, FileNode);
                return result;
            }

            if (firstTokenType == JavaLexer.SYNCHRONIZED) // synchronized(a) { b; c; } => { a; b; c; }
            {
                var resultStatements = new List<Statement>();
                expression = (Expression)Visit(context.parExpression());
                statements = context.block().blockStatement()
                    .Select(s => (Statement)Visit(s))
                    .Where(s => s != null).ToList();
                resultStatements.Add(new ExpressionStatement(expression, expression.TextSpan, FileNode));
                resultStatements.AddRange(statements);

                result = new BlockStatement(resultStatements, textSpan, FileNode);
                return result;
            }

            if (firstTokenType == JavaLexer.RETURN)
            {
                expression = context.expression(0) != null
                            ? (Expression)Visit(context.expression(0))
                            : null;
                result = new ReturnStatement(expression, textSpan, FileNode);
                return result;
            }

            if (firstTokenType == JavaLexer.THROW)
            {
                expression = (Expression)Visit(context.expression(0));
                result = new ThrowStatement(expression, textSpan, FileNode);
                return result;
            }

            if (firstTokenType == JavaLexer.BREAK)
            {
                result = new BreakStatement(textSpan, FileNode);
                return result;
            }

            if (firstTokenType == JavaLexer.CONTINUE)
            {
                result = new ContinueStatement(textSpan, FileNode);
                return result;
            }

            if (firstTokenType == JavaLexer.SEMI)
            {
                result = new EmptyStatement(textSpan, FileNode);
                return result;
            }

            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitForControl(JavaParser.ForControlContext context)
        {
            JavaParser.EnhancedForControlContext forEach = context.enhancedForControl();
            if (forEach != null)
            {
                // TODO: modifiers
                var type = (TypeToken)Visit(forEach.typeType());
                var varDecId = (IdToken)Visit(forEach.variableDeclaratorId());
                var expr = (Expression)Visit(forEach.expression());
                var parent = (JavaParser.StatementContext)context.parent;
                var statement = (Statement)Visit(parent.statement(0));

                var result = new ForeachStatement(type, varDecId, expr, statement, parent.GetTextSpan(), FileNode);
                return result;
            }
            else
            {
                var initializers = new List<Statement>();
                var iterators = new List<Expression>();

                JavaParser.ForInitContext forInit = context.forInit();
                if (forInit != null)
                {
                    var varDec = forInit.localVariableDeclaration();
                    if (varDec != null)
                    {
                        var varDecStatement = (VariableDeclarationExpression)Visit(varDec);
                        initializers.Add(new ExpressionStatement(varDecStatement));
                    }
                    else
                    {
                        initializers.AddRange(forInit.expressionList().expression()
                            .Select(expr => 
                                {
                                    var ex = (Expression)Visit(expr);
                                    return ex == null ? null : new ExpressionStatement(ex, expr.GetTextSpan(), FileNode);
                                }).Where(stmt => stmt != null).ToArray());
                    }
                }
                Expression condition = context.expression() == null ? null : (Expression)Visit(context.expression());
            
                JavaParser.ExpressionListContext forUpdate = context.forUpdate;
                if (forUpdate != null)
                {
                    iterators.AddRange(forUpdate.expression()
                        .Select(expr => (Expression)Visit(expr))
                        .Where(iter => iter != null).ToArray());
                }

                var parent = (JavaParser.StatementContext)context.parent;
                var statement = (Statement)Visit(parent.statement(0));
                var result = new ForStatement(initializers, condition, iterators, statement, parent.GetTextSpan(), FileNode);
                return result;
            }
        }

        public UstNode VisitForInit(JavaParser.ForInitContext context)
        {
            return VisitChildren(context);
        }

        #region Switch

        public UstNode VisitSwitchBlockStatementGroup(JavaParser.SwitchBlockStatementGroupContext context)
        {
            Expression[] caseLabels = context.switchLabel().Select(label => (Expression)Visit(label))
                .Where(l => l != null).ToArray();
            Statement[] statements = context.blockStatement().Select(s => (Statement)Visit(s))
                .Where(s => s != null).ToArray();

            var result = new SwitchSection(caseLabels, statements, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitSwitchLabel(JavaParser.SwitchLabelContext context)
        {
            var result = Visit(context.GetChild(0));
            return result;
        }

        #endregion

        #region Try Catch Finally

        public UstNode VisitCatchClause(JavaParser.CatchClauseContext context)
        {
            var type = (TypeToken)Visit(context.catchType());
            var id = (IdToken)Visit(context.Identifier());
            var body = (BlockStatement)Visit(context.block());

            var result = new CatchClause(type, id, body, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitCatchType(JavaParser.CatchTypeContext context)
        {
            string[] names = context.qualifiedName().Select(name => ((StringLiteral)Visit(name))?.Text)
                .Where(n => n != null).ToArray();

            var result = new TypeToken(string.Join("|", names), context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitFinallyBlock(JavaParser.FinallyBlockContext context)
        {
            var result = (BlockStatement)Visit(context.block());
            return result;
        }

        public UstNode VisitResource(JavaParser.ResourceContext context)
        {
            var type = (TypeToken)Visit(context.classOrInterfaceType());
            var id = (IdToken)Visit(context.variableDeclaratorId());
            var initializer = (Expression)Visit(context.expression());

            var result = new VariableDeclarationExpression(type,
                new[] { new AssignmentExpression(id, initializer, context.variableDeclaratorId().GetTextSpan(), FileNode) },
                context.GetTextSpan(), FileNode);
            return result;
        }

        #endregion
    }
}
