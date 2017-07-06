using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.JavaScriptParseTreeUst.Parser;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.Linq;
using System;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.GeneralScope;

namespace PT.PM.JavaScriptParseTreeUst
{
    public partial class JavaScriptAntlrUstConverterVisitor : AntlrDefaultVisitor, IECMAScriptVisitor<UstNode>
    {
        public JavaScriptAntlrUstConverterVisitor(string fileName, string fileData)
            : base(fileName, fileData)
        {
        }

        public UstNode VisitTernaryExpression([NotNull] ECMAScriptParser.TernaryExpressionContext context)
        {
            var condition = (Expression)Visit(context.singleExpression(0));
            var trueExpression = (Expression)Visit(context.singleExpression(1));
            var falseExpression = (Expression)Visit(context.singleExpression(2));
            return new ConditionalExpression(condition, trueExpression, falseExpression, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitLogicalAndExpression([NotNull] ECMAScriptParser.LogicalAndExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitPreIncrementExpression([NotNull] ECMAScriptParser.PreIncrementExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitObjectLiteralExpression([NotNull] ECMAScriptParser.ObjectLiteralExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitInExpression([NotNull] ECMAScriptParser.InExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitLogicalOrExpression([NotNull] ECMAScriptParser.LogicalOrExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitNotExpression([NotNull] ECMAScriptParser.NotExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitPreDecreaseExpression([NotNull] ECMAScriptParser.PreDecreaseExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitArgumentsExpression([NotNull] ECMAScriptParser.ArgumentsExpressionContext context)
        {
            var target = (Expression)Visit(context.singleExpression());
            var argsNode = (ArgsNode)Visit(context.arguments());
            var result = new InvocationExpression(target, argsNode, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitThisExpression([NotNull] ECMAScriptParser.ThisExpressionContext context)
        {
            return new ThisReferenceToken(context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="AnonymousMethodExpression"/></returns>
        public UstNode VisitFunctionExpression([NotNull] ECMAScriptParser.FunctionExpressionContext context)
        {
            var body = (BlockStatement)Visit(context.functionBody());
            var result = new AnonymousMethodExpression(new ParameterDeclaration[0], body, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitPostDecreaseExpression([NotNull] ECMAScriptParser.PostDecreaseExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitUnaryMinusExpression([NotNull] ECMAScriptParser.UnaryMinusExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitAssignmentExpression([NotNull] ECMAScriptParser.AssignmentExpressionContext context)
        {
            Expression left = Visit(context.singleExpression(0)).ToExpressionIfRequired();
            Expression right = Visit(context.singleExpression(1)).ToExpressionIfRequired();
            return new AssignmentExpression(left, right, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitTypeofExpression([NotNull] ECMAScriptParser.TypeofExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitInstanceofExpression([NotNull] ECMAScriptParser.InstanceofExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitUnaryPlusExpression([NotNull] ECMAScriptParser.UnaryPlusExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitDeleteExpression([NotNull] ECMAScriptParser.DeleteExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitEqualityExpression([NotNull] ECMAScriptParser.EqualityExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitBitXOrExpression([NotNull] ECMAScriptParser.BitXOrExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitMultiplicativeExpression([NotNull] ECMAScriptParser.MultiplicativeExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitBitShiftExpression([NotNull] ECMAScriptParser.BitShiftExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitParenthesizedExpression([NotNull] ECMAScriptParser.ParenthesizedExpressionContext context)
        {
            return Visit(context.expressionSequence());
        }

        public UstNode VisitPostIncrementExpression([NotNull] ECMAScriptParser.PostIncrementExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitAdditiveExpression([NotNull] ECMAScriptParser.AdditiveExpressionContext context)
        {
            Expression result = (Expression)CreateBinaryOperatorExpression(
                    context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
            return result;
        }

        public UstNode VisitRelationalExpression([NotNull] ECMAScriptParser.RelationalExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitBitNotExpression([NotNull] ECMAScriptParser.BitNotExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitNewExpression([NotNull] ECMAScriptParser.NewExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitLiteralExpression([NotNull] ECMAScriptParser.LiteralExpressionContext context)
        {
            return Visit(context.literal());
        }

        public UstNode VisitMemberDotExpression([NotNull] ECMAScriptParser.MemberDotExpressionContext context)
        {
            var target = (Expression)Visit(context.singleExpression());
            var name = (Expression)Visit(context.identifierName());
            return new MemberReferenceExpression(target, name, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitArrayLiteralExpression([NotNull] ECMAScriptParser.ArrayLiteralExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitMemberIndexExpression([NotNull] ECMAScriptParser.MemberIndexExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitIdentifierExpression([NotNull] ECMAScriptParser.IdentifierExpressionContext context)
        {
            return new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
        }

        public UstNode VisitBitAndExpression([NotNull] ECMAScriptParser.BitAndExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                   context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitBitOrExpression([NotNull] ECMAScriptParser.BitOrExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitAssignmentOperatorExpression([NotNull] ECMAScriptParser.AssignmentOperatorExpressionContext context)
        {
            var left = (Expression)Visit(context.singleExpression(0));
            var right = (Expression)Visit(context.singleExpression(1));
            return new AssignmentExpression(left, right, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitVoidExpression([NotNull] ECMAScriptParser.VoidExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitPropertyExpressionAssignment([NotNull] ECMAScriptParser.PropertyExpressionAssignmentContext context) { return VisitChildren(context); }

        public UstNode VisitPropertySetter([NotNull] ECMAScriptParser.PropertySetterContext context) { return VisitChildren(context); }

        public UstNode VisitPropertyGetter([NotNull] ECMAScriptParser.PropertyGetterContext context) { return VisitChildren(context); }

        public UstNode VisitDoStatement([NotNull] ECMAScriptParser.DoStatementContext context) { return VisitChildren(context); }

        public UstNode VisitForVarStatement([NotNull] ECMAScriptParser.ForVarStatementContext context) { return VisitChildren(context); }

        public UstNode VisitForVarInStatement([NotNull] ECMAScriptParser.ForVarInStatementContext context) { return VisitChildren(context); }

        public UstNode VisitWhileStatement([NotNull] ECMAScriptParser.WhileStatementContext context) { return VisitChildren(context); }

        public UstNode VisitForStatement([NotNull] ECMAScriptParser.ForStatementContext context) { return VisitChildren(context); }

        public UstNode VisitForInStatement([NotNull] ECMAScriptParser.ForInStatementContext context) { return VisitChildren(context); }

        public UstNode VisitProgram([NotNull] ECMAScriptParser.ProgramContext context)
        {
            FileNode.Root = Visit(context.sourceElements());
            return FileNode;
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitSourceElements([NotNull] ECMAScriptParser.SourceElementsContext context)
        {
            var statements = context.sourceElement().Select(element => Visit(element).ToStatementIfRequired()).ToList();
            return new BlockStatement(statements, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitSourceElement([NotNull] ECMAScriptParser.SourceElementContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitStatement([NotNull] ECMAScriptParser.StatementContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitBlock([NotNull] ECMAScriptParser.BlockContext context)
        {
            BlockStatement result;
            if (context.statementList() != null)
            {
                result = (BlockStatement)Visit(context.statementList());
            }
            else
            {
                result = new BlockStatement();
            }
            return result;
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitStatementList([NotNull] ECMAScriptParser.StatementListContext context)
        {
            return new BlockStatement(
                context.statement().Select(s => Visit(s).ToStatementIfRequired()).ToArray(),
                context.GetTextSpan(), FileNode);
        }

        public UstNode VisitVariableStatement([NotNull] ECMAScriptParser.VariableStatementContext context) { return VisitChildren(context); }

        public UstNode VisitVariableDeclarationList([NotNull] ECMAScriptParser.VariableDeclarationListContext context) { return VisitChildren(context); }

        public UstNode VisitVariableDeclaration([NotNull] ECMAScriptParser.VariableDeclarationContext context) { return VisitChildren(context); }

        public UstNode VisitInitialiser([NotNull] ECMAScriptParser.InitialiserContext context) { return VisitChildren(context); }

        public UstNode VisitEmptyStatement([NotNull] ECMAScriptParser.EmptyStatementContext context)
        {
            return new EmptyStatement(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitExpressionStatement([NotNull] ECMAScriptParser.ExpressionStatementContext context) { return VisitChildren(context); }

        public UstNode VisitIfStatement([NotNull] ECMAScriptParser.IfStatementContext context) { return VisitChildren(context); }

        public UstNode VisitIterationStatement([NotNull] ECMAScriptParser.IterationStatementContext context) { return VisitChildren(context); }

        public UstNode VisitContinueStatement([NotNull] ECMAScriptParser.ContinueStatementContext context) { return VisitChildren(context); }

        public UstNode VisitBreakStatement([NotNull] ECMAScriptParser.BreakStatementContext context) { return VisitChildren(context); }

        /// <returns><see cref="ReturnStatement"/></returns>
        public UstNode VisitReturnStatement([NotNull] ECMAScriptParser.ReturnStatementContext context)
        {
            Expression expression = null;
            if (context.expressionSequence() != null)
            {
                expression = (Expression)Visit(context.expressionSequence());
            }
            return new ReturnStatement(expression, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitWithStatement([NotNull] ECMAScriptParser.WithStatementContext context) { return VisitChildren(context); }

        public UstNode VisitSwitchStatement([NotNull] ECMAScriptParser.SwitchStatementContext context) { return VisitChildren(context); }

        public UstNode VisitCaseBlock([NotNull] ECMAScriptParser.CaseBlockContext context) { return VisitChildren(context); }

        public UstNode VisitCaseClauses([NotNull] ECMAScriptParser.CaseClausesContext context) { return VisitChildren(context); }

        public UstNode VisitCaseClause([NotNull] ECMAScriptParser.CaseClauseContext context) { return VisitChildren(context); }

        public UstNode VisitDefaultClause([NotNull] ECMAScriptParser.DefaultClauseContext context) { return VisitChildren(context); }

        public UstNode VisitLabelledStatement([NotNull] ECMAScriptParser.LabelledStatementContext context) { return VisitChildren(context); }

        public UstNode VisitThrowStatement([NotNull] ECMAScriptParser.ThrowStatementContext context) { return VisitChildren(context); }

        /// <returns><see cref="TryCatchStatement"/></returns>
        public UstNode VisitTryStatement([NotNull] ECMAScriptParser.TryStatementContext context)
        {
            var catchClauses = new List<CatchClause>();
            if (context.catchProduction() != null)
            {
                catchClauses.Add((CatchClause)Visit(context.catchProduction()));
            }
            var result = new TryCatchStatement
            {
                TryBlock = (BlockStatement)Visit(context.block()),
                CatchClauses = catchClauses,
                FinallyBlock = context.finallyProduction() != null ? (BlockStatement)Visit(context.finallyProduction()) : null,
                TextSpan = context.GetTextSpan(),
                FileNode = FileNode
            };
            return result;
        }

        /// <returns><see cref="CatchClause"/></returns>
        public UstNode VisitCatchProduction([NotNull] ECMAScriptParser.CatchProductionContext context)
        {
            var identifier = context.Identifier();
            var result = new CatchClause
            {
                VarName = new IdToken(identifier.GetText(), identifier.GetTextSpan(), FileNode),
                Body = (BlockStatement)Visit(context.block())
            };
            return result;
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitFinallyProduction([NotNull] ECMAScriptParser.FinallyProductionContext context)
        {
            return Visit(context.block());
        }

        public UstNode VisitDebuggerStatement([NotNull] ECMAScriptParser.DebuggerStatementContext context) { return VisitChildren(context); }

        public UstNode VisitFunctionDeclaration([NotNull] ECMAScriptParser.FunctionDeclarationContext context) { return VisitChildren(context); }

        public UstNode VisitFormalParameterList([NotNull] ECMAScriptParser.FormalParameterListContext context) { return VisitChildren(context); }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitFunctionBody([NotNull] ECMAScriptParser.FunctionBodyContext context)
        {
            BlockStatement result;
            if (context.sourceElements() == null)
            {
                result = new BlockStatement(new Statement[] { new EmptyStatement(context.GetTextSpan(), FileNode) }, FileNode);
            }
            else
            {
                result = (BlockStatement)Visit(context.sourceElements());
            }
            return result;
        }

        public UstNode VisitArrayLiteral([NotNull] ECMAScriptParser.ArrayLiteralContext context) { return VisitChildren(context); }

        public UstNode VisitElementList([NotNull] ECMAScriptParser.ElementListContext context) { return VisitChildren(context); }

        public UstNode VisitElision([NotNull] ECMAScriptParser.ElisionContext context) { return VisitChildren(context); }

        public UstNode VisitObjectLiteral([NotNull] ECMAScriptParser.ObjectLiteralContext context) { return VisitChildren(context); }

        public UstNode VisitPropertyNameAndValueList([NotNull] ECMAScriptParser.PropertyNameAndValueListContext context) { return VisitChildren(context); }

        public UstNode VisitPropertyAssignment([NotNull] ECMAScriptParser.PropertyAssignmentContext context) { return VisitChildren(context); }

        public UstNode VisitPropertyName([NotNull] ECMAScriptParser.PropertyNameContext context) { return VisitChildren(context); }

        public UstNode VisitPropertySetParameterList([NotNull] ECMAScriptParser.PropertySetParameterListContext context) { return VisitChildren(context); }

        public UstNode VisitArguments([NotNull] ECMAScriptParser.ArgumentsContext context)
        {
            return context.argumentList() != null ? (ArgsNode)Visit(context.argumentList()) : new ArgsNode();
        }

        public UstNode VisitArgumentList([NotNull] ECMAScriptParser.ArgumentListContext context)
        {
            Expression[] args = context.singleExpression().Select(expr => Visit(expr).ToExpressionIfRequired()).ToArray();
            var result = new ArgsNode(args, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitExpressionSequence([NotNull] ECMAScriptParser.ExpressionSequenceContext context)
        {
            List<Expression> expressions = context.singleExpression()
                .Select(expr => Visit(expr).ToExpressionIfRequired())
                .Where(expr => expr != null)
                .ToList();
            var result = new MultichildExpression(expressions, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitSingleExpression([NotNull] ECMAScriptParser.SingleExpressionContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitAssignmentOperator([NotNull] ECMAScriptParser.AssignmentOperatorContext context) { return VisitChildren(context); }

        public UstNode VisitLiteral([NotNull] ECMAScriptParser.LiteralContext context)
        {
            if (context.NullLiteral() != null)
            {
                return new NullLiteral(context.GetTextSpan(), FileNode);
            }
            else if (context.BooleanLiteral() != null)
            {
                bool value;
                bool.TryParse(context.GetText(), out value);
                return new BooleanLiteral(value, context.GetTextSpan(), FileNode);
            }
            else if (context.StringLiteral() != null)
            {
                string s = context.GetText();
                return new StringLiteral(s.Substring(1, s.Length - 2), context.GetTextSpan(), FileNode);
            }
            else if (context.RegularExpressionLiteral() != null)
            {
                return new StringLiteral(context.GetText(), context.GetTextSpan(), FileNode);
            }
            else
            {
                return Visit(context.numericLiteral());
            }
        }

        public UstNode VisitNumericLiteral([NotNull] ECMAScriptParser.NumericLiteralContext context)
        {
            if (context.DecimalLiteral() != null)
            {
                double value;
                double.TryParse(context.GetText(), out value);
                return new FloatLiteral(value, context.GetTextSpan(), FileNode);
            }
            else if (context.HexIntegerLiteral() != null)
            {
                long value;
                context.GetText().TryConvertToInt64(16, out value);
                return new IntLiteral(value, context.GetTextSpan(), FileNode);
            }
            else
            {
                long value;
                context.GetText().TryConvertToInt64(8, out value);
                return new IntLiteral(value, context.GetTextSpan(), FileNode);
            }
        }

        public UstNode VisitIdentifierName([NotNull] ECMAScriptParser.IdentifierNameContext context) { return VisitChildren(context); }

        public UstNode VisitReservedWord([NotNull] ECMAScriptParser.ReservedWordContext context) { return VisitChildren(context); }

        public UstNode VisitKeyword([NotNull] ECMAScriptParser.KeywordContext context) { return VisitChildren(context); }

        public UstNode VisitFutureReservedWord([NotNull] ECMAScriptParser.FutureReservedWordContext context) { return VisitChildren(context); }

        public UstNode VisitGetter([NotNull] ECMAScriptParser.GetterContext context) { return VisitChildren(context); }

        public UstNode VisitSetter([NotNull] ECMAScriptParser.SetterContext context) { return VisitChildren(context); }

        public UstNode VisitEos([NotNull] ECMAScriptParser.EosContext context)
        {
            return null;
        }

        public UstNode VisitEof([NotNull] ECMAScriptParser.EofContext context) { return VisitChildren(context); }
    }
}
