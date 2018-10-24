using Esprima.Ast;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.TypeMembers;
using System;
using System.Collections.Generic;
using UstExprs = PT.PM.Common.Nodes.Expressions;
using UstSpecific = PT.PM.Common.Nodes.Specific;
using UstStmts = PT.PM.Common.Nodes.Statements;
using UstTokens = PT.PM.Common.Nodes.Tokens;

namespace PT.PM.JavaScriptParseTreeUst
{
    public partial class JavaScriptEsprimaParseTreeConverter
    {
        private UstStmts.Statement VisitStatement(Statement statement)
        {
            try
            {
                switch (statement)
                {
                    case BlockStatement blockStatement:
                        return VisitBlockStatement(blockStatement);
                    case BreakStatement breakStatement:
                        return VisitBreakStatement(breakStatement);
                    case ContinueStatement continueStatement:
                        return VisitContinueStatement(continueStatement);
                    case DoWhileStatement doWhileStatement:
                        return VisitDoWhileStatement(doWhileStatement);
                    case DebuggerStatement debuggerStatement:
                        return VisitDebuggerStatement(debuggerStatement);
                    case EmptyStatement emptyStatement:
                        return VisitEmptyStatement(emptyStatement);
                    case ExpressionStatement expressionStatement:
                        return VisitExpressionStatement(expressionStatement);
                    case ForStatement forStatement:
                        return VisitForStatement(forStatement);
                    case ForInStatement forInStatement:
                        return VisitForInStatement(forInStatement);
                    case ForOfStatement forOfStatement:
                        return VisitForOfStatement(forOfStatement);
                    case FunctionDeclaration functionDeclaration:
                        return VisitFunctionDeclaration(functionDeclaration);
                    case IfStatement ifStatement:
                        return VisitIfStatement(ifStatement);
                    case LabeledStatement labeledStatement:
                        return VisitLabeledStatement(labeledStatement);
                    case ReturnStatement returnStatement:
                        return VisitReturnStatement(returnStatement);
                    case SwitchStatement switchStatement:
                        return VisitSwitchStatement(switchStatement);
                    case ThrowStatement throwStatement:
                        return VisitThrowStatement(throwStatement);
                    case TryStatement tryStatement:
                        return VisitTryStatement(tryStatement);
                    case VariableDeclaration variableDeclaration:
                        return VisitVariableDeclaration(variableDeclaration);
                    case WhileStatement whileStatement:
                        return VisitWhileStatement(whileStatement);
                    case WithStatement withStatement:
                        return VisitWithStatement(withStatement);
                    case Program program:
                        return VisitProgram(program);
                    default:
                        return VisitUnknownStatement(statement);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(new ConversionException(SourceCodeFile, ex));
                return null;
            }
        }

        private UstStmts.BlockStatement VisitBlockStatement(BlockStatement blockStatement)
        {
            List<UstStmts.Statement> statements = VisitStatements(blockStatement.Body);
            return new UstStmts.BlockStatement(statements, GetTextSpan(blockStatement));
        }

        private UstStmts.BreakStatement VisitBreakStatement(BreakStatement breakStatement)
        {
            return new UstStmts.BreakStatement(GetTextSpan(breakStatement));
        }

        private UstStmts.ContinueStatement VisitContinueStatement(ContinueStatement continueStatement)
        {
            return new UstStmts.ContinueStatement(GetTextSpan(continueStatement));
        }

        private UstStmts.DoWhileStatement VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            var statement = VisitStatement(doWhileStatement.Body);
            var condition = VisitExpression(doWhileStatement.Test);
            return new UstStmts.DoWhileStatement(statement, condition, GetTextSpan(doWhileStatement));
        }

        private UstSpecific.DebuggerStatement VisitDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            return new UstSpecific.DebuggerStatement(GetTextSpan(debuggerStatement));
        }

        private UstStmts.EmptyStatement VisitEmptyStatement(EmptyStatement emptyStatement)
        {
            return new UstStmts.EmptyStatement(GetTextSpan(emptyStatement));
        }

        private UstStmts.ExpressionStatement VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            var expression = VisitExpression(expressionStatement.Expression);
            return new UstStmts.ExpressionStatement(expression, GetTextSpan(expressionStatement));
        }

        private UstStmts.ForStatement VisitForStatement(ForStatement forStatement)
        {
            var initList = new List<UstStmts.Statement>(1);
            if (forStatement.Init != null)
            {
                initList.Add(Visit(forStatement.Init).ToStatementIfRequired());
            }

            UstExprs.Expression condition = forStatement.Test != null
                ? VisitExpression(forStatement.Test)
                : null;

            var iteratorList = new List<UstExprs.Expression>(1);
            if (forStatement.Update != null)
            {
                iteratorList.Add(VisitExpression(forStatement.Update));
            }

            var body = VisitStatement(forStatement.Body);

            return new UstStmts.ForStatement(initList, condition, iteratorList, body, GetTextSpan(forStatement));
        }

        private UstStmts.ForeachStatement VisitForInStatement(ForInStatement forInStatement)
        {
            UstTokens.IdToken name = null;
            var left = Visit(forInStatement.Left);
            if (left is UstTokens.IdToken token)
            {
                name = token;
            }
            else if (left is UstStmts.ExpressionStatement exprStmt &&
                exprStmt.Expression is UstExprs.VariableDeclarationExpression varDecl &&
                varDecl.Variables.Count > 0 &&
                varDecl.Variables[0].Left is UstTokens.IdToken idToken)
            {
                name = idToken;
            }
            var inExpression = VisitExpression(forInStatement.Right);
            var body = VisitStatement(forInStatement.Body);
            return new UstStmts.ForeachStatement(null, name, inExpression, body, GetTextSpan(forInStatement));
        }

        // TODO: maybe introduce the new ForOfStatement node
        private UstStmts.ForeachStatement VisitForOfStatement(ForOfStatement forOfStatement)
        {
            UstTokens.IdToken name = null;
            var left = Visit(forOfStatement.Left);
            if (left is UstTokens.IdToken token)
            {
                name = token;
            }
            else if (left is UstStmts.ExpressionStatement exprStmt &&
                exprStmt.Expression is UstExprs.VariableDeclarationExpression varDecl &&
                varDecl.Variables.Count > 0 &&
                varDecl.Variables[0].Left is UstTokens.IdToken idToken)
            {
                name = idToken;
            }
            var inExpression = VisitExpression(forOfStatement.Right);
            var body = VisitStatement(forOfStatement.Body);
            return new UstStmts.ForeachStatement(null, name, inExpression, body, GetTextSpan(forOfStatement));
        }

        private UstStmts.WrapperStatement VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            var name = functionDeclaration.Id != null ? VisitIdentifier(functionDeclaration.Id) : null;
            var paramDecls = VisitParameters(functionDeclaration.Params);
            var body = VisitBlockStatement(functionDeclaration.Body);
            return new UstStmts.WrapperStatement(new MethodDeclaration(name, paramDecls, body, GetTextSpan(functionDeclaration)));
        }

        private UstStmts.IfElseStatement VisitIfStatement(IfStatement ifStatement)
        {
            var condition = VisitExpression(ifStatement.Test);
            var trueStatement = VisitStatement(ifStatement.Consequent);
            return new UstStmts.IfElseStatement(condition, trueStatement, GetTextSpan(ifStatement))
            {
                FalseStatement = ifStatement.Alternate != null ? VisitStatement(ifStatement.Alternate) : null
            };
        }

        private UstStmts.LabelStatement VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            var identifier = VisitIdentifier(labeledStatement.Label);
            var body = VisitStatement(labeledStatement.Body);
            return new UstStmts.LabelStatement(identifier, body, GetTextSpan(labeledStatement));
        }

        private UstStmts.ReturnStatement VisitReturnStatement(ReturnStatement returnStatement)
        {
            var returnExpr = returnStatement.Argument != null
                ? VisitExpression(returnStatement.Argument)
                : null;
            return new UstStmts.ReturnStatement(returnExpr, GetTextSpan(returnStatement));
        }

        private UstStmts.Switch.SwitchStatement VisitSwitchStatement(SwitchStatement switchStatement)
        {
            var expression = VisitExpression(switchStatement.Discriminant);
            var switchSections = new List<UstStmts.Switch.SwitchSection>(switchStatement.Cases.Count);

            foreach (SwitchCase @case in switchStatement.Cases)
            {
                var caseLabels = new List<UstExprs.Expression>();
                if (@case.Test != null)
                {
                    // Default label if null
                    caseLabels.Add(VisitExpression(@case.Test));
                }

                var switchSection = new UstStmts.Switch.SwitchSection(
                    caseLabels,
                    VisitStatements(@case.Consequent),
                    GetTextSpan(@case));
                switchSections.Add(switchSection);
            }

            return new UstStmts.Switch.SwitchStatement(expression, switchSections, GetTextSpan(switchStatement));
        }

        private UstStmts.ThrowStatement VisitThrowStatement(ThrowStatement throwStatement)
        {
            var throwExpression = VisitExpression(throwStatement.Argument);
            return new UstStmts.ThrowStatement(throwExpression, GetTextSpan(throwStatement));
        }

        private UstStmts.TryCatchFinally.TryCatchStatement VisitTryStatement(TryStatement tryStatement)
        {
            var block = (UstStmts.BlockStatement)VisitStatement(tryStatement.Block);

            var catchClauses = new List<UstStmts.TryCatchFinally.CatchClause>();
            if (tryStatement.Handler != null)
            {
                var body = VisitBlockStatement(tryStatement.Handler.Body);
                var varName = VisitArrayPatternElement(tryStatement.Handler.Param);
                catchClauses.Add(new UstStmts.TryCatchFinally.CatchClause(varName?.Type, varName?.Name, body, GetTextSpan(tryStatement.Handler)));
            }

            return new UstStmts.TryCatchFinally.TryCatchStatement(block, GetTextSpan(tryStatement))
            {
                CatchClauses = catchClauses,
                FinallyBlock = tryStatement.Finalizer != null ? (UstStmts.BlockStatement)VisitStatement(tryStatement.Finalizer) : null
            };
        }

        private UstStmts.ExpressionStatement VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            var variables = new List<UstExprs.AssignmentExpression>(variableDeclaration.Declarations.Count);

            foreach (VariableDeclarator decl in variableDeclaration.Declarations)
            {
                var left = (UstExprs.Expression)Visit(decl.Id); // TODO: fix?
                var initExpr = decl.Init != null ? VisitExpression(decl.Init) : null;
                variables.Add(new UstExprs.AssignmentExpression(left, initExpr, GetTextSpan(decl)));
            }

            var varDecl = new UstExprs.VariableDeclarationExpression(null, variables, GetTextSpan(variableDeclaration));
            return new UstStmts.ExpressionStatement(varDecl);
        }

        private UstStmts.WhileStatement VisitWhileStatement(WhileStatement whileStatement)
        {
            var condition = VisitExpression(whileStatement.Test);
            var statement = VisitStatement(whileStatement.Body);
            return new UstStmts.WhileStatement(condition, statement, GetTextSpan(whileStatement));
        }

        private UstSpecific.WithStatement VisitWithStatement(WithStatement withStatement)
        {
            var expr = VisitExpression(withStatement.Object);
            var stmt = VisitStatement(withStatement.Body);
            return new UstSpecific.WithStatement(expr, stmt, GetTextSpan(withStatement));
        }

        public UstStmts.Statement VisitProgram(Program program)
        {
            var statements = VisitStatements(program.Body);
            return new UstStmts.BlockStatement(statements, GetTextSpan(program));
        }

        private List<UstStmts.Statement> VisitStatements(List<StatementListItem> listItems)
        {
            var statements = new List<UstStmts.Statement>(listItems.Count);

            foreach (StatementListItem listItem in listItems)
            {
                statements.Add(VisitStatementListItem(listItem).ToStatementIfRequired());
            }

            return statements;
        }

        private Ust VisitStatementListItem(StatementListItem listItem)
        {
            if (listItem is Statement statement)
            {
                return VisitStatement(statement);
            }
            else if (listItem is Declaration declaration)
            {
                return VisitDeclaration(declaration);
            }

            Logger.LogDebug($"Unknown {nameof(StatementListItem)} type {listItem?.GetType().Name}"); // TODO
            return null;
        }

        private UstStmts.Statement VisitUnknownStatement(Statement statement)
        {
            string message = statement == null
                ? $"{nameof(statement)} can not be null"
                : $"Unknow {nameof(Statement)} type {statement.Type}";
            Logger.LogError(new ConversionException(SourceCodeFile, message: message));
            return null;
        }
    }
}
