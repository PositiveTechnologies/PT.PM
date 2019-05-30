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
using PT.PM.PlSqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrListenerConverter : AntlrListenerConverter
    {
        public PlSqlAntlrListenerConverter(TextFile sourceFile, AntlrParserConverter antlrParser)
            : base(sourceFile, antlrParser)
        {
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext ctx)
        {
            Ust result = null;

            try
            {
                switch (ctx)
                {
                    case PlSqlParser.Create_procedure_bodyContext context:
                        result = ConvertCreateProcedureBody(context);
                        break;
                    case PlSqlParser.ParameterContext context:
                        result = ConvertParameter(context);
                        break;
                    case PlSqlParser.BodyContext context:
                        result = ConvertBody(context);
                        break;
                    case PlSqlParser.Exception_handlerContext context:
                        result = ConvertExceptionHandler(context);
                        break;
                    case PlSqlParser.Seq_of_statementsContext context:
                        result = ConvertSeqOfStatements(context);
                        break;
                    case PlSqlParser.Seq_of_declare_specsContext context:
                        result = ConvertSeqOfDeclareSpecs(context);
                        break;
                    case PlSqlParser.Grant_statementContext context:
                        result = ConvertGrantStatement(context);
                        break;
                    case PlSqlParser.StatementContext context:
                        result = ConvertStatement(context);
                        break;
                    case PlSqlParser.Assignment_statementContext context:
                        result = ConvertAssignmentStatement(context);
                        break;
                    case PlSqlParser.Continue_statementContext context:
                        result = ConvertContinueStatement(context);
                        break;
                    case PlSqlParser.Exit_statementContext context:
                        result = ConvertExitStatement(context);
                        break;
                    case PlSqlParser.Return_statementContext context:
                        result = ConvertReturnStatement(context);
                        break;
                    case PlSqlParser.Open_statementContext _:
                    case PlSqlParser.Open_for_statementContext _:
                    case PlSqlParser.Fetch_statementContext _:
                    case PlSqlParser.Close_statementContext _:
                        result = ConvertCursorStatement(ctx);
                        break;
                    case PlSqlParser.Cursor_nameContext _:
                    case PlSqlParser.Variable_nameContext _:
                        result = ConvertCursorOrVariableName(ctx);
                        break;
                    case PlSqlParser.Function_callContext _:
                    case PlSqlParser.Procedure_callContext _:
                        result = ConvertFunctionOrProcedureCall(ctx);
                        break;
                    case PlSqlParser.Routine_nameContext context:
                        result = ConvertRoutineName(context);
                        break;
                    case PlSqlParser.General_element_partContext context:
                        result = ConvertGeneralElementPart(context);
                        break;
                    case PlSqlParser.Function_argumentContext context:
                        result = ConvertFunctionArgument(context);
                        break;
                    case PlSqlParser.Execute_immediateContext context:
                        result = ConvertExecuteImmediate(context);
                        break;
                    case PlSqlParser.Relational_expressionContext context:
                        result = ConvertRelationalExpression(context);
                        break;
                    case PlSqlParser.Relational_operatorContext context:
                        result = ConvertRelationalOperator(context);
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

        private MethodDeclaration ConvertCreateProcedureBody(PlSqlParser.Create_procedure_bodyContext context)
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

        private ParameterDeclaration ConvertParameter(PlSqlParser.ParameterContext context)
        {
            return new ParameterDeclaration(null, null, GetChild(0).AsIdToken(), context.GetTextSpan());
        }

        private Statement ConvertBody(PlSqlParser.BodyContext context)
        {
            const int exceptionKeywordIndex = 2;
            List<Ust> children = GetChildren();
            BlockStatement block = (BlockStatement) children[1];

            if (CheckChild<Keyword>(PlSqlLexer.EXCEPTION, exceptionKeywordIndex))
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

        private CatchClause ConvertExceptionHandler(PlSqlParser.Exception_handlerContext context)
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

        private BlockStatement ConvertSeqOfStatements(PlSqlParser.Seq_of_statementsContext context)
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

        private BlockStatement ConvertSeqOfDeclareSpecs(PlSqlParser.Seq_of_declare_specsContext context)
        {
            List<Ust> children = GetChildren();
            var statements = new List<Statement>(children.Count);

            foreach (Ust child in children)
            {
                statements.Add(child.AsStatement());
            }

            return new BlockStatement(statements, context.GetTextSpan());
        }

        private ExpressionStatement ConvertGrantStatement(PlSqlParser.Grant_statementContext context)
        {
             List<Ust> children = GetChildren();
             IdToken name;
             int argInd;

             if (CheckChild<Keyword>(PlSqlLexer.ALL, 1))
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

        private Ust ConvertStatement(PlSqlParser.StatementContext context)
        {
            return GetChild(0).AsStatement();
        }

        private AssignmentExpression ConvertAssignmentStatement(PlSqlParser.Assignment_statementContext context)
        {
            List<Ust> children = GetChildren();
            var left = children[0].AsExpression();
            var right = children[2].AsExpression();
            return new AssignmentExpression(left, right, context.GetTextSpan());
        }

        private ContinueStatement ConvertContinueStatement(PlSqlParser.Continue_statementContext context)
        {
            return new ContinueStatement(context.GetTextSpan());
        }

        private BreakStatement ConvertExitStatement(PlSqlParser.Exit_statementContext context)
        {
            return new BreakStatement(context.GetTextSpan());
        }

        private ReturnStatement ConvertReturnStatement(PlSqlParser.Return_statementContext context)
        {
            return new ReturnStatement(GetChild(0).AsExpression(), context.GetTextSpan());
        }

        private ExpressionStatement ConvertCursorStatement(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();

            var invocation = new InvocationExpression(context.GetTextSpan())
            {
                Target = children[0].AsExpression(),
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

        private InvocationExpression ConvertExecuteImmediate(PlSqlParser.Execute_immediateContext context)
        {
            List<Ust> children = GetChildren();
            var invocation = new InvocationExpression(context.GetTextSpan())
            {
                Target = children[0].AsExpression(),
                Arguments = new ArgsUst(children[2].AsExpression())
            };

            return invocation;
        }

        private InvocationExpression ConvertFunctionOrProcedureCall(ParserRuleContext context)
        {
            List<Ust> children = GetChildren();
            int targetIndex = CheckChild<Keyword>(PlSqlLexer.CALL, 0) ? 1 : 0;
            ArgsUst args = children[children.Count - 1] is ArgsUst argsUst ? argsUst : new ArgsUst();
            return new InvocationExpression(children[targetIndex].AsExpression(), args, context.GetTextSpan());
        }

        private Expression ConvertRoutineName(PlSqlParser.Routine_nameContext context)
        {
            List<Ust> children = GetChildren();
            Expression result = children[0].AsIdToken();

            int index = 1;
            while (CheckChild<IOperatorOrPunctuator>(PlSqlLexer.PERIOD, index))
            {
                Expression name = children[index + 1].AsIdToken();
                result = new MemberReferenceExpression(result, name, result.TextSpan.Union(name.TextSpan));
                index += 2;
            }

            return result;
        }

        private Ust ConvertGeneralElementPart(PlSqlParser.General_element_partContext context)
        {
            List<Ust> children = GetChildren();

            int index = CheckChild<Keyword>(PlSqlLexer.INTRODUCER, 0) ? 2 : 0;

            Expression result = children[index].AsExpression();
            index++;

            while (CheckChild<IOperatorOrPunctuator>(PlSqlLexer.PERIOD, index))
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

        private ArgsUst ConvertFunctionArgument(PlSqlParser.Function_argumentContext context)
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

        private Expression ConvertRelationalExpression(PlSqlParser.Relational_expressionContext context)
        {
            List<Ust> children = GetChildren();

            if (children.Count == 1)
            {
                return children[0].AsExpression();
            }

            var op = (BinaryOperatorLiteral)children[0];
            var right = children[1].AsExpression();
            Pop();
            var left = GetChild(0).AsExpression();
            Peek().Clear();
            PushNew(context);

            var result = new BinaryOperatorExpression(left, op, right, context.GetTextSpan());

            return result;
        }

        private BinaryOperatorLiteral ConvertRelationalOperator(PlSqlParser.Relational_operatorContext context)
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
    }
}
