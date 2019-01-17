using Microsoft.CodeAnalysis.CSharp.Syntax;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class CSharpRoslynParseTreeConverter
    {
        public override Ust VisitBlock(BlockSyntax node)
        {
            var statements = node.Statements.Select(s => (Statement)VisitAndReturnNullIfError(s))
                .ToList();
            var result = new BlockStatement(statements, node.GetTextSpan());
            return result;
        }

        public override Ust VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var type = ConvertType(base.Visit(node.Declaration.Type));
            AssignmentExpression[] variables =
                node.Declaration.Variables.Select(v => (AssignmentExpression)VisitAndReturnNullIfError(v))
                .Select(v =>
                {
                    //here we set the type text if it is hidden on the right
                    if (v is AssignmentExpression assignment
                        && assignment.Right is ArrayCreationExpression arrayCreation)
                    {
                        if (string.IsNullOrEmpty(arrayCreation.Type.TypeText))
                        {
                            arrayCreation.Type.TypeText = type.TypeText;
                        }
                    }
                    return v;
                })
                .ToArray();

            var resultExpression = new VariableDeclarationExpression(type, variables, node.Declaration.GetTextSpan());
            var result = new ExpressionStatement(resultExpression, node.GetTextSpan());
            return result;
        }

        public override Ust VisitBreakStatement(BreakStatementSyntax node)
        {
            var result = new BreakStatement(node.GetTextSpan());
            return result;
        }

        public override Ust VisitCheckedStatement(CheckedStatementSyntax node)
        {
            var result = (BlockStatement)VisitBlock(node.Block);
            return result;
        }

        public override Ust VisitContinueStatement(ContinueStatementSyntax node)
        {
            var result = new ContinueStatement(node.GetTextSpan());
            return result;
        }

        public override Ust VisitDoStatement(DoStatementSyntax node)
        {
            var embedded = (Statement)base.Visit(node.Statement);
            var condition = (Expression)base.Visit(node.Condition);

            var result = new DoWhileStatement(
                embedded,
                condition,
                node.GetTextSpan()
            );
            return result;
        }

        public override Ust VisitEmptyStatement(EmptyStatementSyntax node)
        {
            var result = new EmptyStatement(node.GetTextSpan());
            return result;
        }

        public override Ust VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var expression = (Expression)base.Visit(node.Expression);
            var result = new ExpressionStatement(expression, node.GetTextSpan());
            return result;
        }

        public override Ust VisitFixedStatement(FixedStatementSyntax node)
        {
            var statements = new List<Statement>();

            var varDec = (VariableDeclarationExpression)VisitVariableDeclaration(node.Declaration);
            statements.Add(new ExpressionStatement(varDec));
            statements.AddRange(GetChildStatements(node.Statement));

            var result = new BlockStatement(statements, node.GetTextSpan());
            return result;
        }

        public override Ust VisitForEachStatement(ForEachStatementSyntax node)
        {
            var type = ConvertType(base.Visit(node.Type));
            var var = new IdToken(node.Identifier.ValueText, node.Identifier.GetTextSpan());
            var inExpression = (Expression)base.Visit(node.Expression);
            var embedded = (Statement)base.Visit(node.Statement);

            var result = new ForeachStatement(type, var, inExpression, embedded, node.GetTextSpan());
            return result;
        }

        public override Ust VisitForStatement(ForStatementSyntax node)
        {
            Statement[] initializers = node.Declaration != null ?
                new Statement[]
                {
                    new ExpressionStatement((VariableDeclarationExpression) VisitVariableDeclaration(node.Declaration))
                } :
                node.Initializers.Select(init =>
                {
                    Expression ex = (Expression)VisitAndReturnNullIfError(init);
                    return ex == null ? null : new ExpressionStatement(ex, init.GetTextSpan());
                })
                .ToArray();

            var condition = (Expression)base.Visit(node.Condition);
            var iterators = node.Incrementors.Select(inc => (Expression)VisitAndReturnNullIfError(inc))
                .ToArray();
            var statement = (Statement)base.Visit(node.Statement);

            var result = new ForStatement(initializers, condition, iterators, statement, node.GetTextSpan());
            return result;
        }

        public override Ust VisitGotoStatement(GotoStatementSyntax node)
        {
            return new GotoStatement(
                new IdToken("label", node.GetTextSpan()),
                node.GetTextSpan()); // TODO implement;
        }

        public override Ust VisitIfStatement(IfStatementSyntax node)
        {
            var condition = (Expression)base.Visit(node.Condition);
            Statement trueStatement = (Statement)base.Visit(node.Statement);
            Statement falseStatement = node.Else == null
                ? null
                : (Statement)base.Visit(node.Else.Statement);

            var result = new IfElseStatement(
                condition,
                trueStatement,
                node.GetTextSpan())
            {
                FalseStatement = falseStatement
            };
            return result;
        }

        public override Ust VisitLabeledStatement(LabeledStatementSyntax node)
        {
            return new EmptyStatement(node.GetTextSpan());
        }

        /// <summary>
        /// lock(a) { b; c; } => { a; b; c; }
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override Ust VisitLockStatement(LockStatementSyntax node)
        {
            var statements = new List<Statement>();

            var expr = new ExpressionStatement((Expression)base.Visit(node.Expression),
                node.Expression.GetTextSpan());
            statements.Add(expr);
            statements.AddRange(GetChildStatements(node.Statement));

            var result = new BlockStatement(statements, node.GetTextSpan());
            return result;
        }

        public override Ust VisitReturnStatement(ReturnStatementSyntax node)
        {
            var expression = (Expression)base.Visit(node.Expression);
            var result = new ReturnStatement(expression, node.GetTextSpan());
            return result;
        }

        #region Switch

        public override Ust VisitSwitchStatement(SwitchStatementSyntax node)
        {
            var expression = (Expression)base.Visit(node.Expression);
            var sections = node.Sections.Select(s => (SwitchSection)VisitAndReturnNullIfError(s))
                .ToArray();

            var result = new SwitchStatement(
                expression,
                sections,
                node.GetTextSpan());
            return result;
        }

        public override Ust VisitSwitchSection(SwitchSectionSyntax node)
        {
            var caseLabels = node.Labels.Select(s => (Expression)VisitAndReturnNullIfError(s)).ToArray();
            var statements = node.Statements.Select(s => (Statement)VisitAndReturnNullIfError(s)).ToArray();

            var result = new SwitchSection(
                caseLabels,
                statements,
                node.GetTextSpan());
            return result;
        }

        public override Ust VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            var result = (Expression)base.Visit(node.Value);
            return result;
        }

        public override Ust VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
        {
            return new IdToken(node.Keyword.Text, node.GetTextSpan());
        }

        #endregion

        public override Ust VisitThrowStatement(ThrowStatementSyntax node)
        {
            var throwEpression = (Expression)base.Visit(node.Expression);
            var result = new ThrowStatement(throwEpression, node.GetTextSpan());
            return result;
        }

        #region Try Catch Finally

        public override Ust VisitTryStatement(TryStatementSyntax node)
        {
            var tryBlock = (BlockStatement)VisitBlock(node.Block);
            var catchClauses = node.Catches.Select(c => (CatchClause)VisitAndReturnNullIfError(c))
                .ToList();
            BlockStatement finallyStatement = null;
            if (node.Finally != null)
                finallyStatement = (BlockStatement)VisitFinallyClause(node.Finally);

            var result = new TryCatchStatement(tryBlock, node.GetTextSpan())
            {
                CatchClauses = catchClauses,
                FinallyBlock = finallyStatement
            };
            return result;
        }

        public override Ust VisitCatchClause(CatchClauseSyntax node)
        {
            TypeToken typeToken;
            IdToken varName;
            if (node.Declaration == null)
            {
                typeToken = new TypeToken("Exception", node.CatchKeyword.GetTextSpan());
                varName = new IdToken("e", node.CatchKeyword.GetTextSpan());
            }
            else
            {
                typeToken = ConvertType(base.Visit(node.Declaration.Type));
                varName = new IdToken(node.Declaration.Identifier.ValueText ?? "", node.Declaration.GetTextSpan());
            }

            var body = (BlockStatement)VisitBlock(node.Block);
            var result = new CatchClause(
                typeToken,
                varName,
                body,
                node.GetTextSpan());
            return result;
        }

        public override Ust VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            return base.VisitCatchDeclaration(node);
        }

        public override Ust VisitCatchFilterClause(CatchFilterClauseSyntax node)
        {
            throw new NotSupportedException();
        }

        public override Ust VisitFinallyClause(FinallyClauseSyntax node)
        {
            var result = (BlockStatement)VisitBlock(node.Block);
            return result;
        }

        #endregion

        public override Ust VisitUnsafeStatement(UnsafeStatementSyntax node)
        {
            var result = (BlockStatement)VisitBlock(node.Block);
            return result;
        }

        /// <summary>
        /// Conversion to TryStatement
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override Ust VisitUsingStatement(UsingStatementSyntax node)
        {
            var statements = new List<Statement>();
            Statement resourceAcquisition;
            if (node.Declaration != null)
                resourceAcquisition = new ExpressionStatement((VariableDeclarationExpression)VisitVariableDeclaration(node.Declaration));
            else
                resourceAcquisition = new ExpressionStatement((Expression)base.Visit(node.Expression), node.Expression.GetTextSpan());

            statements.Add(resourceAcquisition);
            statements.AddRange(GetChildStatements(node.Statement));

            var body = new BlockStatement(statements, node.GetTextSpan());
            var finallyStatement = new BlockStatement(new Statement[0], node.CloseParenToken.GetTextSpan());
            var result = new TryCatchStatement(body, node.GetTextSpan())
            {
                FinallyBlock = finallyStatement
            };
            return result;
        }

        public override Ust VisitWhileStatement(WhileStatementSyntax node)
        {
            var condition = (Expression)base.Visit(node.Condition);
            var embedded = (Statement)base.Visit(node.Statement);

            var result = new WhileStatement(condition, embedded, node.GetTextSpan());
            return result;
        }

        public override Ust VisitYieldStatement(YieldStatementSyntax node)
        {
            var expression = (Expression)base.Visit(node.Expression);
            var result = new ReturnStatement(expression, node.GetTextSpan());
            return result;
        }

        public override Ust VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var typeToken = ConvertType(base.Visit(node.Type));
            AssignmentExpression[] vars = node
                .Variables.Select(v => (AssignmentExpression)VisitAndReturnNullIfError(v))
                .ToArray();

            var result = new VariableDeclarationExpression(typeToken, vars, node.GetTextSpan());
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
