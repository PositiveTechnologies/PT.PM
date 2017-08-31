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
using PT.PM.JavaScriptParseTreeUst.Parser;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.JavaScriptParseTreeUst
{
    public partial class JavaScriptParseTreeConverter : AntlrConverter, IJavaScriptParserVisitor<UstNode>
    {
        public override Language Language => Language.JavaScript;

        public UstNode VisitProgram([NotNull] JavaScriptParser.ProgramContext context)
        {
            root.Node = Visit(context.sourceElements());
            return root;
        }

        public UstNode VisitTernaryExpression([NotNull] JavaScriptParser.TernaryExpressionContext context)
        {
            var condition = (Expression)Visit(context.singleExpression(0));
            var trueExpression = (Expression)Visit(context.singleExpression(1));
            var falseExpression = (Expression)Visit(context.singleExpression(2));
            return new ConditionalExpression(condition, trueExpression, falseExpression, context.GetTextSpan(), root);
        }

        public UstNode VisitLogicalAndExpression([NotNull] JavaScriptParser.LogicalAndExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitPreIncrementExpression([NotNull] JavaScriptParser.PreIncrementExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitObjectLiteralExpression([NotNull] JavaScriptParser.ObjectLiteralExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitInExpression([NotNull] JavaScriptParser.InExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitLogicalOrExpression([NotNull] JavaScriptParser.LogicalOrExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitNotExpression([NotNull] JavaScriptParser.NotExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitPreDecreaseExpression([NotNull] JavaScriptParser.PreDecreaseExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitArgumentsExpression([NotNull] JavaScriptParser.ArgumentsExpressionContext context)
        {
            var target = (Expression)Visit(context.singleExpression());
            var argsNode = (ArgsNode)Visit(context.arguments());
            var result = new InvocationExpression(target, argsNode, context.GetTextSpan(), root);
            return result;
        }

        public UstNode VisitSuperExpression([NotNull] JavaScriptParser.SuperExpressionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitThisExpression([NotNull] JavaScriptParser.ThisExpressionContext context)
        {
            return new ThisReferenceToken(context.GetTextSpan(), root);
        }

        /// <returns><see cref="AnonymousMethodExpression"/></returns>
        public UstNode VisitFunctionExpression([NotNull] JavaScriptParser.FunctionExpressionContext context)
        {
            var body = (BlockStatement)Visit(context.functionBody());
            var result = new AnonymousMethodExpression(new ParameterDeclaration[0], body, context.GetTextSpan(), root);
            return result;
        }

        public UstNode VisitPostDecreaseExpression([NotNull] JavaScriptParser.PostDecreaseExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitUnaryMinusExpression([NotNull] JavaScriptParser.UnaryMinusExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitAssignmentExpression([NotNull] JavaScriptParser.AssignmentExpressionContext context)
        {
            Expression left = Visit(context.singleExpression(0)).ToExpressionIfRequired();
            Expression right = Visit(context.singleExpression(1)).ToExpressionIfRequired();
            return new AssignmentExpression(left, right, context.GetTextSpan(), root);
        }

        public UstNode VisitTypeofExpression([NotNull] JavaScriptParser.TypeofExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitInstanceofExpression([NotNull] JavaScriptParser.InstanceofExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitUnaryPlusExpression([NotNull] JavaScriptParser.UnaryPlusExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitDeleteExpression([NotNull] JavaScriptParser.DeleteExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitEqualityExpression([NotNull] JavaScriptParser.EqualityExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitBitXOrExpression([NotNull] JavaScriptParser.BitXOrExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitMultiplicativeExpression([NotNull] JavaScriptParser.MultiplicativeExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitBitShiftExpression([NotNull] JavaScriptParser.BitShiftExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitParenthesizedExpression([NotNull] JavaScriptParser.ParenthesizedExpressionContext context)
        {
            return Visit(context.expressionSequence());
        }

        public UstNode VisitArrowFunctionExpression([NotNull] JavaScriptParser.ArrowFunctionExpressionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitArrowFunctionParameters([NotNull] JavaScriptParser.ArrowFunctionParametersContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitArrowFunctionBody([NotNull] JavaScriptParser.ArrowFunctionBodyContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTemplateStringExpression([NotNull] JavaScriptParser.TemplateStringExpressionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitPostIncrementExpression([NotNull] JavaScriptParser.PostIncrementExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitAdditiveExpression([NotNull] JavaScriptParser.AdditiveExpressionContext context)
        {
            Expression result = (Expression)CreateBinaryOperatorExpression(
                    context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
            return result;
        }

        public UstNode VisitRelationalExpression([NotNull] JavaScriptParser.RelationalExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitBitNotExpression([NotNull] JavaScriptParser.BitNotExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitNewExpression([NotNull] JavaScriptParser.NewExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitLiteralExpression([NotNull] JavaScriptParser.LiteralExpressionContext context)
        {
            return Visit(context.literal());
        }

        public UstNode VisitMemberDotExpression([NotNull] JavaScriptParser.MemberDotExpressionContext context)
        {
            var target = (Expression)Visit(context.singleExpression());
            var name = (Expression)Visit(context.identifierName());
            return new MemberReferenceExpression(target, name, context.GetTextSpan(), root);
        }

        public UstNode VisitArrayLiteralExpression([NotNull] JavaScriptParser.ArrayLiteralExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitMemberIndexExpression([NotNull] JavaScriptParser.MemberIndexExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitIdentifierExpression([NotNull] JavaScriptParser.IdentifierExpressionContext context)
        {
            return new IdToken(context.GetText(), context.GetTextSpan(), root);
        }

        public UstNode VisitBitAndExpression([NotNull] JavaScriptParser.BitAndExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                   context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitBitOrExpression([NotNull] JavaScriptParser.BitOrExpressionContext context)
        {
            return CreateBinaryOperatorExpression(
                context.singleExpression(0), context.GetChild<ITerminalNode>(0), context.singleExpression(1));
        }

        public UstNode VisitAssignmentOperatorExpression([NotNull] JavaScriptParser.AssignmentOperatorExpressionContext context)
        {
            var left = (Expression)Visit(context.singleExpression(0));
            var right = (Expression)Visit(context.singleExpression(1));
            return new AssignmentExpression(left, right, context.GetTextSpan(), root);
        }

        public UstNode VisitVoidExpression([NotNull] JavaScriptParser.VoidExpressionContext context) { return VisitChildren(context); }

        public UstNode VisitPropertyExpressionAssignment([NotNull] JavaScriptParser.PropertyExpressionAssignmentContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitComputedPropertyExpressionAssignment([NotNull] JavaScriptParser.ComputedPropertyExpressionAssignmentContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitPropertySetter([NotNull] JavaScriptParser.PropertySetterContext context) { return VisitChildren(context); }

        public UstNode VisitPropertyGetter([NotNull] JavaScriptParser.PropertyGetterContext context) { return VisitChildren(context); }

        public UstNode VisitMethodProperty([NotNull] JavaScriptParser.MethodPropertyContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitPropertyShorthand([NotNull] JavaScriptParser.PropertyShorthandContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDoStatement([NotNull] JavaScriptParser.DoStatementContext context) { return VisitChildren(context); }

        public UstNode VisitForVarStatement([NotNull] JavaScriptParser.ForVarStatementContext context) { return VisitChildren(context); }

        public UstNode VisitForVarInStatement([NotNull] JavaScriptParser.ForVarInStatementContext context) { return VisitChildren(context); }

        public UstNode VisitWhileStatement([NotNull] JavaScriptParser.WhileStatementContext context) { return VisitChildren(context); }

        public UstNode VisitForStatement([NotNull] JavaScriptParser.ForStatementContext context) { return VisitChildren(context); }

        public UstNode VisitForInStatement([NotNull] JavaScriptParser.ForInStatementContext context) { return VisitChildren(context); }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitSourceElements([NotNull] JavaScriptParser.SourceElementsContext context)
        {
            var statements = context.sourceElement().Select(element => Visit(element).ToStatementIfRequired()).ToList();
            return new BlockStatement(statements, context.GetTextSpan(), root);
        }

        public UstNode VisitSourceElement([NotNull] JavaScriptParser.SourceElementContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitStatement([NotNull] JavaScriptParser.StatementContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitBlock([NotNull] JavaScriptParser.BlockContext context)
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
        public UstNode VisitStatementList([NotNull] JavaScriptParser.StatementListContext context)
        {
            return new BlockStatement(
                context.statement().Select(s => Visit(s).ToStatementIfRequired()).ToArray(),
                context.GetTextSpan(), root);
        }

        public UstNode VisitVariableStatement([NotNull] JavaScriptParser.VariableStatementContext context) { return VisitChildren(context); }

        public UstNode VisitVariableDeclarationList([NotNull] JavaScriptParser.VariableDeclarationListContext context) { return VisitChildren(context); }

        public UstNode VisitVariableDeclaration([NotNull] JavaScriptParser.VariableDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitEmptyStatement([NotNull] JavaScriptParser.EmptyStatementContext context)
        {
            return new EmptyStatement(context.GetTextSpan(), root);
        }

        public UstNode VisitExpressionStatement([NotNull] JavaScriptParser.ExpressionStatementContext context) { return VisitChildren(context); }

        public UstNode VisitIfStatement([NotNull] JavaScriptParser.IfStatementContext context) { return VisitChildren(context); }

        public UstNode VisitIterationStatement([NotNull] JavaScriptParser.IterationStatementContext context) { return VisitChildren(context); }

        public UstNode VisitVarModifier([NotNull] JavaScriptParser.VarModifierContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitContinueStatement([NotNull] JavaScriptParser.ContinueStatementContext context) { return VisitChildren(context); }

        public UstNode VisitBreakStatement([NotNull] JavaScriptParser.BreakStatementContext context) { return VisitChildren(context); }

        /// <returns><see cref="ReturnStatement"/></returns>
        public UstNode VisitReturnStatement([NotNull] JavaScriptParser.ReturnStatementContext context)
        {
            Expression expression = null;
            if (context.expressionSequence() != null)
            {
                expression = (Expression)Visit(context.expressionSequence());
            }
            return new ReturnStatement(expression, context.GetTextSpan(), root);
        }

        public UstNode VisitWithStatement([NotNull] JavaScriptParser.WithStatementContext context) { return VisitChildren(context); }

        public UstNode VisitSwitchStatement([NotNull] JavaScriptParser.SwitchStatementContext context) { return VisitChildren(context); }

        public UstNode VisitCaseBlock([NotNull] JavaScriptParser.CaseBlockContext context) { return VisitChildren(context); }

        public UstNode VisitCaseClauses([NotNull] JavaScriptParser.CaseClausesContext context) { return VisitChildren(context); }

        public UstNode VisitCaseClause([NotNull] JavaScriptParser.CaseClauseContext context) { return VisitChildren(context); }

        public UstNode VisitDefaultClause([NotNull] JavaScriptParser.DefaultClauseContext context) { return VisitChildren(context); }

        public UstNode VisitLabelledStatement([NotNull] JavaScriptParser.LabelledStatementContext context) { return VisitChildren(context); }

        public UstNode VisitThrowStatement([NotNull] JavaScriptParser.ThrowStatementContext context) { return VisitChildren(context); }

        /// <returns><see cref="TryCatchStatement"/></returns>
        public UstNode VisitTryStatement([NotNull] JavaScriptParser.TryStatementContext context)
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
        public UstNode VisitCatchProduction([NotNull] JavaScriptParser.CatchProductionContext context)
        {
            var identifier = context.Identifier();
            var result = new CatchClause
            {
                VarName = new IdToken(identifier.GetText(), identifier.GetTextSpan(), root),
                Body = (BlockStatement)Visit(context.block())
            };
            return result;
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitFinallyProduction([NotNull] JavaScriptParser.FinallyProductionContext context)
        {
            return Visit(context.block());
        }

        public UstNode VisitDebuggerStatement([NotNull] JavaScriptParser.DebuggerStatementContext context) { return VisitChildren(context); }

        public UstNode VisitFunctionDeclaration([NotNull] JavaScriptParser.FunctionDeclarationContext context) { return VisitChildren(context); }

        public UstNode VisitFormalParameterList([NotNull] JavaScriptParser.FormalParameterListContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFormalParameterArg([NotNull] JavaScriptParser.FormalParameterArgContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitLastFormalParameterArg([NotNull] JavaScriptParser.LastFormalParameterArgContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitFunctionBody([NotNull] JavaScriptParser.FunctionBodyContext context)
        {
            BlockStatement result;
            if (context.sourceElements() == null)
            {
                result = new BlockStatement(new Statement[] { new EmptyStatement(context.GetTextSpan(), root) }, root);
            }
            else
            {
                result = (BlockStatement)Visit(context.sourceElements());
            }
            return result;
        }

        public UstNode VisitArrayLiteral([NotNull] JavaScriptParser.ArrayLiteralContext context) { return VisitChildren(context); }

        public UstNode VisitElementList([NotNull] JavaScriptParser.ElementListContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitLastElement([NotNull] JavaScriptParser.LastElementContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitObjectLiteral([NotNull] JavaScriptParser.ObjectLiteralContext context) { return VisitChildren(context); }

        public UstNode VisitPropertyAssignment([NotNull] JavaScriptParser.PropertyAssignmentContext context) { return VisitChildren(context); }

        public UstNode VisitPropertyName([NotNull] JavaScriptParser.PropertyNameContext context) { return VisitChildren(context); }

        public UstNode VisitArguments([NotNull] JavaScriptParser.ArgumentsContext context)
        {
            Expression[] args = context.singleExpression().Select(expr => Visit(expr).ToExpressionIfRequired()).ToArray();
            var result = new ArgsNode(args, context.GetTextSpan(), root);
            return result;
        }

        public UstNode VisitLastArgument([NotNull] JavaScriptParser.LastArgumentContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitExpressionSequence([NotNull] JavaScriptParser.ExpressionSequenceContext context)
        {
            List<Expression> expressions = context.singleExpression()
                .Select(expr => Visit(expr).ToExpressionIfRequired())
                .Where(expr => expr != null)
                .ToList();
            var result = new MultichildExpression(expressions, context.GetTextSpan(), root);
            return result;
        }

        public UstNode VisitSingleExpression([NotNull] JavaScriptParser.SingleExpressionContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitAssignmentOperator([NotNull] JavaScriptParser.AssignmentOperatorContext context) { return VisitChildren(context); }

        public UstNode VisitLiteral([NotNull] JavaScriptParser.LiteralContext context)
        {
            if (context.NullLiteral() != null)
            {
                return new NullLiteral(context.GetTextSpan(), root);
            }
            else if (context.BooleanLiteral() != null)
            {
                bool value;
                bool.TryParse(context.GetText(), out value);
                return new BooleanLiteral(value, context.GetTextSpan(), root);
            }
            else if (context.StringLiteral() != null)
            {
                string s = context.GetText();
                return new StringLiteral(s.Substring(1, s.Length - 2), context.GetTextSpan(), root);
            }
            else if (context.RegularExpressionLiteral() != null)
            {
                return new StringLiteral(context.GetText(), context.GetTextSpan(), root);
            }
            else
            {
                return Visit(context.numericLiteral());
            }
        }

        public UstNode VisitNumericLiteral([NotNull] JavaScriptParser.NumericLiteralContext context)
        {
            if (context.DecimalLiteral() != null)
            {
                double value;
                double.TryParse(context.GetText(), out value);
                return new FloatLiteral(value, context.GetTextSpan(), root);
            }
            else if (context.HexIntegerLiteral() != null)
            {
                long value;
                context.GetText().TryConvertToInt64(16, out value);
                return new IntLiteral(value, context.GetTextSpan(), root);
            }
            else if (context.OctalIntegerLiteral2() != null)
            {
                long value;
                context.GetText().Substring(2).TryConvertToInt64(8, out value);
                return new IntLiteral(value, context.GetTextSpan(), root);
            }
            else if (context.BinaryIntegerLiteral() != null)
            {
                long value;
                context.GetText().Substring(2).TryConvertToInt64(2, out value);
                return new IntLiteral(value, context.GetTextSpan(), root);
            }
            else
            {
                long value;
                context.GetText().TryConvertToInt64(8, out value);
                return new IntLiteral(value, context.GetTextSpan(), root);
            }
        }

        public UstNode VisitIdentifierName([NotNull] JavaScriptParser.IdentifierNameContext context) { return VisitChildren(context); }

        public UstNode VisitReservedWord([NotNull] JavaScriptParser.ReservedWordContext context) { return VisitChildren(context); }

        public UstNode VisitKeyword([NotNull] JavaScriptParser.KeywordContext context) { return VisitChildren(context); }

        public UstNode VisitGetter([NotNull] JavaScriptParser.GetterContext context) { return VisitChildren(context); }

        public UstNode VisitSetter([NotNull] JavaScriptParser.SetterContext context) { return VisitChildren(context); }

        public UstNode VisitEos([NotNull] JavaScriptParser.EosContext context)
        {
            return null;
        }

        public UstNode VisitClassExpression([NotNull] JavaScriptParser.ClassExpressionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitClassDeclaration([NotNull] JavaScriptParser.ClassDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitClassTail([NotNull] JavaScriptParser.ClassTailContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitClassElement([NotNull] JavaScriptParser.ClassElementContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitMethodDefinition([NotNull] JavaScriptParser.MethodDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitGeneratorMethod([NotNull] JavaScriptParser.GeneratorMethodContext context)
        {
            return VisitChildren(context);
        }
    }
}
