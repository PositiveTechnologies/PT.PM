using System;
using System.Collections.Generic;
using System.Threading;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
using static PT.PM.PlSqlParseTreeUst.PlSqlParser;

namespace PT.PM.PlSqlParseTreeUst
{
    public class PlSqlAntlrListenerConverter : AntlrListenerConverter
    {
        private static readonly int[] binaryOperatorPlSqlTypes = {ASTERISK, SOLIDUS, PLUS_SIGN, MINUS_SIGN, BAR};

        public PlSqlAntlrListenerConverter(TextFile sourceFile, AntlrParserConverter antlrParser)
            : base(sourceFile, antlrParser)
        {
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            Ust result = null;

            try
            {
                switch (context.RuleIndex)
                {
                    case RULE_create_procedure_body:
                        result = ConvertCreateProcedureBody(context);
                        break;
                    case RULE_parameter:
                        result = ConvertParameter(context);
                        break;
                    case RULE_body:
                        result = ConvertBody(context);
                        break;
                    case RULE_variable_declaration:
                        result = ConvertVariableDeclaration(context);
                        break;
                    case RULE_exception_handler:
                        result = ConvertExceptionHandler(context);
                        break;
                    case RULE_seq_of_statements:
                        result = ConvertSeqOfStatements(context);
                        break;
                    case RULE_seq_of_declare_specs:
                        result = ConvertSeqOfDeclareSpecs(context);
                        break;
                    case RULE_grant_statement:
                        result = ConvertGrantStatement(context);
                        break;
                    case RULE_statement:
                        result = ConvertStatement(context);
                        break;
                    case RULE_assignment_statement:
                        result = ConvertAssignmentStatement(context);
                        break;
                    case RULE_continue_statement:
                        result = ConvertContinueStatement(context);
                        break;
                    case RULE_exit_statement:
                        result = ConvertExitStatement(context);
                        break;
                    case RULE_return_statement:
                        result = ConvertReturnStatement(context);
                        break;
                    case RULE_open_statement:
                    case RULE_open_for_statement:
                    case RULE_fetch_statement:
                    case RULE_close_statement:
                        result = ConvertCursorStatement(context);
                        break;
                    case RULE_cursor_name:
                    case RULE_variable_name:
                        result = ConvertCursorOrVariableName(context);
                        break;
                    case RULE_function_call:
                    case RULE_procedure_call:
                        result = ConvertFunctionOrProcedureCall(context);
                        break;
                    case RULE_routine_name:
                        result = ConvertRoutineName(context);
                        break;
                    case RULE_general_element_part:
                        result = ConvertGeneralElementPart(context);
                        break;
                    case RULE_function_argument:
                        result = ConvertFunctionArgument(context);
                        break;
                    case RULE_execute_immediate:
                        result = ConvertExecuteImmediate(context);
                        break;
                    case RULE_relational_expression:
                        result = ConvertRelationalExpression(context);
                        break;
                    case RULE_relational_operator:
                        result = ConvertRelationalOperator(context);
                        break;
                    case RULE_concatenation:
                        result = ConvertConcatenationExpression(context);
                        break;
                    default:
                        result = ConvertChildren();
                        break;
                }
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ConversionException(root.SourceFile, ex));
                Console.WriteLine(ex);
            }
            finally
            {
                RemoveAndAdd(result);
            }
        }

        private MethodDeclaration ConvertCreateProcedureBody(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();
            int nameIndex = TryGetChildOfType(out IdToken name);

            var parameters = new List<ParameterDeclaration>();
            var statements = new List<Statement>();

            for (int i = nameIndex + 1; i < children.Count; i++)
            {
                Ust child = children[i];
                if (child is ParameterDeclaration param)
                {
                    parameters.Add(param);
                }
                else if (child is Statement statement)
                {
                    statements.Add(statement);
                }
            }

            return new MethodDeclaration(name, parameters, new BlockStatement(statements), context.GetTextSpan());
        }

        private ParameterDeclaration ConvertParameter(ParserRuleContext context)
        {
            return new ParameterDeclaration(null, null, GetChild(0).AsIdToken(), context.GetTextSpan());
        }

        private Statement ConvertBody(ParserRuleContext context)
        {
            const int exceptionKeywordIndex = 2;
            List<Ust> children = GetChildren();
            BlockStatement block = (BlockStatement) children[1];

            if (CheckChild<Keyword>(EXCEPTION, exceptionKeywordIndex))
            {
                var tryCatchStatement = new TryCatchStatement(block, context.GetTextSpan());

                int endInd = GetChildFromEnd(0) is Keyword ? children.Count - 1 : children.Count - 2;
                var catchClauses = new List<CatchClause>(endInd - exceptionKeywordIndex - 1);

                for (int i = exceptionKeywordIndex + 1; i < endInd; i++)
                {
                    catchClauses.Add((CatchClause) children[i]);
                }

                tryCatchStatement.CatchClauses = catchClauses;

                return tryCatchStatement;
            }

            return block;
        }

        private VariableDeclarationExpression ConvertVariableDeclaration(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();
            var left = children[0].AsIdToken();
            int rightInd = children[children.Count - 1] is Punctuator ? children.Count - 2 : children.Count - 1;
            var right = children[rightInd].AsExpression();

            return new VariableDeclarationExpression(
                null, new List<AssignmentExpression> {new AssignmentExpression(left, right, context.GetTextSpan())});
        }

        private CatchClause ConvertExceptionHandler(ParserRuleContext context)
        {
            var body = (BlockStatement)GetChildFromEnd(0);
            if (body.Statements.Count == 1 &&
                body.Statements[0] is ExpressionStatement expressionStatement &&
                expressionStatement.Expression is NullLiteral)
            {
                body = new BlockStatement(new Statement[0], body.Statements[0].TextSpan);
            }
            return new CatchClause(null, null, body, context.GetTextSpan());
        }

        private BlockStatement ConvertSeqOfStatements(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();
            var statements = new List<Statement>(children.Count);

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Statement statement)
                {
                    statements.Add(statement);
                }
            }

            return new BlockStatement(statements, context.GetTextSpan());
        }

        private BlockStatement ConvertSeqOfDeclareSpecs(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();
            var statements = new List<Statement>(children.Count);

            foreach (Ust child in children)
            {
                statements.Add(child.AsStatement());
            }

            return new BlockStatement(statements, context.GetTextSpan());
        }

        private ExpressionStatement ConvertGrantStatement(ParserRuleContext context)
        {
             List<Ust> children = GetChildren();
             IdToken name;
             int argInd;

             if (CheckChild<Keyword>(ALL, 1))
             {
                 name = new IdToken("grant_all", children[0].TextSpan.Union(children[1].TextSpan));
                 argInd = 2;
             }
             else
             {
                 name = children[0].AsIdToken();
                 argInd = 1;
             }

             var args = new List<Expression>(children.Count - argInd);
             for (int i = argInd; i < children.Count; i++)
             {
                 args.Add(children[i].AsExpression());
             }

             return new ExpressionStatement(new InvocationExpression(name, new ArgsUst(args), context.GetTextSpan()));
        }

        private Statement ConvertStatement(ParserRuleContext context)
        {
            return GetChild(0).AsStatement();
        }

        private AssignmentExpression ConvertAssignmentStatement(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();
            var left = children[0].AsExpression();
            var right = children[2].AsExpression();
            return new AssignmentExpression(left, right, context.GetTextSpan());
        }

        private ContinueStatement ConvertContinueStatement(ParserRuleContext context)
        {
            return new ContinueStatement(context.GetTextSpan());
        }

        private BreakStatement ConvertExitStatement(ParserRuleContext context)
        {
            return new BreakStatement(context.GetTextSpan());
        }

        private ReturnStatement ConvertReturnStatement(ParserRuleContext context)
        {
            return new ReturnStatement(GetChild(0).AsExpression(), context.GetTextSpan());
        }

        private ExpressionStatement ConvertCursorStatement(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();

            var invocation = new InvocationExpression(context.GetTextSpan())
            {
                Target = children[0].AsIdToken(),
                Arguments = new ArgsUst(children[1].AsExpression())
            };

            for (int i = 2; i < children.Count; i++)
            {
                Ust child = children[i];

                if (!(child is ITerminal) || child is Token)
                {
                    invocation.Arguments.Collection.Add(child.AsExpression());
                }
            }

            return new ExpressionStatement(invocation);
        }

        private ArgumentExpression ConvertCursorOrVariableName(ParserRuleContext context)
        {
            var argModifier = new InOutModifierLiteral(InOutModifier.InOut, TextSpan.Zero);
            TextSpan contextTextSpan = context.GetTextSpan();
            var arg = new IdToken(GetChildFromEnd(0).Substring, contextTextSpan);
            return new ArgumentExpression(argModifier, arg, contextTextSpan);
        }

        private InvocationExpression ConvertExecuteImmediate(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();
            var invocation = new InvocationExpression(context.GetTextSpan())
            {
                Target = children[0].AsIdToken(),
                Arguments = new ArgsUst(children[2].AsExpression())
            };

            return invocation;
        }

        private InvocationExpression ConvertFunctionOrProcedureCall(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();
            int targetIndex = CheckChild<Keyword>(CALL, 0) ? 1 : 0;
            ArgsUst args = children[children.Count - 1] is ArgsUst argsUst ? argsUst : new ArgsUst();
            return new InvocationExpression(children[targetIndex].AsExpression(), args, context.GetTextSpan());
        }

        private Expression ConvertRoutineName(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();
            Expression result = children[0].AsIdToken();

            int index = 1;
            while (CheckChild<IOperatorOrPunctuator>(PERIOD, index))
            {
                Expression name = children[index + 1].AsIdToken();
                result = new MemberReferenceExpression(result, name, result.TextSpan.Union(name.TextSpan));
                index += 2;
            }

            return result;
        }

        private Expression ConvertGeneralElementPart(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();

            int index = CheckChild<Keyword>(INTRODUCER, 0) ? 2 : 0;

            Expression result = children[index].AsExpression();
            index++;

            while (CheckChild<IOperatorOrPunctuator>(PERIOD, index))
            {
                Expression name = children[index + 1].AsIdToken();
                result = new MemberReferenceExpression(result, name, result.TextSpan.Union(name.TextSpan));
                index += 2;
            }

            if (!(children[children.Count - 1] is ArgsUst args))
            {
                return result;
            }

            return new InvocationExpression(result, args, context.GetTextSpan());
        }

        private ArgsUst ConvertFunctionArgument(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();

            var args = new List<Expression>(children.Count);

            foreach (Ust child in children)
            {
                if (!(child is Punctuator))
                {
                    args.Add(child.AsExpression());
                }
            }

            return new ArgsUst(args, context.GetTextSpan());
        }

        private Expression ConvertRelationalExpression(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();

            if (children.Count == 1)
            {
                return children[0].AsExpression();
            }

            var right = children[children.Count - 1].AsExpression();
            var op = (BinaryOperatorLiteral) children[children.Count - 2];
            var left = GetLeftChildFromLeftRecursiveRule(context).AsExpression();

            return new BinaryOperatorExpression(left, op, right, context.GetTextSpan());
        }

        private BinaryOperatorLiteral ConvertRelationalOperator(ParserRuleContext context)
        {
            string opText = GetChild(0).ToString(false);

            BinaryOperator op;
            switch (opText)
            {
                case "=":
                    op = BinaryOperator.Equal;
                    break;
                case ">":
                    op = BinaryOperator.Greater;
                    break;
                case "<":
                    op = BinaryOperator.Less;
                    break;
                case ">=":
                    op = BinaryOperator.GreaterOrEqual;
                    break;
                case "<=":
                    op = BinaryOperator.LessOrEqual;
                    break;
                default:
                    op = BinaryOperator.NotEqual;
                    break;
            }

            return new BinaryOperatorLiteral(op, context.GetTextSpan());
        }

        private Expression ConvertConcatenationExpression(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();

            var operatorIndex = IsParseTreeExisting ? 1 : 0;

            if (!CheckChild<IOperatorOrPunctuator>(binaryOperatorPlSqlTypes, operatorIndex))
            {
                return ConvertChildren().AsExpression();
            }

            var right = children[children.Count - 1].AsExpression();

            Ust opUst = children[operatorIndex];
            BinaryOperatorLiteral op;

            if (CheckChild<IOperatorOrPunctuator>(BAR, operatorIndex))
            {
                op = new BinaryOperatorLiteral(BinaryOperator.LogicalOr,
                    opUst.TextSpan.Union(children[operatorIndex + 1].TextSpan));
            }
            else if (opUst is BinaryOperatorLiteral binaryOperatorLiteral)
            {
                op = binaryOperatorLiteral;
            }
            else
            {
                op = new BinaryOperatorLiteral(BinaryOperatorLiteral.TextBinaryOperator[opUst.Substring], opUst.TextSpan);
            }

            var left = GetLeftChildFromLeftRecursiveRule(context).AsExpression();

            return new BinaryOperatorExpression(left, op, right, context.GetTextSpan());
        }
    }
}
