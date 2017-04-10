using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.TSqlParseTreeUst.Parser;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Collections;
using Antlr4.Runtime;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.SqlParseTreeUst
{
    public partial class TSqlConverterVisitor : AntlrDefaultVisitor, ItsqlVisitor<UstNode>
    {
        public TSqlConverterVisitor(string fileName, string fileData)
            : base(fileName, fileData)
        {
        }

        public UstNode VisitTsql_file([NotNull] tsqlParser.Tsql_fileContext context)
        {
            FileNode.Root = new BlockStatement(context.batch().Select(b => (Statement)Visit(b)).ToArray(), context.GetTextSpan(), FileNode);
            return FileNode;
        }

        public UstNode VisitBatch([NotNull] tsqlParser.BatchContext context)
        {
            Statement[] sqlClauses = GetStatements(context.sql_clauses());
            var result = new BlockStatement(sqlClauses, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitSql_clauses([NotNull] tsqlParser.Sql_clausesContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitSql_clause([NotNull] tsqlParser.Sql_clauseContext context)
        {
            var result = Visit(context.children[0]);
            if (!(result is Statement))
            {
                result = new WrapperStatement(result);
            }
            return result;
        }

        public UstNode VisitDml_clause([NotNull] tsqlParser.Dml_clauseContext context)
        {
            return Visit(context.children[0]);
        }

        public UstNode VisitDdl_clause([NotNull] tsqlParser.Ddl_clauseContext context)
        {
            return Visit(context.children[0]);
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitCfl_statement([NotNull] tsqlParser.Cfl_statementContext context)
        {
            return Visit(context);
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitAnother_statement([NotNull] tsqlParser.Another_statementContext context)
        {
            return (Statement)Visit(context.children[0]);
        }

        #region CFL statements

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitBlock_statement([NotNull] tsqlParser.Block_statementContext context)
        {
            Statement[] sqlClauses = GetStatements(context.sql_clauses());
            var result = new BlockStatement(sqlClauses, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="BreakStatement"/></returns>
        public UstNode VisitBreak_statement([NotNull] tsqlParser.Break_statementContext context)
        {
            var result = new BreakStatement(context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ContinueStatement"/></returns>
        public UstNode VisitContinue_statement([NotNull] tsqlParser.Continue_statementContext context)
        {
            var result = new ContinueStatement(context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="GotoStatement"/></returns>
        public UstNode VisitGoto_statement([NotNull] tsqlParser.Goto_statementContext context)
        {
            var id = (IdToken)Visit(context.id());
            var result = new GotoStatement(id, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="IfElseStatement"/></returns>
        public UstNode VisitIf_statement([NotNull] tsqlParser.If_statementContext context)
        {
            var condition = (Expression)Visit(context.search_condition());
            var trueStatement = (Statement)Visit(context.sql_clause(0));
            var result = new IfElseStatement(condition, trueStatement, context.GetTextSpan(), FileNode);
            if (context.sql_clause().Length == 2)
            {
                result.FalseStatement = (Statement)Visit(context.sql_clause(1));
            }
            return result;
        }

        /// <returns><see cref="ReturnStatement"/></returns>
        public UstNode VisitReturn_statement([NotNull] tsqlParser.Return_statementContext context)
        {
            Expression expression = null;
            if (context.expression() != null)
            {
                expression = (Expression)Visit(context.expression());
            }
            var result = new ReturnStatement(expression, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ThrowStatement"/></returns>
        public UstNode VisitThrow_statement([NotNull] tsqlParser.Throw_statementContext context)
        {
            var exprs = new List<Expression>();
            if (context.error_number != null)
            {
                exprs.Add(ExtractLiteral(context.error_number));
                exprs.Add(ExtractLiteral(context.message));
                exprs.Add(ExtractLiteral(context.state));
            }
            var result = new ThrowStatement(
                new MultichildExpression(exprs, FileNode),
                context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="TryCatchStatement"/></returns>
        public UstNode VisitTry_catch_statement([NotNull] tsqlParser.Try_catch_statementContext context)
        {
            Statement[] tryClauses = GetStatements(context.try_clauses);
            Statement[] catchClauses = GetStatements(context.catch_clauses);

            var tryBlock = context.try_clauses != null ? new BlockStatement(tryClauses, context.try_clauses.GetTextSpan(), FileNode) : null;
            var tryCatchStatement = new TryCatchStatement(tryBlock, context.GetTextSpan(), FileNode);

            var catchBlock = new BlockStatement(catchClauses,
                context.catch_clauses?.GetTextSpan() ?? context.CATCH(0).GetTextSpan().Union(context.CATCH(1).GetTextSpan()), FileNode);
            tryCatchStatement.CatchClauses = new List<CatchClause>()
            {
                new CatchClause(null, null, catchBlock, catchBlock.TextSpan, FileNode)
            };

            return tryCatchStatement;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitWaitfor_statement([NotNull] tsqlParser.Waitfor_statementContext context)
        {
            return CreateSpecialInvocation(context.WAITFOR(), context, (Expression)Visit(context.expression()));
        }

        /// <returns><see cref="WhileStatement"/></returns>
        public UstNode VisitWhile_statement([NotNull] tsqlParser.While_statementContext context)
        {
            var condition = (Expression)Visit(context.search_condition());
            var statement = (Statement)Visit(context.sql_clause());
            var result = new WhileStatement(condition, statement, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitPrint_statement([NotNull] tsqlParser.Print_statementContext context)
        {
            InvocationExpression invoke = CreateSpecialInvocation(context.PRINT(), context, (Expression)Visit(context.expression()));
            var result = new ExpressionStatement(invoke);
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitRaiseerror_statement([NotNull] tsqlParser.Raiseerror_statementContext context)
        {
            var msg = (Token)ExtractLiteral(context.msg);
            var args = new List<Expression>() { msg,
                (Expression)Visit(context.severity),
                (Expression)Visit(context.state) };
            for (int i = 2; i < context.constant_LOCAL_ID().Length; i++)
                args.Add((Expression)Visit(context.constant_LOCAL_ID()[i]));
            InvocationExpression invoke = CreateSpecialInvocation(context.RAISERROR(), context, args);
            var result = new ExpressionStatement(invoke);
            return result;
        }

        #endregion

        #region DML statements

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitDelete_statement([NotNull] tsqlParser.Delete_statementContext context)
        {
            var exprs = new List<Expression>();

            if (context.with_expression() != null)
            {
                exprs.Add((Expression)Visit(context.with_expression()));
            }
            if (context.expression() != null)
            {
                exprs.Add((Expression)Visit(context.expression()));
            }

            exprs.Add((Expression)Visit(context.delete_statement_from()));

            if (context.insert_with_table_hints() != null)
            {
                exprs.Add((MultichildExpression)Visit(context.insert_with_table_hints()));
            }
            if (context.output_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.output_clause()));
            }
            if (context.table_sources() != null)
            {
                exprs.Add((MultichildExpression)Visit(context.table_sources()));
            }
            if (context.search_condition() != null)
            {
                exprs.Add((Expression)Visit(context.search_condition()));
            }
            if (context.for_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.for_clause()));
            }
            if (context.option_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.option_clause()));
            }

            var result = CreateSpecialInvocation(context.DELETE(), context, exprs);
            return new ExpressionStatement(result);
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitDelete_statement_from([NotNull] tsqlParser.Delete_statement_fromContext context)
        {
            return (Expression)Visit(context.GetChild(0));
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitInsert_with_table_hints([NotNull] tsqlParser.Insert_with_table_hintsContext context)
        {
            var exprs = context.table_hint().Select(hint => (Expression)Visit(hint)).ToList();
            var result = new MultichildExpression(exprs, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitInsert_statement([NotNull] tsqlParser.Insert_statementContext context)
        {
            var exprs = new List<Expression>();

            if (context.with_expression() != null)
            {
                exprs.Add((Expression)Visit(context.with_expression()));
            }
            if (context.expression() != null)
            {
                exprs.Add((Expression)Visit(context.expression()));
            }

            if (context.ddl_object() != null)
            {
                exprs.Add((Token)Visit(context.ddl_object()));
            }
            else if (context.rowset_function_limited() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.rowset_function_limited()));
            }

            if (context.insert_with_table_hints() != null)
            {
                exprs.Add((MultichildExpression)Visit(context.insert_with_table_hints()));
            }
            if (context.column_name_list() !=null)
            {
                exprs.Add(new WrapperExpression((ArgsNode)Visit(context.column_name_list())));
            }
            if (context.output_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.output_clause()));
            }

            exprs.Add((Expression)Visit(context.insert_statement_value()));

            if (context.for_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.for_clause()));
            }
            if (context.option_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.option_clause()));
            }

            var result = CreateSpecialInvocation(context.INSERT(), context, exprs);
            return new ExpressionStatement(result);
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitInsert_statement_value([NotNull] tsqlParser.Insert_statement_valueContext context)
        {
            if (context.DEFAULT() != null)
            {
                return new IdToken(context.DEFAULT().GetText().ToLowerInvariant() + context.VALUES().GetText().ToLowerInvariant(),
                    context.DEFAULT().GetTextSpan().Union(context.VALUES().GetTextSpan()), FileNode);
            }
            else
            {
                var result = Visit(context.GetChild(0));
                if (!(result is Expression))
                {
                    result = new WrapperExpression(result);
                }
                return result;
            }
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitSelect_statement([NotNull] tsqlParser.Select_statementContext context)
        {
            var exprs = new List<Expression>();
            
            if (context.with_expression() != null)
            {
                exprs.Add((Expression)Visit(context.with_expression()));
            }
            exprs.Add((Expression)Visit(context.query_expression()));

            if (context.order_by_clause() != null)
            {
                exprs.Add((Expression)Visit(context.order_by_clause()));
            }

            if (context.for_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.for_clause()));
            }

            if (context.option_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.option_clause()));
            }

            var selectLiteral = new IdToken("select", default(TextSpan), FileNode);
            var result = new InvocationExpression(selectLiteral, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            return new ExpressionStatement(result);
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitUpdate_statement([NotNull] tsqlParser.Update_statementContext context)
        {
            var exprs = new List<Expression>();

            if (context.with_expression() != null)
            {
                exprs.Add((Expression)Visit(context.with_expression()));
            }
            if (context.expression() != null)
            {
                exprs.Add((Expression)Visit(context.expression()));
            }

            if (context.ddl_object() != null)
            {
                exprs.Add((Token)Visit(context.ddl_object()));
            }
            else if (context.rowset_function_limited() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.rowset_function_limited()));
            }

            if (context.with_table_hints() != null)
            {
                exprs.Add((MultichildExpression)Visit(context.with_table_hints()));
            }

            if (context.output_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.output_clause()));
            }

            if (context.table_sources() != null)
            {
                exprs.Add((MultichildExpression)Visit(context.table_sources()));
            }

            if (context.search_condition_list() != null)
            {
                exprs.Add(new WrapperExpression(Visit(context.search_condition_list())));
            }

            if (context.for_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.for_clause()));
            }
            if (context.option_clause() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.option_clause()));
            }

            var result = CreateSpecialInvocation(context.UPDATE(), context, exprs);
            return new ExpressionStatement(result);
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitOutput_clause([NotNull] tsqlParser.Output_clauseContext context)
        {
            var exprs = new List<Expression>();
            var multichildren = context.output_dml_list_elem()
                .Select(elem => (MultichildExpression)Visit(elem));
            exprs.AddRange(multichildren);
            if (context.column_name_list() != null)
            {
                exprs.Add(new WrapperExpression(
                    (ArgsNode)Visit(context.column_name_list())));
            }

            var result = CreateSpecialInvocation(context.OUTPUT(), context, exprs);
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitOutput_dml_list_elem([NotNull] tsqlParser.Output_dml_list_elemContext context)
        {
            var exprs = new List<Expression>();
            if (context.output_column_name() != null)
            {
                exprs.Add((IdToken)Visit(context.output_column_name()));
            }
            else
            {
                exprs.Add((Expression)Visit(context.expression()));
            }

            if (context.column_alias() != null)
            {
                exprs.Add((Token)Visit(context.column_alias()));
            }

            return new MultichildExpression(exprs, FileNode);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitOutput_column_name([NotNull] tsqlParser.Output_column_nameContext context)
        {
            var result = new IdToken(RemoveSpaces(context.GetText()), context.GetTextSpan(), FileNode);
            return result;
        }

        #endregion

        #region DDL statements

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitCreate_index([NotNull] tsqlParser.Create_indexContext context)
        {
            var funcName = new IdToken((context.CREATE().GetText() + " " + context.INDEX().GetText()).ToLowerInvariant(),
               context.CREATE().GetTextSpan().Union(context.INDEX().GetTextSpan()), FileNode);
            var exprs = new List<Expression>();
            if (context.id().Length > 0)
            {
                exprs.Add((IdToken)Visit(context.id(0)));
            }
            exprs.Add((Expression)Visit(context.table_name_with_hint()));
            exprs.Add(new WrapperExpression((ArgsNode)Visit(context.column_name_list())));
            var invocation = new InvocationExpression(funcName, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(invocation);
            return result;
        }

        /// <returns><see cref="MethodDeclaration"/></returns>
        public UstNode VisitCreate_or_alter_procedure([NotNull] tsqlParser.Create_or_alter_procedureContext context)
        {
            var exprs = new List<Expression>();
            exprs.AddRange(context.procedure_option().Select(opt => (Expression)Visit(opt)).ToArray());

            var id = (IdToken)Visit(context.func_proc_name());
            var body = new BlockStatement(
                context.sql_clauses().sql_clause().Select(clause => (Statement)Visit(clause)).ToArray(),
                FileNode);
            ParameterDeclaration[] parameters = context.procedure_param()
                .Select(param => (ParameterDeclaration)Visit(param)).ToArray();

            var result = new MethodDeclaration(id, parameters, body, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ParameterDeclaration"/></returns>
        public UstNode VisitProcedure_param([NotNull] tsqlParser.Procedure_paramContext context)
        {
            var type = (TypeToken)Visit(context.data_type());
            var id = (IdToken)Visit(context.LOCAL_ID());
            var result = new ParameterDeclaration(type, id, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitProcedure_option([NotNull] tsqlParser.Procedure_optionContext context)
        {
            Expression result;
            if (context.execute_clause() != null)
            {
                result = (Expression)Visit(context.execute_clause());
            }
            else
            {
                result = new IdToken(context.GetChild(0).GetText(), context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitCreate_statistics([NotNull] tsqlParser.Create_statisticsContext context)
        {
            var funcName = new IdToken((context.CREATE().GetText() + " " + context.STATISTICS().GetText()).ToLowerInvariant(),
               context.CREATE().GetTextSpan().Union(context.STATISTICS().GetTextSpan()), FileNode);
            var exprs = new List<Expression>();
            exprs.Add((IdToken)Visit(context.id()));
            exprs.Add((Expression)Visit(context.table_name_with_hint()));
            exprs.Add(new WrapperExpression((ArgsNode)Visit(context.column_name_list())));
            var invocation = new InvocationExpression(funcName, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(invocation);
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitCreate_table([NotNull] tsqlParser.Create_tableContext context)
        {
            var funcName = new IdToken((context.CREATE().GetText() + " " + context.TABLE().GetText()).ToLowerInvariant(),
               context.CREATE().GetTextSpan().Union(context.TABLE().GetTextSpan()), FileNode);
            var exprs = new List<Expression>();
            exprs.Add((IdToken)Visit(context.table_name()));
            exprs.Add(new WrapperExpression((ArgsNode)Visit(context.column_def_table_constraints())));
            if (context.id().Length > 0)
            {
                exprs.Add((IdToken)Visit(context.id(0)));
            }
            var invocation = new InvocationExpression(funcName, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(invocation);
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitCreate_view([NotNull] tsqlParser.Create_viewContext context)
        {
            var funcName = new IdToken((context.CREATE().GetText() + " " + context.VIEW().GetText()).ToLowerInvariant(),
               context.CREATE().GetTextSpan().Union(context.VIEW().GetTextSpan()), FileNode);
            var exprs = new List<Expression>();
            exprs.Add((IdToken)Visit(context.simple_name()));
            if (context.column_name_list() != null)
            {
                exprs.Add(new WrapperExpression((ArgsNode)Visit(context.column_name_list())));
            }
            exprs.AddRange(context.view_attribute().Select(attr => (IdToken)Visit(attr)));
            exprs.Add(new WrapperExpression((Statement)Visit(context.select_statement())));
            var invocation = new InvocationExpression(funcName, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(invocation);
            return result;
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitView_attribute([NotNull] tsqlParser.View_attributeContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitAlter_table([NotNull] tsqlParser.Alter_tableContext context)
        {
            var funcName = new IdToken((context.ALTER().GetText() + " " + context.TABLE(0).GetText()).ToLowerInvariant(),
                context.ALTER().GetTextSpan().Union(context.TABLE(0).GetTextSpan()), FileNode);
            var exprs = new List<Expression>();
            var tableName = (IdToken)Visit(context.table_name(0));
            exprs.Add(tableName);
            if (context.column_def_table_constraint() != null)
            {
                exprs.Add((Expression)Visit(context.column_def_table_constraint()));
            }
            var invocation = new InvocationExpression(tableName, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(invocation);
            return result;
        }
        
        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitAlter_database([NotNull] tsqlParser.Alter_databaseContext context)
        {
            var funcName = new IdToken((context.ALTER().GetText() + " " + context.DATABASE().GetText()).ToLowerInvariant(),
                context.ALTER().GetTextSpan().Union(context.DATABASE().GetTextSpan()), FileNode);

            IdToken id;
            if (context.database != null)
            {
                id = (IdToken)Visit(context.database);
            }
            else
            {
                id = new IdToken(context.CURRENT().GetText().ToLowerInvariant(), context.CURRENT().GetTextSpan(), FileNode);
            }
            var invocation = new InvocationExpression(funcName, new ArgsNode(id), context.GetTextSpan(), FileNode);
            return new ExpressionStatement(invocation);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitDatabase_optionspec([NotNull] tsqlParser.Database_optionspecContext context)
        {
            return VisitChildren(context);
            /*var idToken = (IdToken)Visit(context.id(0));
            if (context.ChildCount > 1)
            {
                idToken.Id += " " + context.GetChild(1).GetText();
            }
            return new IdToken(idToken.Id, context.GetTextSpan(), FileNode);*/
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitDrop_index([NotNull] tsqlParser.Drop_indexContext context)
        {
            var funcName = new IdToken((context.DROP().GetText() + " " + context.INDEX().GetText()).ToLowerInvariant(),
                context.DROP().GetTextSpan().Union(context.INDEX().GetTextSpan()), FileNode);
            var id = (IdToken)Visit(context.id());
            var invocation = new InvocationExpression(funcName, new ArgsNode(id), context.GetTextSpan(), FileNode);
            return new ExpressionStatement(invocation);
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitDrop_procedure([NotNull] tsqlParser.Drop_procedureContext context)
        {
            var funcName = new IdToken((context.DROP().GetText() + " " + context.PROCEDURE().GetText()).ToLowerInvariant(),
                context.DROP().GetTextSpan().Union(context.PROCEDURE().GetTextSpan()), FileNode);
            var procName = (IdToken)Visit(context.func_proc_name(0));
            var invocation = new InvocationExpression(funcName, new ArgsNode(procName), context.GetTextSpan(), FileNode);
            return new ExpressionStatement(invocation);
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitDrop_statistics([NotNull] tsqlParser.Drop_statisticsContext context)
        {
            var funcName = new IdToken((context.DROP().GetText() + " " + context.STATISTICS().GetText()).ToLowerInvariant(),
                context.DROP().GetTextSpan().Union(context.STATISTICS().GetTextSpan()), FileNode);
            var id = (IdToken)Visit(context.id());
            var invocation = new InvocationExpression(funcName, new ArgsNode(id), context.GetTextSpan(), FileNode);
            return new ExpressionStatement(invocation);
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitDrop_table([NotNull] tsqlParser.Drop_tableContext context)
        {
            var funcName = new IdToken((context.DROP().GetText() + " " + context.TABLE().GetText()).ToLowerInvariant(),
                context.DROP().GetTextSpan().Union(context.TABLE().GetTextSpan()), FileNode);
            var tableName = (IdToken)Visit(context.table_name());
            var invocation = new InvocationExpression(funcName, new ArgsNode(tableName), context.GetTextSpan(), FileNode);
            return new ExpressionStatement(invocation);
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitDrop_view([NotNull] tsqlParser.Drop_viewContext context)
        {
            var exprs = context.simple_name().Select(name => (IdToken)Visit(name)).ToArray();
            var funcName = new IdToken((context.DROP().GetText() + " " + context.VIEW().GetText()).ToLowerInvariant(),
                context.DROP().GetTextSpan().Union(context.VIEW().GetTextSpan()), FileNode);
            var invocation = new InvocationExpression(funcName, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            return new ExpressionStatement(invocation);
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitRowset_function_limited([NotNull] tsqlParser.Rowset_function_limitedContext context)
        {
            return (InvocationExpression)Visit(context.GetChild(0));
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitOpenquery([NotNull] tsqlParser.OpenqueryContext context)
        {
            return CreateSpecialInvocation(context.OPENQUERY(), context,
                new List<Expression> { (IdToken)Visit(context.id()), ExtractLiteral(context.query) });
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitOpendatasource([NotNull] tsqlParser.OpendatasourceContext context)
        {
            return CreateSpecialInvocation(context.OPENDATASOURCE(), context, new List<Expression> {
                ExtractLiteral(context.provider), ExtractLiteral(context.init),
                (IdToken)Visit(context.database), (IdToken)Visit(context.scheme),
                (IdToken)Visit(context.scheme) });
        }

        #endregion

        #region Other statements

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitDeclare_statement([NotNull] tsqlParser.Declare_statementContext context)
        {
            Statement result;
            if (context.LOCAL_ID() != null)
            {
                var right = (Expression)Visit(context.table_type_definition());
                var assignment = new AssignmentExpression((Token)Visit(context.LOCAL_ID()), right,
                    context.table_type_definition().GetTextSpan(), FileNode);
                result = new ExpressionStatement(
                    new VariableDeclarationExpression(new TypeToken("TABLE", default(TextSpan), FileNode),
                    new[] { assignment }, context.GetTextSpan(), FileNode));
            }
            else
            {
                if (context.declare_local().Length == 1)
                {
                    result = new ExpressionStatement(
                        (VariableDeclarationExpression)Visit(context.declare_local(0)),
                        context.GetTextSpan(), FileNode);
                }
                else
                {
                    Statement[] statements = context.declare_local()
                        .Select(local => new ExpressionStatement(
                         (VariableDeclarationExpression)Visit(local))).ToArray();
                    result = new BlockStatement(statements, context.GetTextSpan(), FileNode);
                }
            }
            return result;
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitCursor_statement([NotNull] tsqlParser.Cursor_statementContext context)
        {
            Statement result;
            if (context.fetch_cursor() != null)
            {
                result = (Statement)Visit(context.fetch_cursor());
            }
            else if (context.declare_cursor() != null)
            {
                result = (Statement)Visit(context.declare_cursor());
            }
            else
            {
                var first = context.GetChild<ITerminalNode>(0);
                var funcName = new IdToken(first.GetText(), first.GetTextSpan(), FileNode);
                var args = new ArgsNode((Token)Visit(context.cursor_name()));
                var invocation = new InvocationExpression(funcName, args, context.GetTextSpan(), FileNode);
                result = new ExpressionStatement(invocation);
            }
            return result;
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitExecute_statement([NotNull] tsqlParser.Execute_statementContext context)
        {
            var first = context.GetChild<ITerminalNode>(0);
            var argsNode = new ArgsNode();
            Expression expr;
            if (context.func_proc_name() != null)
            {
                var funcName = (IdToken)Visit(context.func_proc_name());
                for (int i = 0; i < context.execute_statement_arg().Length; i++)
                {
                    argsNode.Collection.Add((Expression)Visit(context.execute_statement_arg(i)));
                }
                expr = new InvocationExpression(funcName, argsNode, context.GetTextSpan(), FileNode);
            }
            else
            {
                var executeName = new IdToken("exec", first.GetTextSpan(), FileNode);
                expr = (Expression)Visit(context.execute_var_string(0));
                for (int i = 1; i < context.execute_var_string().Length; i++)
                {
                    var right = (Expression)Visit(context.execute_var_string(i));
                    var binatyOpLiteral = new BinaryOperatorLiteral(BinaryOperator.Plus,
                        context.execute_var_string(i).GetTextSpan(), FileNode);
                    expr = new BinaryOperatorExpression(expr, binatyOpLiteral, right,
                        context.execute_var_string(0).GetTextSpan().Union(
                            context.execute_var_string(i).GetTextSpan()), FileNode);
                }
                argsNode.Collection.Add(expr);
                expr = new InvocationExpression(executeName, argsNode, context.GetTextSpan(), FileNode);
            }
            var result = new ExpressionStatement(expr);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitExecute_statement_arg([NotNull] tsqlParser.Execute_statement_argContext context)
        {
            Expression result;
            if (context.constant_LOCAL_ID() != null)
            {
                result = (Token)Visit(context.constant_LOCAL_ID());
            }
            else if (context.id() != null)
            {
                result = (Expression)Visit(context.id());
            }
            else
            {
                var lastTerminal = context.GetChild<ITerminalNode>(context.ChildCount - 1);
                result = new IdToken(lastTerminal.GetText(), lastTerminal.GetTextSpan(), FileNode);
            }

            if (context.parameter != null)
            {
                Token left = ExtractLiteral(context.parameter);
                result = new AssignmentExpression(left, result, context.GetTextSpan(), FileNode);
            }
            return result;
        }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitExecute_var_string([NotNull] tsqlParser.Execute_var_stringContext context)
        {
            return (Token)Visit(context.GetChild<ITerminalNode>(0));
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitSecurity_statement([NotNull] tsqlParser.Security_statementContext context)
        {
            Expression expr;
            if (context.execute_clause() != null)
            {
                expr = (Expression)Visit(context.execute_clause());
            }
            else if (context.GRANT().Length > 0)
            {
                string str = context.GRANT(0).GetText().ToLowerInvariant();
                if (context.ALL() != null)
                {
                    str += "_" + context.ALL().GetText().ToLowerInvariant();
                }
                var funcName = new IdToken(str, context.GetTextSpan(), FileNode);
                var args = new ArgsNode();
                expr = new InvocationExpression(funcName, args, context.GetTextSpan(), FileNode);
            }
            else
            {
                var args = new ArgsNode();
                if (context.LOCAL_ID() != null)
                {
                    args.Collection.Add((Token)Visit(context.LOCAL_ID()));
                }
                expr = new InvocationExpression(
                    new IdToken(context.REVERT().GetText(), context.REVERT().GetTextSpan(), FileNode),
                    new ArgsNode(), context.GetTextSpan(), FileNode);
            }
            var result = new ExpressionStatement(expr);
            return result;
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitGrant_permission([NotNull] tsqlParser.Grant_permissionContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitSet_statement([NotNull] tsqlParser.Set_statementContext context)
        {
            Statement result = null;
            if (context.LOCAL_ID() != null)
            {
                var localId = (Token)Visit(context.LOCAL_ID());
                if (context.assignment_operator() != null)
                {
                    var binaryOpLiteral = (BinaryOperatorLiteral)Visit(context.assignment_operator());
                    result = new ExpressionStatement(
                        new BinaryOperatorExpression(localId, binaryOpLiteral,
                        (Expression)Visit(context.expression()), context.GetTextSpan(), FileNode));
                }
                else
                {
                    if (context.expression() != null)
                    {
                        Expression left = localId;
                        if (context.member_name != null)
                        {
                            left = new MemberReferenceExpression(localId, (IdToken)Visit(context.member_name),
                                context.LOCAL_ID().GetTextSpan().Union(context.member_name.GetTextSpan()), FileNode);
                        }
                        result = new ExpressionStatement(
                            new AssignmentExpression(left, (Expression)Visit(context.expression()),
                            context.GetTextSpan(), FileNode));
                    }
                    else if (context.declare_set_cursor_common() != null)
                    {
                        result = new ExpressionStatement((Expression)Visit(context.declare_set_cursor_common()));
                    }
                }
            }
            else
            {
                result = (Statement)Visit(context.set_special());
            }
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitTransaction_statement([NotNull] tsqlParser.Transaction_statementContext context)
        {
            Token id;
            var args = new ArgsNode();
            if (context.id() != null)
            {
                id = (IdToken)Visit(context.id());
                args.Collection.Add(id);
            }
            else if (context.LOCAL_ID() != null)
            {
                id = (Token)Visit(context.LOCAL_ID());
                args.Collection.Add(id);
            }
            var first = context.GetChild<ITerminalNode>(0);
            var functionName = new IdToken(first.GetText(), first.GetTextSpan(), FileNode);
            return new ExpressionStatement(new InvocationExpression(functionName, args, context.GetTextSpan(), FileNode));
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitGo_statement([NotNull] tsqlParser.Go_statementContext context)
        {
            return new ExpressionStatement(CreateSpecialInvocation(context.GO(), context, new List<Expression>()));
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitUse_statement([NotNull] tsqlParser.Use_statementContext context)
        {
            var database = (IdToken)Visit(context.id());
            return new ExpressionStatement(CreateSpecialInvocation(context.USE(), context, database));
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitExecute_clause([NotNull] tsqlParser.Execute_clauseContext context)
        {
            var first = context.GetChild<ITerminalNode>(0);
            var executeName = new IdToken(first.GetText(), first.GetTextSpan(), FileNode);
            return new InvocationExpression(executeName, new ArgsNode(new Expression[]
                { new IdToken(context.clause.Text, context.clause.GetTextSpan(), FileNode) }),
                  context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="VariableDeclarationExpression"/></returns>
        public UstNode VisitDeclare_local([NotNull] tsqlParser.Declare_localContext context)
        {
            var type = (TypeToken)Visit(context.data_type());
            var variable = (Token)Visit(context.LOCAL_ID());
            var initExpr = context.expression() != null ? (Expression)Visit(context.expression()) : null;
            var assignment = new AssignmentExpression(variable, initExpr,
                type.TextSpan.Union(initExpr?.TextSpan ?? default(TextSpan)), FileNode);
            var result = new VariableDeclarationExpression(type, new[] { assignment }, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ObjectCreateExpression"/></returns>
        public UstNode VisitTable_type_definition([NotNull] tsqlParser.Table_type_definitionContext context)
        {
            var type = new TypeToken(context.TABLE().GetText(), context.TABLE().GetTextSpan(), FileNode);
            var argsNode = (ArgsNode)Visit(context.column_def_table_constraints());
            var result = new ObjectCreateExpression(type, argsNode, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ArgsNode"/></returns>
        public UstNode VisitColumn_def_table_constraints([NotNull] tsqlParser.Column_def_table_constraintsContext context)
        {
            var result = new ArgsNode(context.column_def_table_constraint()
                .Select(def => (Expression)Visit(def)).ToArray());
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitColumn_def_table_constraint([NotNull] tsqlParser.Column_def_table_constraintContext context)
        {
            Expression result;
            if (context.column_definition() != null)
            {
                result = new WrapperExpression((FieldDeclaration)Visit(context.column_definition()));
            }
            else
            {
                result = (MultichildExpression)Visit(context.table_constraint());
            }
            return result;
        }

        /// <returns><see cref="FieldDeclaration"/></returns>
        public UstNode VisitColumn_definition([NotNull] tsqlParser.Column_definitionContext context)
        {
            Expression right = null;
            TextSpan assignmentTextSpan = context.id(0).GetTextSpan();
            if (context.constant_expression() != null)
            {
                right = (Expression)Visit(context.constant_expression());
                assignmentTextSpan = assignmentTextSpan.Union(right.TextSpan);
            }

            Expression[] constraints = context.column_constraint()
                .Select(constraint => (Expression)Visit(constraint)).ToArray();
            if (constraints.Length > 0)
            {
                if (right != null)
                {
                    right = new MultichildExpression(new List<Expression>(constraints) { right }, FileNode);
                }
                else
                {
                    right = new MultichildExpression(new List<Expression>(constraints), FileNode);
                }
            }

            var assignment = new AssignmentExpression((IdToken)Visit(context.id(0)), right, assignmentTextSpan, FileNode);
            var result = new FieldDeclaration(new[] { assignment }, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitColumn_constraint([NotNull] tsqlParser.Column_constraintContext context)
        {
            var exprs = new List<Expression>();
            if (context.index_options() != null)
            {
                exprs.AddRange(((ArgsNode)Visit(context.index_options())).Collection);
            }

            if (context.search_condition() != null)
            {
                exprs.Add((Expression)Visit(context.search_condition()));
            }
            return new MultichildExpression(exprs, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitTable_constraint([NotNull] tsqlParser.Table_constraintContext context)
        {
            var exprs = new List<Expression>();
            if (context.index_options() != null)
            {
                exprs.AddRange(((ArgsNode)Visit(context.index_options())).Collection);
            }

            if (context.search_condition() != null)
            {
                exprs.Add((Expression)Visit(context.search_condition()));
            }
            return new MultichildExpression(exprs, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="ArgsNode"/></returns>
        public UstNode VisitIndex_options([NotNull] tsqlParser.Index_optionsContext context)
        {
            Expression[] options = context.index_option().Select(option => (Expression)Visit(option))
                .ToArray();
            return new ArgsNode(options);
        }

        /// <returns><see cref="AssignmentExpression"/></returns>
        public UstNode VisitIndex_option([NotNull] tsqlParser.Index_optionContext context)
        {
            var left = (IdToken)Visit(context.simple_id(0));
            Token right;
            if (context.simple_id().Length > 1)
            {
                right = new IdToken(context.simple_id(1).GetText(), context.simple_id(1).GetTextSpan(), FileNode);
            }
            else if (context.on_off() != null)
            {
                right = (BooleanLiteral)Visit(context.on_off());
            }
            else
            {
                right = new IntLiteral(long.Parse(context.DECIMAL().GetText()), context.DECIMAL().Symbol.GetTextSpan(), FileNode);
            }

            var result = new AssignmentExpression(left, right, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitDeclare_cursor([NotNull] tsqlParser.Declare_cursorContext context)
        {
            var declareId = new IdToken(context.DECLARE().GetText().ToLowerInvariant() + "_" +
                 context.CURSOR().GetText().ToLowerInvariant(), context.GetTextSpan(), FileNode);
            var cursorName = (Token)Visit(context.cursor_name());
            var exprs = new List<Expression>() { cursorName };
            if (context.declare_set_cursor_common() != null)
            {
                exprs.Add((InvocationExpression)Visit(context.declare_set_cursor_common()));
            }
            if (context.column_name_list() != null)
            {
                exprs.AddRange(((ArgsNode)Visit(context.column_name_list())).Collection);
            }
            if (context.select_statement() != null)
            {
                exprs.Add(new WrapperExpression((Statement)Visit(context.select_statement())));
            }
            var invocation = new InvocationExpression(declareId, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(invocation);
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitDeclare_set_cursor_common([NotNull] tsqlParser.Declare_set_cursor_commonContext context)
        {
            var selectStatement = (Statement)Visit(context.select_statement());
            var argsNode = new ArgsNode(new WrapperExpression(selectStatement));
            var funcName = string.Join(" ", context.children.Take(context.ChildCount - 1).Select(c => c.GetText()));
            var funcNameLiteral = new IdToken(funcName, context.GetTextSpan(), FileNode);
            var result = new InvocationExpression(funcNameLiteral, argsNode, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitFetch_cursor([NotNull] tsqlParser.Fetch_cursorContext context)
        {
            var exprs = new List<Expression>();
            if (context.expression() != null)
            {
                exprs.Add((Expression)Visit(context.expression()));
            }
            var invocation = CreateSpecialInvocation(context.FETCH(), context, exprs);
            var result = new ExpressionStatement(invocation);
            return result;
        }

        /// <returns><see cref="ExpressionStatement"/></returns>
        public UstNode VisitSet_special([NotNull] tsqlParser.Set_specialContext context)
        {
            var funcName = new IdToken(context.SET().GetText().ToLowerInvariant(), context.GetTextSpan(), FileNode);
            var invocation = new InvocationExpression(funcName, new ArgsNode(), context.GetTextSpan(), FileNode);
            var result = new ExpressionStatement(invocation);
            return result;
        }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitConstant_LOCAL_ID([NotNull] tsqlParser.Constant_LOCAL_IDContext context)
        {
            Token result;
            if (context.constant() != null)
            {
                result = (Token)Visit(context.constant());
            }
            else
            {
                result = (Token)Visit(context.LOCAL_ID());
            }
            return result;
        }

        #endregion

        #region Expressions

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitExpression([NotNull] tsqlParser.ExpressionContext context)
        {
            return Visit(context);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitPrimitive_expression([NotNull] tsqlParser.Primitive_expressionContext context)
        {
            Token result;
            if (context.DEFAULT() != null)
            {
                result = new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
            }
            else if (context.NULL() != null)
            {
                result = new NullLiteral(context.GetTextSpan(), FileNode);
            }
            else if (context.LOCAL_ID() != null)
            {
                result = new IdToken(context.GetText().Substring(1), context.GetTextSpan(), FileNode);
            }
            else // constant
            {
                result = (Token)Visit(context.constant());
            }
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitFunction_call_expression([NotNull] tsqlParser.Function_call_expressionContext context)
        {
            if (context.function_call() != null)
            {
                return Visit(context.function_call());
            }
            else
            {
                return new InvocationExpression(
                    new IdToken("Collate", context.COLLATE().GetTextSpan(), FileNode),
                    new ArgsNode((IdToken)Visit(context.id())), context.GetTextSpan(), FileNode);
            }
        }

        /// <returns><see cref="WrapperExpression"/></returns>
        public UstNode VisitCase_expression([NotNull] tsqlParser.Case_expressionContext context)
        {
            SwitchStatement result = null;
            if (context.caseExpr != null)
            {
                var caseExpr = (Expression)Visit(context.caseExpr);
                SwitchSection[] switchSection = context.switch_section()
                    .Select(ss => (SwitchSection)Visit(ss)).ToArray();
                result = new SwitchStatement(caseExpr, switchSection, context.GetTextSpan(), FileNode);
            }
            else
            {
                SwitchSection[] switchSection = context.switch_search_condition_section()
                    .Select(ss => (SwitchSection)Visit(ss)).ToArray();
                result = new SwitchStatement(null, switchSection, context.GetTextSpan(), FileNode);
            }
            return new WrapperExpression(result);
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitColumn_ref_expression([NotNull] tsqlParser.Column_ref_expressionContext context)
        {
            return Visit(context.full_column_name());
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitBracket_expression([NotNull] tsqlParser.Bracket_expressionContext context)
        {
            return Visit(context.expression());
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitSubquery_expression([NotNull] tsqlParser.Subquery_expressionContext context)
        {
            return Visit(context.subquery());
        }

        /// <returns><see cref="UnaryOperatorExpression"/></returns>
        public UstNode VisitUnary_operator_expression([NotNull] tsqlParser.Unary_operator_expressionContext context)
        {
            var expr = (Expression)Visit(context.expression());
            UnaryOperator op = UnaryOperatorLiteral.PrefixTextUnaryOperator[context.GetChild(0).GetText()];
            var opLiteral = new UnaryOperatorLiteral(op, context.GetTextSpan(), FileNode);
            var result = new UnaryOperatorExpression(opLiteral, expr, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="BinaryOperatorExpression"/></returns>
        public UstNode VisitBinary_operator_expression([NotNull] tsqlParser.Binary_operator_expressionContext context)
        {
            var expr1 = (Expression)Visit(context.expression(0));
            var expr2 = (Expression)Visit(context.expression(1));
            var opText = RemoveSpaces(context.GetChild(1).GetText());
            if (opText == "=")
            {
                opText = "==";
            }
            BinaryOperator op = BinaryOperatorLiteral.TextBinaryOperator[opText];
            var opLiteral = new BinaryOperatorLiteral(op, context.GetTextSpan(), FileNode);
            var result = new BinaryOperatorExpression(expr1, opLiteral, expr2, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitOver_clause_expression([NotNull] tsqlParser.Over_clause_expressionContext context)
        {
            return Visit(context.over_clause());
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitConstant_expression([NotNull] tsqlParser.Constant_expressionContext context)
        {
            Expression result;
            if (context.NULL() != null)
            {
                result = new NullLiteral(context.GetTextSpan(), FileNode);
            }
            else if (context.constant() != null)
            {
                result = (Token)Visit(context.constant());
            }
            else if (context.function_call() != null)
            {
                result = (InvocationExpression)Visit(context.function_call());
            }
            else if (context.constant_expression() != null)
            {
                result = (Expression)Visit(context.constant_expression());
            }
            else
            {
                result = (Token)Visit(context.LOCAL_ID());
            }
            return result;
        }

        /// <returns><see cref="WrapperExpression"/></returns>
        public UstNode VisitSubquery([NotNull] tsqlParser.SubqueryContext context)
        {
            return new WrapperExpression((Statement)Visit(context.select_statement()),
                context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitWith_expression([NotNull] tsqlParser.With_expressionContext context)
        {
            List<Expression> exprs = context.common_table_expression()
                .Select(expr => (Expression)Visit(expr)).ToList();
            var result = CreateSpecialInvocation(context.WITH(), context, exprs);
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitCommon_table_expression([NotNull] tsqlParser.Common_table_expressionContext context)
        {
            var exprs = new List<Expression>();
            if (context.column_name_list() != null)
            {
                exprs.AddRange(((ArgsNode)Visit(context.column_name_list())).Collection);
            }
            var selectStatement = (Statement)Visit(context.select_statement());
            exprs.Add(new WrapperExpression(selectStatement));

            var result = new MultichildExpression(exprs, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitUpdate_elem([NotNull] tsqlParser.Update_elemContext context)
        {
            Expression[] children = context.children.Select(child => (Expression)Visit(child)).ToArray();
            var result = new MultichildExpression(children, FileNode);
            return result;
        }

        /// <returns><see cref="ArgsNode"/></returns>
        public UstNode VisitSearch_condition_list([NotNull] tsqlParser.Search_condition_listContext context)
        {
            var exprs =
                context.search_condition().Select(condition => (Expression)Visit(condition)).ToArray();
            var result = new ArgsNode(exprs);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitSearch_condition([NotNull] tsqlParser.Search_conditionContext context)
        {
            Expression result = (Expression)Visit(context.search_condition_and(0));
            if (context.search_condition_and().Length > 1)
            {
                var firstSpan = context.search_condition_and(0).GetTextSpan();
                for (int i = 1; i < context.search_condition_and().Length; i++)
                {
                    var andOpLiteral = new BinaryOperatorLiteral(BinaryOperator.LogicalOr,
                        context.GetChild<ITerminalNode>(i - 1).GetTextSpan(), FileNode);
                    var rightExpression = (Expression)Visit(context.search_condition_and(i));
                    result = new BinaryOperatorExpression(result, andOpLiteral, rightExpression,
                        firstSpan.Union(rightExpression.TextSpan), FileNode);
                }
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitSearch_condition_and([NotNull] tsqlParser.Search_condition_andContext context)
        {
            Expression result = (Expression)Visit(context.search_condition_not(0));
            if (context.search_condition_not().Length > 1)
            {
                var firstSpan = context.search_condition_not(0).GetTextSpan();
                for (int i = 1; i < context.search_condition_not().Length; i += 1)
                {
                    var andOpLiteral = new BinaryOperatorLiteral(BinaryOperator.LogicalAnd,
                        context.GetChild<ITerminalNode>(i - 1).GetTextSpan(), FileNode);
                    var rightExpression = (Expression)Visit(context.search_condition_not(i));
                    result = new BinaryOperatorExpression(result, andOpLiteral, rightExpression,
                        firstSpan.Union(rightExpression.TextSpan), FileNode);
                }
            }
            return result;
        }

        public UstNode VisitSearch_condition_not([NotNull] tsqlParser.Search_condition_notContext context)
        {
            var result = (Expression)Visit(context.predicate());
            if (context.NOT() != null)
            {
                var notOp = new UnaryOperatorLiteral(UnaryOperator.Not, context.NOT().GetTextSpan(), FileNode);
                result = new UnaryOperatorExpression(notOp, result, context.GetTextSpan(), FileNode);
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitPredicate([NotNull] tsqlParser.PredicateContext context)
        {
            Expression result = null;
            var textSpan = context.GetTextSpan();
            if (context.EXISTS() != null)
            {
                var args = new ArgsNode((Expression)Visit(context.subquery()));
                result = new InvocationExpression(new IdToken(context.EXISTS().GetText()), args, textSpan, FileNode);
            }
            else if (context.search_condition() != null)
            {
                result = (Expression)Visit(context.search_condition());
            }
            else
            {
                if (context.comparison_operator() != null)
                {
                    Expression left = (Expression)Visit(context.expression(0));
                    Expression right;
                    if (context.expression().Length == 2)
                    {
                        right = (Expression)Visit(context.expression(1));
                    }
                    else
                    {
                        right = (Expression)Visit(context.subquery());
                    }
                    var opLiteral = (BinaryOperatorLiteral)Visit(context.comparison_operator());
                    result = new BinaryOperatorExpression(left, opLiteral, right, textSpan, FileNode);
                }
                else
                {
                    if (context.BETWEEN() != null)
                    {
                        // expr0 BETWEEN expr1 AND expr2 =>
                        // expr0 >= expr1 && expr0 <= expr2
                        var expr = (Expression)Visit(context.expression(0));
                        var greaterLiteral = new BinaryOperatorLiteral(BinaryOperator.GreaterOrEqual,
                            context.BETWEEN().GetTextSpan(), FileNode);
                        var lessLiteral = new BinaryOperatorLiteral(BinaryOperator.LessOrEqual,
                            context.AND().GetTextSpan(), FileNode);
                        var greaterExpr = new BinaryOperatorExpression(expr, greaterLiteral,
                            (Expression)Visit(context.expression(1)),
                            context.BETWEEN().GetTextSpan().Union(context.expression(1).GetTextSpan()), FileNode);
                        var lessExpr = new BinaryOperatorExpression(expr, lessLiteral,
                            (Expression)Visit(context.expression(2)),
                            context.AND().GetTextSpan().Union(context.expression(2).GetTextSpan()), FileNode);
                        var andLiteral = new BinaryOperatorLiteral(BinaryOperator.LogicalAnd,
                            context.AND().GetTextSpan(), FileNode);
                        result = new BinaryOperatorExpression(greaterExpr, andLiteral, lessExpr,
                            textSpan, FileNode);
                    }
                    else if (context.IN() != null)
                    {
                        var exprs = new List<Expression>();
                        if (context.subquery() != null)
                        {
                            exprs.Add((Expression)Visit(context.subquery()));
                        }
                        else
                        {
                            exprs.AddRange(GetArgsNode(context.expression_list()).Collection);
                        }
                        var eqLiteral = new BinaryOperatorLiteral(BinaryOperator.Equal,
                            context.IN().GetTextSpan(), FileNode);
                        var leftExpression = (Expression)Visit(context.expression(0));
                        result = new BinaryOperatorExpression(leftExpression, eqLiteral, exprs[0],
                            context.expression(0).GetTextSpan().Union(exprs[0].TextSpan), FileNode);
                        for (int i = 1; i < exprs.Count; i++)
                        {
                            var orLiteral = new BinaryOperatorLiteral(BinaryOperator.LogicalOr,
                                context.IN().GetTextSpan(), FileNode);
                            var rightExpr = new BinaryOperatorExpression(
                                leftExpression, eqLiteral, exprs[i], exprs[i].TextSpan, FileNode);
                            result = new BinaryOperatorExpression(result, orLiteral, rightExpr,
                                leftExpression.TextSpan.Union(exprs[i].TextSpan), FileNode);
                        }
                    }
                    else if (context.LIKE() != null)
                    {
                        var left = (Expression)Visit(context.expression(0));
                        var right = (Expression)Visit(context.expression(1));
                        var equalLiteral = new BinaryOperatorLiteral(BinaryOperator.Equal, context.LIKE().GetTextSpan(), FileNode);
                        result = new BinaryOperatorExpression(left, equalLiteral, right, textSpan, FileNode);
                    }
                    else // IS
                    {
                        var functionName = (IdToken)Visit(context.IS());
                        var args = new List<Expression>();
                        args.Add((Expression)Visit(context.expression(0)));
                        args.Add((Expression)Visit(context.null_notnull()));
                        result = new InvocationExpression(functionName, new ArgsNode(args), textSpan, FileNode);
                    }
                    if (context.NOT() != null)
                    {
                        var notLiteral = new UnaryOperatorLiteral(UnaryOperator.Not, context.NOT().GetTextSpan(), FileNode);
                        result = new UnaryOperatorExpression(notLiteral, result, textSpan, FileNode);
                    }
                }
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitQuery_expression([NotNull] tsqlParser.Query_expressionContext context)
        {
            Expression result;

            if (context.query_specification() != null)
            {
                result = (Expression)Visit(context.query_specification());
            }
            else
            {
                result = (Expression)Visit(context.query_expression());
            }

            if (context.union().Length > 0)
            {
                result = new MultichildExpression(new List<Expression>(
                    context.union().Select(union => (InvocationExpression)Visit(union))), FileNode);
            }
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitUnion([NotNull] tsqlParser.UnionContext context)
        {
            var exprs = new List<Expression>();
            if (context.query_specification() != null)
            {
                exprs.Add((MultichildExpression)Visit(context.query_specification()));
            }
            else
            {
                exprs.Add((Expression)Visit(context.query_expression()));
            }

            InvocationExpression result = CreateSpecialInvocation(context.GetChild<ITerminalNode>(0), context, exprs);
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitQuery_specification([NotNull] tsqlParser.Query_specificationContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitOrder_by_clause([NotNull] tsqlParser.Order_by_clauseContext context)
        {
            var idToken = new IdToken(context.ORDER().GetText() + context.BY().GetText(),
                context.ORDER().GetTextSpan(), FileNode);
            List<Expression> exprs = context.order_by_expression()
                .Select(expr => (Expression)Visit(expr)).ToList();
            exprs.AddRange(context.expression().Select(expr => (Expression)Visit(expr)));
            var result = new InvocationExpression(idToken, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitFor_clause([NotNull] tsqlParser.For_clauseContext context)
        {
            var exprs = new List<Expression>();
            if (context.STRING() != null)
            {
                exprs.Add((Token)Visit(context.STRING()));
            }
            InvocationExpression result = CreateSpecialInvocation(context.FOR(), context, exprs);
            return result;
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitXml_common_directives([NotNull] tsqlParser.Xml_common_directivesContext context)
        {
            return (IdToken)Visit(context.GetChild(1));
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitOrder_by_expression([NotNull] tsqlParser.Order_by_expressionContext context)
        {
            return Visit(context.expression());
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitGroup_by_item([NotNull] tsqlParser.Group_by_itemContext context)
        {
            return Visit(context.expression());
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitOption_clause([NotNull] tsqlParser.Option_clauseContext context)
        {
            List<Expression> exprs = context.option()
                .Select(o => (Expression)Visit(o)).ToList();
            var result = CreateSpecialInvocation(context.OPTION(), context, exprs);
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitOption([NotNull] tsqlParser.OptionContext context)
        {
            var exprs = new List<Expression>();
            IdToken functionName;
            if (context.optimize_for_arg().Length > 0)
            {
                exprs.AddRange(context.optimize_for_arg().Select(arg => (AssignmentExpression)Visit(arg))
                    .ToArray());
                functionName = new IdToken(context.OPTIMIZE().GetText() + context.FOR().GetText(),
                    context.OPTIMIZE().GetTextSpan().Union(context.FOR().GetTextSpan()), FileNode);
            }
            else
            {
                functionName = new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
            }
            var result = new InvocationExpression(functionName, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="AssignmentExpression"/></returns>
        public UstNode VisitOptimize_for_arg([NotNull] tsqlParser.Optimize_for_argContext context)
        {
            Expression right;
            if (context.constant() != null)
            {
                right = (Token)Visit(context.constant());
            }
            else
            {
                right = new IdToken(context.UNKNOWN().GetText(), context.UNKNOWN().GetTextSpan(), FileNode);
            }
            var result = new AssignmentExpression((Token)Visit(context.LOCAL_ID()), right, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitSelect_list([NotNull] tsqlParser.Select_listContext context)
        {
            var result = new MultichildExpression(context.select_list_elem().Select(
                elem => (Expression)Visit(elem)).ToList(), context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitSelect_list_elem([NotNull] tsqlParser.Select_list_elemContext context)
        {
            Expression result;
            if (context.expression() == null)
            {
                if (context.table_name() != null)
                {
                    var leftLiteral = (IdToken)Visit(context.table_name());
                    var rightLiteral = new IdToken(string.Join("", context.children.Skip(2).Select(c => c.GetText())),
                        context.GetTextSpan(), FileNode);
                    result = new MemberReferenceExpression(leftLiteral, rightLiteral, context.GetTextSpan(), FileNode);
                }
                else
                {
                    result = new IdToken(RemoveSpaces(context.GetText()), context.GetTextSpan(), FileNode);
                }
            }
            else
            {
                result = (Expression)Visit(context.expression());
            }
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitTable_sources([NotNull] tsqlParser.Table_sourcesContext context)
        {
            var result = new MultichildExpression(
                context.table_source().Select(tableSource =>
                (Expression)Visit(tableSource)).ToList(), FileNode);
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitTable_source([NotNull] tsqlParser.Table_sourceContext context)
        {
            return (MultichildExpression)Visit(context.table_source_item_joined());
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitTable_source_item_joined([NotNull] tsqlParser.Table_source_item_joinedContext context)
        {
            var exprs = new List<Expression>();
            if (context.table_source_item() != null)
            {
                exprs.Add((Expression)Visit(context.table_source_item()));
            }
            exprs.AddRange(context.join_part().Select(part => (InvocationExpression)Visit(part)));

            return new MultichildExpression(exprs, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitTable_source_item([NotNull] tsqlParser.Table_source_itemContext context)
        {
            var exprs = new List<Expression>();
            if (context.as_table_alias() != null)
            {
                exprs.Add((Expression)Visit(context.as_table_alias()));
            }
            if (context.column_alias_list() != null)
            {
                exprs.Add((Expression)Visit(context.column_alias_list()));
            }

            if (context.LOCAL_ID() == null)
            {
                exprs.Add((Expression)Visit(context.GetChild(0)));
            }
            else
            {
                Token expr = (Token)Visit(context.LOCAL_ID());
                if (context.function_call() != null)
                {
                    exprs.Add(new MemberReferenceExpression(expr,
                        (InvocationExpression)Visit(context.function_call()),
                        context.LOCAL_ID().GetTextSpan().Union(context.function_call().GetTextSpan()), FileNode));
                }
            }

            return new MultichildExpression(exprs, FileNode);
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitJoin_part([NotNull] tsqlParser.Join_partContext context)
        {
            var nameLiteral = new IdToken(context.GetChild<ITerminalNode>(0).GetText() +
                context.GetChild<ITerminalNode>(1).GetText(), context.GetTextSpan(), FileNode);
            var exprs = new List<Expression>();
            exprs.Add((Expression)Visit(context.table_source()));
            if (context.search_condition() != null)
            {
                exprs.Add((Expression)Visit(context.search_condition()));
            }
            var argsNode = new ArgsNode(exprs);
            var result = new InvocationExpression(nameLiteral, argsNode, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitTable_name_with_hint([NotNull] tsqlParser.Table_name_with_hintContext context)
        {
            Expression result = (IdToken)Visit(context.table_name());
            if (context.with_table_hints() != null)
            {
                var multichild = (MultichildExpression)Visit(context.with_table_hints());
                multichild.Expressions.Add(result);
                result = multichild;
            }
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitRowset_function([NotNull] tsqlParser.Rowset_functionContext context)
        {
            return CreateSpecialInvocation(context.OPENROWSET(), context,
                context.bulk_option().Select(opt => (Expression)Visit(opt)).ToList());
        }

        /// <returns><see cref="AssignmentExpression"/></returns>
        public UstNode VisitBulk_option([NotNull] tsqlParser.Bulk_optionContext context)
        {
            var left = (IdToken)Visit(context.id());
            var right = ExtractLiteral(context.bulk_option_value);
            var result = new AssignmentExpression(left, right, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitDerived_table([NotNull] tsqlParser.Derived_tableContext context)
        {
            Expression result = (WrapperExpression)Visit(context.subquery());
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitChange_table([NotNull] tsqlParser.Change_tableContext context)
        {
            var tableName = (IdToken)Visit(context.table_name());
            var result = CreateSpecialInvocation(context.CHANGETABLE(), context, tableName);
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitFunction_call([NotNull] tsqlParser.Function_callContext context)
        {
            InvocationExpression result = null;
            if (context.ranking_windowed_function() != null)
            {
                result = (InvocationExpression)Visit(context.ranking_windowed_function());
            }
            else if (context.aggregate_windowed_function() != null)
            {
                result = (InvocationExpression)Visit(context.aggregate_windowed_function());
            }
            else if (context.scalar_function_name() != null)
            {
                var target = (Expression)Visit(context.scalar_function_name());
                var args = GetArgsNode(context.expression_list());
                result = new InvocationExpression(target, args, context.GetTextSpan(), FileNode);
            }
            else
            {
                var exprs = new List<Expression>();
                exprs.AddRange(context.expression().Select(expr => (Expression)Visit(expr)));
                if (context.expression_list() != null)
                {
                    exprs.AddRange(GetArgsNode(context.expression_list()).Collection);
                }
                if (context.data_type() != null)
                {
                    exprs.Add((TypeToken)Visit(context.data_type()));
                }
                if (context.seed != null)
                {
                    exprs.Add(ExtractLiteral(context.seed));
                }
                if (context.increment != null)
                {
                    exprs.Add(ExtractLiteral(context.increment));
                }
                result = CreateSpecialInvocation(context.GetChild<ITerminalNode>(0), context, exprs);
            }
            return result;
        }

        /// <returns><see cref="SwitchSection"/></returns>
        public UstNode VisitSwitch_section([NotNull] tsqlParser.Switch_sectionContext context)
        {
            var caseLabels = new Expression[] { (Expression)Visit(context.expression(0)) };
            var statements = new Statement[] { new ExpressionStatement(
                (Expression)Visit(context.expression(1))) };
            var result = new SwitchSection(caseLabels, statements, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="SwitchSection"/></returns>
        public UstNode VisitSwitch_search_condition_section([NotNull] tsqlParser.Switch_search_condition_sectionContext context)
        {
            var caseLabels = new Expression[] { (Expression)Visit(context.search_condition()) };
            var statements = new Statement[] { new ExpressionStatement(
                (Expression)Visit(context.expression())) };
            var result = new SwitchSection(caseLabels, statements, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitAs_table_alias([NotNull] tsqlParser.As_table_aliasContext context)
        {
            return (Expression)Visit(context.table_alias());
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitTable_alias([NotNull] tsqlParser.Table_aliasContext context)
        {
            Expression result = (IdToken)Visit(context.id());
            if (context.with_table_hints() != null)
            {
                var multichild = (MultichildExpression)Visit(context.with_table_hints());
                multichild.Expressions.Add(result);
                result = multichild;
            }
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitWith_table_hints([NotNull] tsqlParser.With_table_hintsContext context)
        {
            var exprs = context.table_hint().Select(hint => (Expression)Visit(hint)).ToList();
            var result = new MultichildExpression(exprs, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitTable_hint([NotNull] tsqlParser.Table_hintContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitIndex_value([NotNull] tsqlParser.Index_valueContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="MultichildExpression"/></returns>
        public UstNode VisitColumn_alias_list([NotNull] tsqlParser.Column_alias_listContext context)
        {
            var exprs = context.column_alias().Select(column_alias => (Token)Visit(column_alias)).ToArray();
            return new MultichildExpression(exprs, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitColumn_alias([NotNull] tsqlParser.Column_aliasContext context)
        {
            Token result;
            if (context.id() != null)
            {
                result = (Token)Visit(context.id());
            }
            else
            {
                result = (Token)Visit(context.STRING());
            }
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitTable_value_constructor([NotNull] tsqlParser.Table_value_constructorContext context)
        {
            var args1 = GetArgsNode(context.expression_list(0));
            var args2 = GetArgsNode(context.expression_list(1));
            var newArgs = new ArgsNode(new WrapperExpression(args1), new WrapperExpression(args2));

            var name = new IdToken(context.VALUES().GetText().ToLowerInvariant(), context.VALUES().GetTextSpan(), FileNode);
            var result = new InvocationExpression(name, newArgs, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitExpression_list([NotNull] tsqlParser.Expression_listContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitRanking_windowed_function([NotNull] tsqlParser.Ranking_windowed_functionContext context)
        {
            var exprs = new List<Expression>();
            if (context.expression() != null)
            {
                exprs.Add((Expression)Visit(context.expression()));
            }
            exprs.Add((Expression)Visit(context.over_clause()));

            InvocationExpression result = CreateSpecialInvocation(context.GetChild<ITerminalNode>(0), context, exprs);
            return result;
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitAggregate_windowed_function([NotNull] tsqlParser.Aggregate_windowed_functionContext context)
        {
            var terminal = context.GetChild<ITerminalNode>(0);
            var functionName = new IdToken(terminal.GetText(), terminal.GetTextSpan(), FileNode);

            var exprs = new List<Expression>();
            if (context.all_distinct_expression() != null)
            {
                exprs.Add((Expression)Visit(context.all_distinct_expression()));
            }
            if (context.over_clause() != null)
            {
                exprs.Add((Expression)Visit(context.over_clause()));
            }
            if (context.expression() != null)
            {
                exprs.Add((Expression)Visit(context.expression()));
            }
            if (context.expression_list() != null)
            {
                exprs.Add(new WrapperExpression(GetArgsNode(context.expression_list())));
            }

            InvocationExpression result = CreateSpecialInvocation(context.GetChild<ITerminalNode>(0), context, exprs);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitAll_distinct_expression([NotNull] tsqlParser.All_distinct_expressionContext context)
        {
            return Visit(context.expression());
        }

        /// <returns><see cref="InvocationExpression"/></returns>
        public UstNode VisitOver_clause([NotNull] tsqlParser.Over_clauseContext context)
        {
            var exprs = new List<Expression>();
            if (context.expression_list() != null)
            {
                context.expression_list().expression().Select(expr => (Expression)Visit(expr)).ToList();
            }

            if (context.order_by_clause() != null)
            {
                exprs.Add((Expression)Visit(context.order_by_clause()));
            }

            string rowOrRangeClause;
            if (context.row_or_range_clause() != null)
            {
                rowOrRangeClause = context.row_or_range_clause().GetText();
                exprs.Add(new IdToken(rowOrRangeClause, context.row_or_range_clause().GetTextSpan(), FileNode));
            }

            var functionName = new IdToken(context.OVER().GetText(), context.OVER().GetTextSpan(), FileNode);
            var result = new InvocationExpression(functionName, new ArgsNode(exprs), context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitRow_or_range_clause([NotNull] tsqlParser.Row_or_range_clauseContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitWindow_frame_extent([NotNull] tsqlParser.Window_frame_extentContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitWindow_frame_bound([NotNull] tsqlParser.Window_frame_boundContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitWindow_frame_preceding([NotNull] tsqlParser.Window_frame_precedingContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitWindow_frame_following([NotNull] tsqlParser.Window_frame_followingContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        #endregion

        #region Primitive

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitFull_table_name([NotNull] tsqlParser.Full_table_nameContext context)
        {
            return new IdToken(RemoveSpaces(context.GetText()), context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitTable_name([NotNull] tsqlParser.Table_nameContext context)
        {
            // (database=id '.' (schema=id)? '.' | schema=id '.')? table=id
            return new IdToken(RemoveSpaces(context.GetText()), context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitSimple_name([NotNull] tsqlParser.Simple_nameContext context)
        {
            return new IdToken(RemoveSpaces(context.GetText()), context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitFunc_proc_name([NotNull] tsqlParser.Func_proc_nameContext context)
        {
            return new IdToken(RemoveSpaces(context.GetText()), context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitDdl_object([NotNull] tsqlParser.Ddl_objectContext context)
        {
            Token result;
            if (context.full_table_name() != null)
            {
                result = (Token)Visit(context.full_table_name());
            }
            else
            {
                result = (Token)Visit(context.LOCAL_ID());
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitFull_column_name([NotNull] tsqlParser.Full_column_nameContext context)
        {
            if (context.table_name() != null)
            {
                return new MemberReferenceExpression(
                    (Expression)Visit(context.table_name()),
                    (Expression)Visit(context.id()), context.GetTextSpan(), FileNode);
            }
            else
            {
                return Visit(context.id());
            }
        }

        /// <returns><see cref="ArgsNode"/></returns>
        public UstNode VisitColumn_name_list([NotNull] tsqlParser.Column_name_listContext context)
        {
            var result = new ArgsNode(context.id().Select(id => (IdToken)Visit(id)).ToArray());
            return result;
        }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitCursor_name([NotNull] tsqlParser.Cursor_nameContext context)
        {
            Token result;
            if (context.id() != null)
            {
                result = (IdToken)Visit(context.id());
            }
            else
            {
                result = (Token)Visit(context.LOCAL_ID());
            }
            return result;
        }

        /// <returns><see cref="BooleanLiteral"/></returns>
        public UstNode VisitOn_off([NotNull] tsqlParser.On_offContext context)
        {
            var text = context.GetText().ToLowerInvariant();
            return new BooleanLiteral(text == "on", context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitClustered([NotNull] tsqlParser.ClusteredContext context)
        {
            var result = new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitNull_notnull([NotNull] tsqlParser.Null_notnullContext context)
        {
            Expression result = new NullLiteral(context.NULL().GetTextSpan(), FileNode);
            if (context.NOT() != null)
            {
                var literal = new UnaryOperatorLiteral(UnaryOperator.Not, context.GetTextSpan(), FileNode);
                result = new UnaryOperatorExpression(literal, result, context.GetTextSpan(), FileNode);
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitScalar_function_name([NotNull] tsqlParser.Scalar_function_nameContext context)
        {
            if (context.func_proc_name() != null)
            {
                return Visit(context.func_proc_name());
            }
            else
            {
                return new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
            }
        }

        /// <returns><see cref="TypeToken"/></returns>
        public UstNode VisitData_type([NotNull] tsqlParser.Data_typeContext context)
        {
            var resultType = RemoveSpaces(context.GetText());
            return new TypeToken(resultType, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitDefault_value([NotNull] tsqlParser.Default_valueContext context)
        {
            Token result;
            if (context.NULL() != null)
            {
                result = new NullLiteral(context.GetTextSpan(), FileNode);
            }
            else
            {
                result = (Token)Visit(context.constant());
            }
            return result;
        }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitConstant([NotNull] tsqlParser.ConstantContext context)
        {
            var text = context.GetText();
            var textSpan = context.GetTextSpan();
            Token result;
            if (context.STRING() != null)
            {
                if (text.StartsWith("N"))
                {
                    text = text.Substring(1);
                }
                result = new StringLiteral(text.Substring(1, text.Length - 2), textSpan, FileNode);
            }
            else if (context.BINARY() != null)
            {
                result = new IntLiteral(Convert.ToInt64(text.Substring(2), 16), textSpan, FileNode);
            }
            else if (context.dollar != null)
            {
                result = new StringLiteral(text, textSpan, FileNode);
            }
            else if (context.DECIMAL() != null)
            {
                result = new IntLiteral(long.Parse(text), textSpan, FileNode);
            }
            else
            {
                result = new FloatLiteral(double.Parse(text), textSpan, FileNode);
            }
            return result;
        }
        
        public UstNode VisitSign([NotNull] tsqlParser.SignContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitId([NotNull] tsqlParser.IdContext context)
        { 
            string id;
            if (context.simple_id() != null)
            {
                id = context.simple_id().GetText();
            }
            else
            {
                id = context.GetText();
                id = id.Substring(1, id.Length - 2);
            }
            return new IdToken(id, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitSimple_id([NotNull] tsqlParser.Simple_idContext context)
        {
            return new IdToken(context.GetText(), context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public UstNode VisitComparison_operator([NotNull] tsqlParser.Comparison_operatorContext context)
        {
            var opText = RemoveSpaces(context.GetText());
            if (opText == "=")
            {
                opText = "==";
            }
            else if (opText == "<>")
            {
                opText = "!=";
            }
            var result = new BinaryOperatorLiteral(opText, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public UstNode VisitAssignment_operator([NotNull] tsqlParser.Assignment_operatorContext context)
        {
            var result = new BinaryOperatorLiteral(RemoveSpaces(context.GetText()), context.GetTextSpan(), FileNode);
            return result;
        }

        #endregion

        #region Without implementation

        public UstNode VisitCreate_database([NotNull] tsqlParser.Create_databaseContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitAuto_option([NotNull] tsqlParser.Auto_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitChange_tracking_option([NotNull] tsqlParser.Change_tracking_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitChange_tracking_option_list([NotNull] tsqlParser.Change_tracking_option_listContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitContainment_option([NotNull] tsqlParser.Containment_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitCursor_option([NotNull] tsqlParser.Cursor_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDate_correlation_optimization_option([NotNull] tsqlParser.Date_correlation_optimization_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDb_encryption_option([NotNull] tsqlParser.Db_encryption_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDb_state_option([NotNull] tsqlParser.Db_state_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDb_update_option([NotNull] tsqlParser.Db_update_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDb_user_access_option([NotNull] tsqlParser.Db_user_access_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDelayed_durability_option([NotNull] tsqlParser.Delayed_durability_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitExternal_access_option([NotNull] tsqlParser.External_access_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitMixed_page_allocation_option([NotNull] tsqlParser.Mixed_page_allocation_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitParameterization_option([NotNull] tsqlParser.Parameterization_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitRecovery_option([NotNull] tsqlParser.Recovery_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitService_broker_option([NotNull] tsqlParser.Service_broker_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitSnapshot_option([NotNull] tsqlParser.Snapshot_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitSql_option([NotNull] tsqlParser.Sql_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTarget_recovery_time_option([NotNull] tsqlParser.Target_recovery_time_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTermination([NotNull] tsqlParser.TerminationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitCreate_type([NotNull] tsqlParser.Create_typeContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDrop_type([NotNull] tsqlParser.Drop_typeContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitCreate_database_option([NotNull] tsqlParser.Create_database_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDatabase_filestream_option([NotNull] tsqlParser.Database_filestream_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDatabase_file_spec([NotNull] tsqlParser.Database_file_specContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFile_group([NotNull] tsqlParser.File_groupContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFile_spec([NotNull] tsqlParser.File_specContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFile_size([NotNull] tsqlParser.File_sizeContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitEmpty_statement([NotNull] tsqlParser.Empty_statementContext context)
        {
            return new EmptyStatement(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitCreate_or_alter_function([NotNull] tsqlParser.Create_or_alter_functionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFunc_body_returns_select([NotNull] tsqlParser.Func_body_returns_selectContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFunc_body_returns_table([NotNull] tsqlParser.Func_body_returns_tableContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFunc_body_returns_scalar([NotNull] tsqlParser.Func_body_returns_scalarContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFunction_option([NotNull] tsqlParser.Function_optionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDrop_function([NotNull] tsqlParser.Drop_functionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDbcc_clause([NotNull] tsqlParser.Dbcc_clauseContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDbcc_options([NotNull] tsqlParser.Dbcc_optionsContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTop_clause([NotNull] tsqlParser.Top_clauseContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTop_percent([NotNull] tsqlParser.Top_percentContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTop_count([NotNull] tsqlParser.Top_countContext context)
        {
            return VisitChildren(context);
        }

        #endregion
    }
}
