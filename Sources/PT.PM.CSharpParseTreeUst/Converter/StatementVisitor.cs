using System;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class RoslynUstCommonConverterVisitor
    {
        public override UstNode VisitBlock(BlockSyntax node)
        {
            var statements = node.Statements.Select(s => (Statement)VisitAndReturnNullIfError(s))
                .ToList();
            var result = new BlockStatement(statements, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var type = ConvertType(base.Visit(node.Declaration.Type));
            AssignmentExpression[] variables = 
                node.Declaration.Variables.Select(v => (AssignmentExpression)VisitAndReturnNullIfError(v))
                .ToArray();

            var resultExpression = new VariableDeclarationExpression(type, variables, node.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(resultExpression);
            return result;
        }

        public override UstNode VisitBreakStatement(BreakStatementSyntax node)
        {
            var result = new BreakStatement(node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitCheckedStatement(CheckedStatementSyntax node)
        {
            var result = (BlockStatement)VisitBlock(node.Block);
            return result;
        }

        public override UstNode VisitContinueStatement(ContinueStatementSyntax node)
        {
            var result = new ContinueStatement(node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitDoStatement(DoStatementSyntax node)
        {
            var embedded = (Statement)base.Visit(node.Statement);
            var condition = (Expression)base.Visit(node.Condition);

            var result = new DoWhileStatement(
                embedded,
                condition,
                node.GetTextSpan(),
                FileNode
            );
            return result;
        }

        public override UstNode VisitEmptyStatement(EmptyStatementSyntax node)
        {
            var result = new EmptyStatement(node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var expression = (Expression)base.Visit(node.Expression);
            var result = new ExpressionStatement(expression, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitFixedStatement(FixedStatementSyntax node)
        {
            var statements = new List<Statement>();

            var varDec = (VariableDeclarationExpression)VisitVariableDeclaration(node.Declaration);
            statements.Add(new ExpressionStatement(varDec));
            statements.AddRange(GetChildStatements(node.Statement));

            var result = new BlockStatement(statements, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitForEachStatement(ForEachStatementSyntax node)
        {
            var type = ConvertType(base.Visit(node.Type));
            var var = new IdToken(node.Identifier.ValueText, node.Identifier.GetTextSpan(), FileNode);
            var inExpression = (Expression) base.Visit(node.Expression);
            var embedded = (Statement) base.Visit(node.Statement);

            var result = new ForeachStatement(type, var, inExpression, embedded, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitForStatement(ForStatementSyntax node)
        {
            Statement[] initializers = node.Declaration != null ?
                new Statement[]
                {
                    new ExpressionStatement((VariableDeclarationExpression) VisitVariableDeclaration(node.Declaration))
                } :
                node.Initializers.Select(init => {
                    Expression ex = (Expression)VisitAndReturnNullIfError(init);
                    return ex == null ? null : new ExpressionStatement(ex, init.GetTextSpan(), FileNode);
                })
                .ToArray();

            var condition = (Expression) base.Visit(node.Condition);
            var iterators = node.Incrementors.Select(inc => (Expression)VisitAndReturnNullIfError(inc))
                .ToArray();
            var statement = (Statement)base.Visit(node.Statement);

            var result = new ForStatement(initializers, condition, iterators, statement, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitGotoStatement(GotoStatementSyntax node)
        {
            return new GotoStatement(
                new IdToken("label", node.GetTextSpan(), FileNode),
                node.GetTextSpan(), FileNode); // TODO implement;
        }
        
        public override UstNode VisitIfStatement(IfStatementSyntax node)
        {
            var condition = (Expression) base.Visit(node.Condition);
            Statement trueStatement = (Statement)base.Visit(node.Statement);
            Statement falseStatement = node.Else == null
                ? null
                : (Statement)base.Visit(node.Else.Statement);
            
            var result = new IfElseStatement(
                condition,
                trueStatement,
                node.GetTextSpan(),
                FileNode)
            {
                FalseStatement = falseStatement
            };
            return result;
        }

        public override UstNode VisitLabeledStatement(LabeledStatementSyntax node)
        {
            return new EmptyStatement(node.GetTextSpan(), FileNode);
        }

        /// <summary>
        /// lock(a) { b; c; } => { a; b; c; }
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override UstNode VisitLockStatement(LockStatementSyntax node)
        {
            var statements = new List<Statement>();

            var expr = new ExpressionStatement((Expression) base.Visit(node.Expression),
                node.Expression.GetTextSpan(), FileNode);
            statements.Add(expr);
            statements.AddRange(GetChildStatements(node.Statement));

            var result = new BlockStatement(statements, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitReturnStatement(ReturnStatementSyntax node)
        {
            var expression = (Expression) base.Visit(node.Expression);
            var result = new ReturnStatement(expression, node.GetTextSpan(), FileNode);
            return result;
        }

        #region Switch

        public override UstNode VisitSwitchStatement(SwitchStatementSyntax node)
        {
            var expression = (Expression) base.Visit(node.Expression);
            var sections = node.Sections.Select(s => (SwitchSection) VisitAndReturnNullIfError(s))
                .ToArray();

            var result = new SwitchStatement(
                expression,
                sections,
                node.GetTextSpan(),
                FileNode);
            return result;
        }

        public override UstNode VisitSwitchSection(SwitchSectionSyntax node)
        {
            var caseLabels = node.Labels.Select(s => (Expression)VisitAndReturnNullIfError(s)).ToArray();
            var statements = node.Statements.Select(s => (Statement)VisitAndReturnNullIfError(s)).ToArray();

            var result = new SwitchSection(
                caseLabels,
                statements,
                node.GetTextSpan(),
                FileNode);
            return result;
        }

        public override UstNode VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            var result = (Expression) base.Visit(node.Value);
            return result;
        }

        public override UstNode VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
        {
            return null;
        }

        #endregion

        public override UstNode VisitThrowStatement(ThrowStatementSyntax node)
        {
            var throwEpression = (Expression) base.Visit(node.Expression);
            var result = new ThrowStatement(throwEpression, node.GetTextSpan(), FileNode);
            return result;
        }

        #region Try Catch Finally

        public override UstNode VisitTryStatement(TryStatementSyntax node)
        {
            var tryBlock = (BlockStatement)VisitBlock(node.Block);
            var catchClauses = node.Catches.Select(c => (CatchClause)VisitAndReturnNullIfError(c))
                .ToList();
            BlockStatement finallyStatement = null;
            if (node.Finally != null)
                finallyStatement = (BlockStatement)VisitFinallyClause(node.Finally);

            var result = new TryCatchStatement(tryBlock, node.GetTextSpan(), FileNode)
            {
                CatchClauses = catchClauses,
                FinallyBlock = finallyStatement
            };
            return result;
        }

        public override UstNode VisitCatchClause(CatchClauseSyntax node)
        {
            TypeToken typeToken;
            IdToken varName;
            if (node.Declaration == null)
            {
                typeToken = new TypeToken("Exception", node.CatchKeyword.GetTextSpan(), FileNode);
                varName = new IdToken("e", node.CatchKeyword.GetTextSpan(), FileNode);
            }
            else
            {
                typeToken = ConvertType(base.Visit(node.Declaration.Type));
                varName = new IdToken(node.Declaration.Identifier.ValueText ?? "", node.Declaration.GetTextSpan(), FileNode);
            }

            var body = (BlockStatement) VisitBlock(node.Block);
            var result = new CatchClause(
                typeToken,
                varName,
                body,
                node.GetTextSpan(),
                FileNode);
            return result;
        }

        public override UstNode VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            return base.VisitCatchDeclaration(node);
        }

        public override UstNode VisitCatchFilterClause(CatchFilterClauseSyntax node)
        {
            throw new NotSupportedException();
        }

        public override UstNode VisitFinallyClause(FinallyClauseSyntax node)
        {
            var result = (BlockStatement)VisitBlock(node.Block);
            return result;
        }

        #endregion

        public override UstNode VisitUnsafeStatement(UnsafeStatementSyntax node)
        {
            var result = (BlockStatement)VisitBlock(node.Block);
            return result;
        }

        /// <summary>
        /// Conversion to TryStatement
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override UstNode VisitUsingStatement(UsingStatementSyntax node)
        {
            var statements = new List<Statement>();
            Statement resourceAcquisition;
            if (node.Declaration != null)
                resourceAcquisition = new ExpressionStatement((VariableDeclarationExpression)VisitVariableDeclaration(node.Declaration));
            else
                resourceAcquisition = new ExpressionStatement((Expression)base.Visit(node.Expression), node.Expression.GetTextSpan(), FileNode);
            
            statements.Add(resourceAcquisition);
            statements.AddRange(GetChildStatements(node.Statement));

            var body = new BlockStatement(statements, node.GetTextSpan(), FileNode);
            var finallyStatement = new BlockStatement(new Statement[0], node.CloseParenToken.GetTextSpan(), FileNode);
            var result = new TryCatchStatement(body, node.GetTextSpan(), FileNode)
            {
                FinallyBlock = finallyStatement
            };
            return result;
        }

        public override UstNode VisitWhileStatement(WhileStatementSyntax node)
        {
            var condition = (Expression)base.Visit(node.Condition);
            var embedded = (Statement)base.Visit(node.Statement);

            var result = new WhileStatement(condition, embedded, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitYieldStatement(YieldStatementSyntax node)
        {
            var expression = (Expression)base.Visit(node.Expression);
            var result = new ReturnStatement(expression, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var typeToken = ConvertType(base.Visit(node.Type));
            AssignmentExpression[] vars = node
                .Variables.Select(v => (AssignmentExpression)VisitAndReturnNullIfError(v))
                .ToArray();

            var result = new VariableDeclarationExpression(typeToken, vars, node.GetTextSpan(), FileNode);
            return result;
        }
        
        private IEnumerable<Statement> GetChildStatements(StatementSyntax node)
        {
            var blockStatement = node as BlockSyntax;
            if (blockStatement == null)
                return new[] { (Statement)base.Visit(node) };
            return blockStatement.Statements.Select(s => (Statement)VisitAndReturnNullIfError(s))
                .ToArray();
        }
    }
}
