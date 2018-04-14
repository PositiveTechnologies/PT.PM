using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.JavaScriptParseTreeUst
{
    public partial class JavaScriptParseTreeConverter : AntlrConverter, IJavaScriptParserVisitor<Ust>
    {
        public override Language Language => JavaScript.Language;

        public Ust VisitProgram([NotNull] JavaScriptParser.ProgramContext context)
        {
            root.Node = Visit(context.sourceElements());
            return root;
        }

        public Ust VisitTernaryExpression([NotNull] JavaScriptParser.TernaryExpressionContext context)
        {
            var condition = (Expression)Visit(context.singleExpression(0));
            var trueExpression = (Expression)Visit(context.singleExpression(1));
            var falseExpression = (Expression)Visit(context.singleExpression(2));
            return new ConditionalExpression(condition, trueExpression, falseExpression, context.GetTextSpan());
        }

        public Ust VisitLogicalAndExpression([NotNull] JavaScriptParser.LogicalAndExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public Ust VisitPreIncrementExpression([NotNull] JavaScriptParser.PreIncrementExpressionContext context) { return VisitChildren(context); }

        public Ust VisitObjectLiteralExpression([NotNull] JavaScriptParser.ObjectLiteralExpressionContext context) { return VisitChildren(context); }

        public Ust VisitInExpression([NotNull] JavaScriptParser.InExpressionContext context) { return VisitChildren(context); }

        public Ust VisitLogicalOrExpression([NotNull] JavaScriptParser.LogicalOrExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public Ust VisitNotExpression([NotNull] JavaScriptParser.NotExpressionContext context) { return VisitChildren(context); }

        public Ust VisitPreDecreaseExpression([NotNull] JavaScriptParser.PreDecreaseExpressionContext context) { return VisitChildren(context); }

        public Ust VisitArgumentsExpression([NotNull] JavaScriptParser.ArgumentsExpressionContext context)
        {
            var target = (Expression)Visit(context.singleExpression());
            var argsNode = (ArgsUst)Visit(context.arguments());
            var result = new InvocationExpression(target, argsNode, context.GetTextSpan());
            return result;
        }

        public Ust VisitSuperExpression([NotNull] JavaScriptParser.SuperExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitThisExpression([NotNull] JavaScriptParser.ThisExpressionContext context)
        {
            return new ThisReferenceToken(context.GetTextSpan());
        }

        /// <returns><see cref="AnonymousMethodExpression"/></returns>
        public Ust VisitFunctionExpression([NotNull] JavaScriptParser.FunctionExpressionContext context)
        {
            var body = (BlockStatement)Visit(context.functionBody());
            var result = new AnonymousMethodExpression(new ParameterDeclaration[0], body, context.GetTextSpan());
            return result;
        }

        public Ust VisitPostDecreaseExpression([NotNull] JavaScriptParser.PostDecreaseExpressionContext context) { return VisitChildren(context); }

        public Ust VisitUnaryMinusExpression([NotNull] JavaScriptParser.UnaryMinusExpressionContext context) { return VisitChildren(context); }

        public Ust VisitAssignmentExpression([NotNull] JavaScriptParser.AssignmentExpressionContext context)
        {
            Expression left = Visit(context.singleExpression(0)).ToExpressionIfRequired();
            Expression right = Visit(context.singleExpression(1)).ToExpressionIfRequired();
            return new AssignmentExpression(left, right, context.GetTextSpan());
        }

        public Ust VisitTypeofExpression([NotNull] JavaScriptParser.TypeofExpressionContext context) { return VisitChildren(context); }

        public Ust VisitInstanceofExpression([NotNull] JavaScriptParser.InstanceofExpressionContext context) { return VisitChildren(context); }

        public Ust VisitUnaryPlusExpression([NotNull] JavaScriptParser.UnaryPlusExpressionContext context) { return VisitChildren(context); }

        public Ust VisitDeleteExpression([NotNull] JavaScriptParser.DeleteExpressionContext context) { return VisitChildren(context); }

        public Ust VisitEqualityExpression([NotNull] JavaScriptParser.EqualityExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public Ust VisitBitXOrExpression([NotNull] JavaScriptParser.BitXOrExpressionContext context) { return VisitChildren(context); }

        public Ust VisitMultiplicativeExpression([NotNull] JavaScriptParser.MultiplicativeExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public Ust VisitBitShiftExpression([NotNull] JavaScriptParser.BitShiftExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public Ust VisitParenthesizedExpression([NotNull] JavaScriptParser.ParenthesizedExpressionContext context)
        {
            return Visit(context.expressionSequence());
        }

        public Ust VisitArrowFunctionExpression([NotNull] JavaScriptParser.ArrowFunctionExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitArrowFunctionParameters([NotNull] JavaScriptParser.ArrowFunctionParametersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitArrowFunctionBody([NotNull] JavaScriptParser.ArrowFunctionBodyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTemplateStringExpression([NotNull] JavaScriptParser.TemplateStringExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPostIncrementExpression([NotNull] JavaScriptParser.PostIncrementExpressionContext context) { return VisitChildren(context); }

        public Ust VisitAdditiveExpression([NotNull] JavaScriptParser.AdditiveExpressionContext context)
        {
            Expression result = (Expression)CreateBinaryOperatorExpression(
                    context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
            return result;
        }

        public Ust VisitRelationalExpression([NotNull] JavaScriptParser.RelationalExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public Ust VisitBitNotExpression([NotNull] JavaScriptParser.BitNotExpressionContext context) { return VisitChildren(context); }

        public Ust VisitNewExpression([NotNull] JavaScriptParser.NewExpressionContext context) { return VisitChildren(context); }

        public Ust VisitLiteralExpression([NotNull] JavaScriptParser.LiteralExpressionContext context)
        {
            return Visit(context.literal());
        }

        public Ust VisitMemberDotExpression([NotNull] JavaScriptParser.MemberDotExpressionContext context)
        {
            var target = (Expression)Visit(context.singleExpression());
            var name = (Expression)Visit(context.identifierName());
            return new MemberReferenceExpression(target, name, context.GetTextSpan());
        }

        public Ust VisitArrayLiteralExpression([NotNull] JavaScriptParser.ArrayLiteralExpressionContext context) { return VisitChildren(context); }

        public Ust VisitMemberIndexExpression([NotNull] JavaScriptParser.MemberIndexExpressionContext context) { return VisitChildren(context); }

        public Ust VisitIdentifierExpression([NotNull] JavaScriptParser.IdentifierExpressionContext context)
        {
            return new IdToken(context.GetText(), context.GetTextSpan());
        }

        public Ust VisitBitAndExpression([NotNull] JavaScriptParser.BitAndExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                   context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public Ust VisitBitOrExpression([NotNull] JavaScriptParser.BitOrExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public Ust VisitAssignmentOperatorExpression([NotNull] JavaScriptParser.AssignmentOperatorExpressionContext context)
        {
            var left = (Expression)Visit(context.singleExpression(0));
            var right = (Expression)Visit(context.singleExpression(1));
            return new AssignmentExpression(left, right, context.GetTextSpan());
        }

        public Ust VisitVoidExpression([NotNull] JavaScriptParser.VoidExpressionContext context) { return VisitChildren(context); }

        public Ust VisitPropertyExpressionAssignment([NotNull] JavaScriptParser.PropertyExpressionAssignmentContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComputedPropertyExpressionAssignment([NotNull] JavaScriptParser.ComputedPropertyExpressionAssignmentContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPropertySetter([NotNull] JavaScriptParser.PropertySetterContext context) { return VisitChildren(context); }

        public Ust VisitPropertyGetter([NotNull] JavaScriptParser.PropertyGetterContext context) { return VisitChildren(context); }

        public Ust VisitMethodProperty([NotNull] JavaScriptParser.MethodPropertyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPropertyShorthand([NotNull] JavaScriptParser.PropertyShorthandContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDoStatement([NotNull] JavaScriptParser.DoStatementContext context) { return VisitChildren(context); }

        public Ust VisitForVarStatement([NotNull] JavaScriptParser.ForVarStatementContext context) { return VisitChildren(context); }

        public Ust VisitForVarInStatement([NotNull] JavaScriptParser.ForVarInStatementContext context) { return VisitChildren(context); }

        public Ust VisitWhileStatement([NotNull] JavaScriptParser.WhileStatementContext context) { return VisitChildren(context); }

        public Ust VisitForStatement([NotNull] JavaScriptParser.ForStatementContext context) { return VisitChildren(context); }

        public Ust VisitForInStatement([NotNull] JavaScriptParser.ForInStatementContext context) { return VisitChildren(context); }

        /// <returns><see cref="BlockStatement"/></returns>
        public Ust VisitSourceElements([NotNull] JavaScriptParser.SourceElementsContext context)
        {
            var statements = context.sourceElement().Select(element => Visit(element).ToStatementIfRequired()).ToList();
            return new BlockStatement(statements, context.GetTextSpan());
        }

        public Ust VisitSourceElement([NotNull] JavaScriptParser.SourceElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStatement([NotNull] JavaScriptParser.StatementContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public Ust VisitBlock([NotNull] JavaScriptParser.BlockContext context)
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
        public Ust VisitStatementList([NotNull] JavaScriptParser.StatementListContext context)
        {
            return new BlockStatement(
                context.statement().Select(s => Visit(s).ToStatementIfRequired()).ToArray(),
                context.GetTextSpan());
        }

        public Ust VisitVariableStatement([NotNull] JavaScriptParser.VariableStatementContext context) { return VisitChildren(context); }

        public Ust VisitVariableDeclarationList([NotNull] JavaScriptParser.VariableDeclarationListContext context) { return VisitChildren(context); }

        public Ust VisitVariableDeclaration([NotNull] JavaScriptParser.VariableDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEmptyStatement([NotNull] JavaScriptParser.EmptyStatementContext context)
        {
            return new EmptyStatement(context.GetTextSpan());
        }

        public Ust VisitExpressionStatement([NotNull] JavaScriptParser.ExpressionStatementContext context) { return VisitChildren(context); }

        public Ust VisitIfStatement([NotNull] JavaScriptParser.IfStatementContext context) { return VisitChildren(context); }

        public Ust VisitIterationStatement([NotNull] JavaScriptParser.IterationStatementContext context) { return VisitChildren(context); }

        public Ust VisitVarModifier([NotNull] JavaScriptParser.VarModifierContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitContinueStatement([NotNull] JavaScriptParser.ContinueStatementContext context) { return VisitChildren(context); }

        public Ust VisitBreakStatement([NotNull] JavaScriptParser.BreakStatementContext context) { return VisitChildren(context); }

        /// <returns><see cref="ReturnStatement"/></returns>
        public Ust VisitReturnStatement([NotNull] JavaScriptParser.ReturnStatementContext context)
        {
            Expression expression = null;
            if (context.expressionSequence() != null)
            {
                expression = (Expression)Visit(context.expressionSequence());
            }
            return new ReturnStatement(expression, context.GetTextSpan());
        }

        public Ust VisitWithStatement([NotNull] JavaScriptParser.WithStatementContext context) { return VisitChildren(context); }

        public Ust VisitSwitchStatement([NotNull] JavaScriptParser.SwitchStatementContext context) { return VisitChildren(context); }

        public Ust VisitCaseBlock([NotNull] JavaScriptParser.CaseBlockContext context) { return VisitChildren(context); }

        public Ust VisitCaseClauses([NotNull] JavaScriptParser.CaseClausesContext context) { return VisitChildren(context); }

        public Ust VisitCaseClause([NotNull] JavaScriptParser.CaseClauseContext context) { return VisitChildren(context); }

        public Ust VisitDefaultClause([NotNull] JavaScriptParser.DefaultClauseContext context) { return VisitChildren(context); }

        public Ust VisitLabelledStatement([NotNull] JavaScriptParser.LabelledStatementContext context) { return VisitChildren(context); }

        public Ust VisitThrowStatement([NotNull] JavaScriptParser.ThrowStatementContext context) { return VisitChildren(context); }

        /// <returns><see cref="TryCatchStatement"/></returns>
        public Ust VisitTryStatement([NotNull] JavaScriptParser.TryStatementContext context)
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
                Root = root
            };
            return result;
        }

        /// <returns><see cref="CatchClause"/></returns>
        public Ust VisitCatchProduction([NotNull] JavaScriptParser.CatchProductionContext context)
        {
            var identifier = context.Identifier();
            var result = new CatchClause
            {
                VarName = new IdToken(identifier.GetText(), identifier.GetTextSpan()),
                Body = (BlockStatement)Visit(context.block())
            };
            return result;
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public Ust VisitFinallyProduction([NotNull] JavaScriptParser.FinallyProductionContext context)
        {
            return Visit(context.block());
        }

        public Ust VisitDebuggerStatement([NotNull] JavaScriptParser.DebuggerStatementContext context) { return VisitChildren(context); }

        public Ust VisitFunctionDeclaration([NotNull] JavaScriptParser.FunctionDeclarationContext context) { return VisitChildren(context); }

        public Ust VisitFormalParameterList([NotNull] JavaScriptParser.FormalParameterListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFormalParameterArg([NotNull] JavaScriptParser.FormalParameterArgContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLastFormalParameterArg([NotNull] JavaScriptParser.LastFormalParameterArgContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public Ust VisitFunctionBody([NotNull] JavaScriptParser.FunctionBodyContext context)
        {
            BlockStatement result;
            if (context.sourceElements() == null)
            {
                result = new BlockStatement(new Statement[] { new EmptyStatement(context.GetTextSpan()) });
            }
            else
            {
                result = (BlockStatement)Visit(context.sourceElements());
            }
            return result;
        }

        public Ust VisitArrayLiteral([NotNull] JavaScriptParser.ArrayLiteralContext context) { return VisitChildren(context); }

        public Ust VisitElementList([NotNull] JavaScriptParser.ElementListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLastElement([NotNull] JavaScriptParser.LastElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitObjectLiteral([NotNull] JavaScriptParser.ObjectLiteralContext context) { return VisitChildren(context); }

        public Ust VisitPropertyAssignment([NotNull] JavaScriptParser.PropertyAssignmentContext context) { return VisitChildren(context); }

        public Ust VisitPropertyName([NotNull] JavaScriptParser.PropertyNameContext context) { return VisitChildren(context); }

        public Ust VisitArguments([NotNull] JavaScriptParser.ArgumentsContext context)
        {
            Expression[] args = context.singleExpression().Select(expr => Visit(expr).ToExpressionIfRequired()).ToArray();
            var result = new ArgsUst(args, context.GetTextSpan());
            return result;
        }

        public Ust VisitLastArgument([NotNull] JavaScriptParser.LastArgumentContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public Ust VisitExpressionSequence([NotNull] JavaScriptParser.ExpressionSequenceContext context)
        {
            List<Expression> expressions = context.singleExpression()
                .Select(expr => Visit(expr).ToExpressionIfRequired())
                .Where(expr => expr != null)
                .ToList();
            var result = new MultichildExpression(expressions, context.GetTextSpan());
            return result;
        }

        public Ust VisitSingleExpression([NotNull] JavaScriptParser.SingleExpressionContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitAssignmentOperator([NotNull] JavaScriptParser.AssignmentOperatorContext context) { return VisitChildren(context); }

        public Ust VisitLiteral([NotNull] JavaScriptParser.LiteralContext context)
        {
            if (context.NullLiteral() != null)
            {
                return new NullLiteral(context.GetTextSpan());
            }
            else if (context.BooleanLiteral() != null)
            {
                bool value;
                bool.TryParse(context.GetText(), out value);
                return new BooleanLiteral(value, context.GetTextSpan());
            }
            else if (context.StringLiteral() != null)
            {
                string s = context.GetText();
                return new StringLiteral(s.Substring(1, s.Length - 2), context.GetTextSpan());
            }
            else if (context.RegularExpressionLiteral() != null)
            {
                return new StringLiteral(context.GetText(), context.GetTextSpan());
            }
            else
            {
                return Visit(context.numericLiteral());
            }
        }

        public Ust VisitNumericLiteral([NotNull] JavaScriptParser.NumericLiteralContext context)
        {
            if (context.DecimalLiteral() != null)
            {
                double value;
                double.TryParse(context.GetText(), out value);
                return new FloatLiteral(value, context.GetTextSpan());
            }
            else if (context.HexIntegerLiteral() != null)
            {
                long value;
                context.GetText().TryConvertToInt64(16, out value);
                return new IntLiteral(value, context.GetTextSpan());
            }
            else if (context.OctalIntegerLiteral2() != null)
            {
                long value;
                context.GetText().Substring(2).TryConvertToInt64(8, out value);
                return new IntLiteral(value, context.GetTextSpan());
            }
            else if (context.BinaryIntegerLiteral() != null)
            {
                long value;
                context.GetText().Substring(2).TryConvertToInt64(2, out value);
                return new IntLiteral(value, context.GetTextSpan());
            }
            else
            {
                long value;
                context.GetText().TryConvertToInt64(8, out value);
                return new IntLiteral(value, context.GetTextSpan());
            }
        }

        public Ust VisitIdentifierName([NotNull] JavaScriptParser.IdentifierNameContext context) { return VisitChildren(context); }

        public Ust VisitReservedWord([NotNull] JavaScriptParser.ReservedWordContext context) { return VisitChildren(context); }

        public Ust VisitKeyword([NotNull] JavaScriptParser.KeywordContext context) { return VisitChildren(context); }

        public Ust VisitGetter([NotNull] JavaScriptParser.GetterContext context) { return VisitChildren(context); }

        public Ust VisitSetter([NotNull] JavaScriptParser.SetterContext context) { return VisitChildren(context); }

        public Ust VisitEos([NotNull] JavaScriptParser.EosContext context)
        {
            return null;
        }

        public Ust VisitClassExpression([NotNull] JavaScriptParser.ClassExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitClassDeclaration([NotNull] JavaScriptParser.ClassDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitClassTail([NotNull] JavaScriptParser.ClassTailContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitClassElement([NotNull] JavaScriptParser.ClassElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMethodDefinition([NotNull] JavaScriptParser.MethodDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGeneratorMethod([NotNull] JavaScriptParser.GeneratorMethodContext context)
        {
            return VisitChildren(context);
        }
    }
}
