using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.TypeMembers;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.PlSqlParseTreeUst;
using System;

namespace PT.PM.SqlParseTreeUst
{
    public partial class PlSqlConverterVisitor : AntlrDefaultVisitor, IPlSqlParserVisitor<UstNode>
    {
        public PlSqlConverterVisitor(string fileName, string fileData)
            : base(fileName, fileData)
        {
            FileNode = new FileNode(fileName, fileData);
        }

        /// <returns><see cref="ArgsNode"/></returns>
        public UstNode VisitSwallow_to_semi([NotNull] PlSqlParser.Swallow_to_semiContext context)
        {
            Expression[] args = context.children.Select(c => (Expression)Visit(c)).ToArray();
            var result = new ArgsNode(args, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitCompilation_unit([NotNull] PlSqlParser.Compilation_unitContext context)
        {
            var roots = context.unit_statement().Select(statement => (Statement)Visit(statement)).ToArray();
            FileNode.Root = roots.CreateRootNamespace(Language.PlSql, FileNode);
            return FileNode;
        }

        public UstNode VisitSql_script([NotNull] PlSqlParser.Sql_scriptContext context) { return VisitChildren(context); }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitUnit_statement([NotNull] PlSqlParser.Unit_statementContext context)
        {
            var child = Visit(context.GetChild(0));
            Statement result = child.ToStatementIfRequired();
            return result;
        }

        public UstNode VisitDrop_function([NotNull] PlSqlParser.Drop_functionContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_function([NotNull] PlSqlParser.Alter_functionContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_function_body([NotNull] PlSqlParser.Create_function_bodyContext context)
        {
            var name = (IdToken)Visit(context.function_name());
            ParameterDeclaration[] parameters = context.parameter().Select(p => (ParameterDeclaration)Visit(p)).ToArray();
            var body = (BlockStatement)Visit(context.body());

            var result = new MethodDeclaration(name, parameters, body, context.GetTextSpan(), FileNode);
            result.ReturnType = (TypeToken)Visit(context.type_spec());
            return result;
        }

        public UstNode VisitParallel_enable_clause([NotNull] PlSqlParser.Parallel_enable_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPartition_by_clause([NotNull] PlSqlParser.Partition_by_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitResult_cache_clause([NotNull] PlSqlParser.Result_cache_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitRelies_on_part([NotNull] PlSqlParser.Relies_on_partContext context) { return VisitChildren(context); }

        public UstNode VisitStreaming_clause([NotNull] PlSqlParser.Streaming_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitDrop_package([NotNull] PlSqlParser.Drop_packageContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_package([NotNull] PlSqlParser.Alter_packageContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_package([NotNull] PlSqlParser.Create_packageContext context) { return VisitChildren(context); }

        public UstNode VisitPackage_obj_spec([NotNull] PlSqlParser.Package_obj_specContext context) { return VisitChildren(context); }

        public UstNode VisitProcedure_spec([NotNull] PlSqlParser.Procedure_specContext context) { return VisitChildren(context); }

        public UstNode VisitFunction_spec([NotNull] PlSqlParser.Function_specContext context) { return VisitChildren(context); }

        public UstNode VisitPackage_obj_body([NotNull] PlSqlParser.Package_obj_bodyContext context) { return VisitChildren(context); }

        public UstNode VisitDrop_procedure([NotNull] PlSqlParser.Drop_procedureContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_procedure([NotNull] PlSqlParser.Alter_procedureContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_procedure_body([NotNull] PlSqlParser.Create_procedure_bodyContext context)
        {
            var name = (IdToken)Visit(context.procedure_name());
            ParameterDeclaration[] parameters = context.parameter().Select(p => (ParameterDeclaration)Visit(p)).ToArray();
            var body = (BlockStatement)Visit(context.body());

            var result = new MethodDeclaration(name, parameters, body, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitDrop_trigger([NotNull] PlSqlParser.Drop_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_trigger([NotNull] PlSqlParser.Alter_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_trigger([NotNull] PlSqlParser.Create_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitTrigger_follows_clause([NotNull] PlSqlParser.Trigger_follows_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitTrigger_when_clause([NotNull] PlSqlParser.Trigger_when_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSimple_dml_trigger([NotNull] PlSqlParser.Simple_dml_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitFor_each_row([NotNull] PlSqlParser.For_each_rowContext context) { return VisitChildren(context); }

        public UstNode VisitCompound_dml_trigger([NotNull] PlSqlParser.Compound_dml_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitNon_dml_trigger([NotNull] PlSqlParser.Non_dml_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitTrigger_body([NotNull] PlSqlParser.Trigger_bodyContext context) { return VisitChildren(context); }

        public UstNode VisitRoutine_clause([NotNull] PlSqlParser.Routine_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCompound_trigger_block([NotNull] PlSqlParser.Compound_trigger_blockContext context) { return VisitChildren(context); }

        public UstNode VisitTiming_point_section([NotNull] PlSqlParser.Timing_point_sectionContext context) { return VisitChildren(context); }

        public UstNode VisitNon_dml_event([NotNull] PlSqlParser.Non_dml_eventContext context) { return VisitChildren(context); }

        public UstNode VisitDml_event_clause([NotNull] PlSqlParser.Dml_event_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitDml_event_element([NotNull] PlSqlParser.Dml_event_elementContext context) { return VisitChildren(context); }

        public UstNode VisitDml_event_nested_clause([NotNull] PlSqlParser.Dml_event_nested_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitReferencing_clause([NotNull] PlSqlParser.Referencing_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitReferencing_element([NotNull] PlSqlParser.Referencing_elementContext context) { return VisitChildren(context); }

        public UstNode VisitDrop_type([NotNull] PlSqlParser.Drop_typeContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_type([NotNull] PlSqlParser.Alter_typeContext context) { return VisitChildren(context); }

        public UstNode VisitCompile_type_clause([NotNull] PlSqlParser.Compile_type_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitReplace_type_clause([NotNull] PlSqlParser.Replace_type_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_method_spec([NotNull] PlSqlParser.Alter_method_specContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_method_element([NotNull] PlSqlParser.Alter_method_elementContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_attribute_definition([NotNull] PlSqlParser.Alter_attribute_definitionContext context) { return VisitChildren(context); }

        public UstNode VisitAttribute_definition([NotNull] PlSqlParser.Attribute_definitionContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_collection_clauses([NotNull] PlSqlParser.Alter_collection_clausesContext context) { return VisitChildren(context); }

        public UstNode VisitDependent_handling_clause([NotNull] PlSqlParser.Dependent_handling_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitDependent_exceptions_part([NotNull] PlSqlParser.Dependent_exceptions_partContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_type([NotNull] PlSqlParser.Create_typeContext context) { return VisitChildren(context); }

        public UstNode VisitType_definition([NotNull] PlSqlParser.Type_definitionContext context) { return VisitChildren(context); }

        public UstNode VisitObject_type_def([NotNull] PlSqlParser.Object_type_defContext context) { return VisitChildren(context); }

        public UstNode VisitObject_as_part([NotNull] PlSqlParser.Object_as_partContext context) { return VisitChildren(context); }

        public UstNode VisitObject_under_part([NotNull] PlSqlParser.Object_under_partContext context) { return VisitChildren(context); }

        public UstNode VisitNested_table_type_def([NotNull] PlSqlParser.Nested_table_type_defContext context) { return VisitChildren(context); }

        public UstNode VisitSqlj_object_type([NotNull] PlSqlParser.Sqlj_object_typeContext context) { return VisitChildren(context); }

        public UstNode VisitType_body([NotNull] PlSqlParser.Type_bodyContext context) { return VisitChildren(context); }

        public UstNode VisitType_body_elements([NotNull] PlSqlParser.Type_body_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitMap_order_func_declaration([NotNull] PlSqlParser.Map_order_func_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitSubprog_decl_in_type([NotNull] PlSqlParser.Subprog_decl_in_typeContext context) { return VisitChildren(context); }

        public UstNode VisitProc_decl_in_type([NotNull] PlSqlParser.Proc_decl_in_typeContext context) { return VisitChildren(context); }

        public UstNode VisitFunc_decl_in_type([NotNull] PlSqlParser.Func_decl_in_typeContext context) { return VisitChildren(context); }

        public UstNode VisitConstructor_declaration([NotNull] PlSqlParser.Constructor_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitModifier_clause([NotNull] PlSqlParser.Modifier_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitObject_member_spec([NotNull] PlSqlParser.Object_member_specContext context) { return VisitChildren(context); }

        public UstNode VisitSqlj_object_type_attr([NotNull] PlSqlParser.Sqlj_object_type_attrContext context) { return VisitChildren(context); }

        public UstNode VisitElement_spec([NotNull] PlSqlParser.Element_specContext context) { return VisitChildren(context); }

        public UstNode VisitElement_spec_options([NotNull] PlSqlParser.Element_spec_optionsContext context) { return VisitChildren(context); }

        public UstNode VisitSubprogram_spec([NotNull] PlSqlParser.Subprogram_specContext context) { return VisitChildren(context); }

        public UstNode VisitType_procedure_spec([NotNull] PlSqlParser.Type_procedure_specContext context) { return VisitChildren(context); }

        public UstNode VisitType_function_spec([NotNull] PlSqlParser.Type_function_specContext context) { return VisitChildren(context); }

        public UstNode VisitConstructor_spec([NotNull] PlSqlParser.Constructor_specContext context) { return VisitChildren(context); }

        public UstNode VisitMap_order_function_spec([NotNull] PlSqlParser.Map_order_function_specContext context) { return VisitChildren(context); }

        public UstNode VisitPragma_clause([NotNull] PlSqlParser.Pragma_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPragma_elements([NotNull] PlSqlParser.Pragma_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitType_elements_parameter([NotNull] PlSqlParser.Type_elements_parameterContext context) { return VisitChildren(context); }

        public UstNode VisitDrop_sequence([NotNull] PlSqlParser.Drop_sequenceContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_sequence([NotNull] PlSqlParser.Alter_sequenceContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_sequence([NotNull] PlSqlParser.Create_sequenceContext context) { return VisitChildren(context); }

        public UstNode VisitSequence_spec([NotNull] PlSqlParser.Sequence_specContext context) { return VisitChildren(context); }

        public UstNode VisitSequence_start_clause([NotNull] PlSqlParser.Sequence_start_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitInvoker_rights_clause([NotNull] PlSqlParser.Invoker_rights_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCompiler_parameters_clause([NotNull] PlSqlParser.Compiler_parameters_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCall_spec([NotNull] PlSqlParser.Call_specContext context) { return VisitChildren(context); }

        public UstNode VisitJava_spec([NotNull] PlSqlParser.Java_specContext context) { return VisitChildren(context); }

        public UstNode VisitC_spec([NotNull] PlSqlParser.C_specContext context) { return VisitChildren(context); }

        public UstNode VisitC_agent_in_clause([NotNull] PlSqlParser.C_agent_in_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitC_parameters_clause([NotNull] PlSqlParser.C_parameters_clauseContext context) { return VisitChildren(context); }

        /// <returns><see cref="ParameterDeclaration"/></returns>
        public UstNode VisitParameter([NotNull] PlSqlParser.ParameterContext context)
        {
            ParameterDeclaration result;
            TypeToken typeToken = null;
            if (context.type_spec() != null)
            {
                typeToken = (TypeToken)Visit(context.type_spec());
            }
            var name = (IdToken)Visit(context.parameter_name());
            result = new ParameterDeclaration(typeToken, name, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitDefault_value_part([NotNull] PlSqlParser.Default_value_partContext context) { return VisitChildren(context); }

        public UstNode VisitDeclare_spec([NotNull] PlSqlParser.Declare_specContext context) { return VisitChildren(context); }

        public UstNode VisitVariable_declaration([NotNull] PlSqlParser.Variable_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitSubtype_declaration([NotNull] PlSqlParser.Subtype_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitCursor_declaration([NotNull] PlSqlParser.Cursor_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitParameter_spec([NotNull] PlSqlParser.Parameter_specContext context) { return VisitChildren(context); }

        public UstNode VisitException_declaration([NotNull] PlSqlParser.Exception_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitPragma_declaration([NotNull] PlSqlParser.Pragma_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitField_spec([NotNull] PlSqlParser.Field_specContext context) { return VisitChildren(context); }

        public UstNode VisitTable_indexed_by_part([NotNull] PlSqlParser.Table_indexed_by_partContext context) { return VisitChildren(context); }

        public UstNode VisitVarray_type_def([NotNull] PlSqlParser.Varray_type_defContext context) { return VisitChildren(context); }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitSeq_of_statements([NotNull] PlSqlParser.Seq_of_statementsContext context)
        {
            var result = new BlockStatement(context.statement().Select(s => (Statement)Visit(s)).ToArray(),
                context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitLabel_declaration([NotNull] PlSqlParser.Label_declarationContext context) { return VisitChildren(context); }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitStatement([NotNull] PlSqlParser.StatementContext context)
        {
            Statement result;
            if (context.CREATE() != null || context.ALTER() != null || context.GRANT() != null || context.TRUNCATE() != null)
            {
                string str = context.GetChild(0).GetText().ToLowerInvariant();
                if (context.ALL() != null)
                {
                    str += "_" + context.ALL().GetText().ToLowerInvariant();
                }
                var args = (ArgsNode)Visit(context.swallow_to_semi());
                var funcName = new IdToken(str, context.GetTextSpan(), FileNode);
                result = new ExpressionStatement(new InvocationExpression(funcName, args, context.GetTextSpan(), FileNode));
            }
            else
            {
                var node = Visit(context.GetChild(0));
                Statement resultStatement;
                Expression resultExpression;
                if ((resultStatement = node as Statement) != null)
                {
                    result = resultStatement;
                }
                else if ((resultExpression = node as Expression) != null)
                {
                    result = new ExpressionStatement(resultExpression);
                }
                else
                {
                    result = new WrapperStatement(node);
                }
            }
            return result;
        }

        public UstNode VisitAssignment_statement([NotNull] PlSqlParser.Assignment_statementContext context)
        {
            Expression left;
            if (context.general_element() != null)
            {
                left = (Expression)Visit(context.general_element());
            }
            else
            {
                left = (Expression)Visit(context.bind_variable());
            }
            var right = (Expression)Visit(context.expression());
            var result = new AssignmentExpression(left, right, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitContinue_statement([NotNull] PlSqlParser.Continue_statementContext context)
        {
            return new ContinueStatement(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitExit_statement([NotNull] PlSqlParser.Exit_statementContext context)
        {
            return new BreakStatement(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitGoto_statement([NotNull] PlSqlParser.Goto_statementContext context) { return VisitChildren(context); }

        /// <returns><see cref="IfElseStatement"/></returns>
        public UstNode VisitIf_statement([NotNull] PlSqlParser.If_statementContext context)
        {
            var condition = (Expression)Visit(context.condition());
            var block = (BlockStatement)Visit(context.seq_of_statements());
            var result = new IfElseStatement(condition, block, context.GetTextSpan(), FileNode);
            IfElseStatement stmt = result;
            for (int i = 0; i < context.elsif_part().Length; i++)
            {
                stmt.FalseStatement = (IfElseStatement)Visit(context.elsif_part(i));
                stmt = (IfElseStatement)stmt.FalseStatement;
            }
            if (context.else_part() != null)
            {
                stmt.FalseStatement = (Statement)Visit(context.else_part());
            }
            return result;
        }

        /// <returns><see cref="IfElseStatement"/></returns>
        public UstNode VisitElsif_part([NotNull] PlSqlParser.Elsif_partContext context)
        {
            var condition = (Expression)Visit(context.condition());
            var block = (BlockStatement)Visit(context.seq_of_statements());
            var result = new IfElseStatement(condition, block, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitElse_part([NotNull] PlSqlParser.Else_partContext context)
        {
            return (BlockStatement)Visit(context.seq_of_statements());
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitLoop_statement([NotNull] PlSqlParser.Loop_statementContext context)
        {
            Statement result;
            var block = (BlockStatement)Visit(context.seq_of_statements());
            var textSpan = context.GetTextSpan();
            if (context.WHILE() != null)
            {
                result = new WhileStatement((Expression)Visit(context.condition()), block, textSpan, FileNode);
            }
            else
            {
                var cursorLoopParam = context.cursor_loop_param();
                if (context.cursor_loop_param().lower_bound() != null)
                {
                    var varName = (IdToken)Visit(cursorLoopParam.index_name());
                    bool reverse = cursorLoopParam.REVERSE() != null;
                    var lowerBound = (Expression)Visit(cursorLoopParam.lower_bound());
                    var upperBound = (Expression)Visit(cursorLoopParam.upper_bound());
                    if (reverse)
                    {
                        var t = lowerBound;
                        lowerBound = upperBound;
                        upperBound = t;
                    }
                    var init = new ExpressionStatement(new AssignmentExpression(varName, lowerBound, varName.TextSpan.Union(lowerBound.TextSpan), FileNode));
                    var condition = new BinaryOperatorExpression(varName,
                        new BinaryOperatorLiteral(BinaryOperator.Less, cursorLoopParam.range.GetTextSpan(), FileNode), upperBound,
                        lowerBound.TextSpan.Union(upperBound.TextSpan), FileNode);
                    var iterator = new UnaryOperatorExpression(
                        new UnaryOperatorLiteral(UnaryOperator.PostIncrement, default(TextSpan), FileNode), varName,
                        cursorLoopParam.range.GetTextSpan(), FileNode);
                    result = new ForStatement(new Statement[] { init }, condition, new Expression[] { iterator },
                        block, textSpan, FileNode);
                }
                else
                {
                    return VisitChildren(context);
                }
            }
            return result;
        }

        public UstNode VisitCursor_loop_param([NotNull] PlSqlParser.Cursor_loop_paramContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitForall_statement([NotNull] PlSqlParser.Forall_statementContext context) { return VisitChildren(context); }

        public UstNode VisitBounds_clause([NotNull] PlSqlParser.Bounds_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitBetween_bound([NotNull] PlSqlParser.Between_boundContext context) { return VisitChildren(context); }

        public UstNode VisitLower_bound([NotNull] PlSqlParser.Lower_boundContext context) { return VisitChildren(context); }

        public UstNode VisitUpper_bound([NotNull] PlSqlParser.Upper_boundContext context) { return VisitChildren(context); }

        public UstNode VisitNull_statement([NotNull] PlSqlParser.Null_statementContext context)
        {
            return new NullLiteral(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitRaise_statement([NotNull] PlSqlParser.Raise_statementContext context) { return VisitChildren(context); }

        public UstNode VisitReturn_statement([NotNull] PlSqlParser.Return_statementContext context)
        {
            Expression returnExpression = null;
            if (context.expression() != null)
            {
                returnExpression = (Expression)Visit(context.expression());
            }
            var result = new ReturnStatement(returnExpression, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitFunction_call([NotNull] PlSqlParser.Function_callContext context)
        {
            var target = (Expression)Visit(context.routine_name());
            ArgsNode args;
            if (context.function_argument() != null)
            {
                args = (ArgsNode)Visit(context.function_argument());
            }
            else
            {
                args = new ArgsNode();
            }
            var result = new InvocationExpression(target, args, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitBody([NotNull] PlSqlParser.BodyContext context)
        {
            BlockStatement result;
            var block = (BlockStatement)Visit(context.seq_of_statements());
            if (context.exception_handler().Length > 0)
            {
                var tryCatch = new TryCatchStatement(block, context.GetTextSpan(), FileNode);
                tryCatch.CatchClauses = context.exception_handler().Select(handler =>
                    (CatchClause)Visit(handler)).ToList();
                result = new BlockStatement(new Statement[] { tryCatch }, context.GetTextSpan(), FileNode);
            }
            else
            {
                result = block;
            }
            return result;
        }

        /// <returns><see cref="CatchClause"/></returns>
        public UstNode VisitException_handler([NotNull] PlSqlParser.Exception_handlerContext context)
        {
            TypeToken type;
            if (context.exception_name().Length == 1 && context.exception_name(0).GetText().ToLowerInvariant() == "others")
            {
                type = null;
            }
            else
            {
                type = (TypeToken)Visit(context.exception_name(0));
            }
            var body = (BlockStatement)Visit(context.seq_of_statements());
            if (body.Statements.Count == 1 &&
                body.Statements[0].NodeType == NodeType.ExpressionStatement &&
                ((ExpressionStatement)body.Statements[0]).Expression.NodeType == NodeType.NullLiteral)
            {
                body = new BlockStatement(new Statement[0], context.seq_of_statements().GetTextSpan(), FileNode);
            }
            var result = new CatchClause(type, null, body, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitTrigger_block([NotNull] PlSqlParser.Trigger_blockContext context) { return VisitChildren(context); }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitBlock([NotNull] PlSqlParser.BlockContext context)
        {
            var declare = new BlockStatement(context.declare_spec().Select(s => Visit(s).ToStatementIfRequired()).ToArray(),
                context.GetTextSpan(), FileNode);
            var body = (BlockStatement)Visit(context.body());

            var result = new BlockStatement(new Statement[] { declare, body }, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitSql_statement([NotNull] PlSqlParser.Sql_statementContext context) { return VisitChildren(context); }

        public UstNode VisitExecute_immediate([NotNull] PlSqlParser.Execute_immediateContext context) { return VisitChildren(context); }

        public UstNode VisitDynamic_returning_clause([NotNull] PlSqlParser.Dynamic_returning_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitData_manipulation_language_statements([NotNull] PlSqlParser.Data_manipulation_language_statementsContext context) { return VisitChildren(context); }

        public UstNode VisitCursor_manipulation_statements([NotNull] PlSqlParser.Cursor_manipulation_statementsContext context) { return VisitChildren(context); }

        public UstNode VisitClose_statement([NotNull] PlSqlParser.Close_statementContext context) { return VisitChildren(context); }

        public UstNode VisitOpen_statement([NotNull] PlSqlParser.Open_statementContext context) { return VisitChildren(context); }

        public UstNode VisitFetch_statement([NotNull] PlSqlParser.Fetch_statementContext context) { return VisitChildren(context); }

        public UstNode VisitOpen_for_statement([NotNull] PlSqlParser.Open_for_statementContext context) { return VisitChildren(context); }

        public UstNode VisitTransaction_control_statements([NotNull] PlSqlParser.Transaction_control_statementsContext context) { return VisitChildren(context); }

        public UstNode VisitSet_transaction_command([NotNull] PlSqlParser.Set_transaction_commandContext context) { return VisitChildren(context); }

        public UstNode VisitSet_constraint_command([NotNull] PlSqlParser.Set_constraint_commandContext context) { return VisitChildren(context); }

        public UstNode VisitCommit_statement([NotNull] PlSqlParser.Commit_statementContext context) { return VisitChildren(context); }

        public UstNode VisitWrite_clause([NotNull] PlSqlParser.Write_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitRollback_statement([NotNull] PlSqlParser.Rollback_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSavepoint_statement([NotNull] PlSqlParser.Savepoint_statementContext context) { return VisitChildren(context); }

        public UstNode VisitExplain_statement([NotNull] PlSqlParser.Explain_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSelect_statement([NotNull] PlSqlParser.Select_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery_factoring_clause([NotNull] PlSqlParser.Subquery_factoring_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitFactoring_element([NotNull] PlSqlParser.Factoring_elementContext context) { return VisitChildren(context); }

        public UstNode VisitSearch_clause([NotNull] PlSqlParser.Search_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCycle_clause([NotNull] PlSqlParser.Cycle_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery([NotNull] PlSqlParser.SubqueryContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery_operation_part([NotNull] PlSqlParser.Subquery_operation_partContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery_basic_elements([NotNull] PlSqlParser.Subquery_basic_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitQuery_block([NotNull] PlSqlParser.Query_blockContext context) { return VisitChildren(context); }

        public UstNode VisitSelected_element([NotNull] PlSqlParser.Selected_elementContext context) { return VisitChildren(context); }

        public UstNode VisitFrom_clause([NotNull] PlSqlParser.From_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSelect_list_elements([NotNull] PlSqlParser.Select_list_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitTable_ref_list([NotNull] PlSqlParser.Table_ref_listContext context) { return VisitChildren(context); }

        public UstNode VisitTable_ref([NotNull] PlSqlParser.Table_refContext context) { return VisitChildren(context); }

        public UstNode VisitTable_ref_aux([NotNull] PlSqlParser.Table_ref_auxContext context) { return VisitChildren(context); }

        public UstNode VisitJoin_clause([NotNull] PlSqlParser.Join_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitJoin_on_part([NotNull] PlSqlParser.Join_on_partContext context) { return VisitChildren(context); }

        public UstNode VisitJoin_using_part([NotNull] PlSqlParser.Join_using_partContext context) { return VisitChildren(context); }

        public UstNode VisitOuter_join_type([NotNull] PlSqlParser.Outer_join_typeContext context) { return VisitChildren(context); }

        public UstNode VisitQuery_partition_clause([NotNull] PlSqlParser.Query_partition_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitFlashback_query_clause([NotNull] PlSqlParser.Flashback_query_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_clause([NotNull] PlSqlParser.Pivot_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_element([NotNull] PlSqlParser.Pivot_elementContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_for_clause([NotNull] PlSqlParser.Pivot_for_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_in_clause([NotNull] PlSqlParser.Pivot_in_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_in_clause_element([NotNull] PlSqlParser.Pivot_in_clause_elementContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_in_clause_elements([NotNull] PlSqlParser.Pivot_in_clause_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitUnpivot_clause([NotNull] PlSqlParser.Unpivot_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitUnpivot_in_clause([NotNull] PlSqlParser.Unpivot_in_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitUnpivot_in_elements([NotNull] PlSqlParser.Unpivot_in_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitHierarchical_query_clause([NotNull] PlSqlParser.Hierarchical_query_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitStart_part([NotNull] PlSqlParser.Start_partContext context) { return VisitChildren(context); }

        public UstNode VisitGroup_by_clause([NotNull] PlSqlParser.Group_by_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitGroup_by_elements([NotNull] PlSqlParser.Group_by_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitRollup_cube_clause([NotNull] PlSqlParser.Rollup_cube_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitGrouping_sets_clause([NotNull] PlSqlParser.Grouping_sets_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitGrouping_sets_elements([NotNull] PlSqlParser.Grouping_sets_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitHaving_clause([NotNull] PlSqlParser.Having_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitModel_clause([NotNull] PlSqlParser.Model_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCell_reference_options([NotNull] PlSqlParser.Cell_reference_optionsContext context) { return VisitChildren(context); }

        public UstNode VisitReturn_rows_clause([NotNull] PlSqlParser.Return_rows_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitReference_model([NotNull] PlSqlParser.Reference_modelContext context) { return VisitChildren(context); }

        public UstNode VisitMain_model([NotNull] PlSqlParser.Main_modelContext context) { return VisitChildren(context); }

        public UstNode VisitModel_column_clauses([NotNull] PlSqlParser.Model_column_clausesContext context) { return VisitChildren(context); }

        public UstNode VisitModel_column_partition_part([NotNull] PlSqlParser.Model_column_partition_partContext context) { return VisitChildren(context); }

        public UstNode VisitModel_column_list([NotNull] PlSqlParser.Model_column_listContext context) { return VisitChildren(context); }

        public UstNode VisitModel_column([NotNull] PlSqlParser.Model_columnContext context) { return VisitChildren(context); }

        public UstNode VisitModel_rules_clause([NotNull] PlSqlParser.Model_rules_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitModel_rules_part([NotNull] PlSqlParser.Model_rules_partContext context) { return VisitChildren(context); }

        public UstNode VisitModel_rules_element([NotNull] PlSqlParser.Model_rules_elementContext context) { return VisitChildren(context); }

        public UstNode VisitCell_assignment([NotNull] PlSqlParser.Cell_assignmentContext context) { return VisitChildren(context); }

        public UstNode VisitModel_iterate_clause([NotNull] PlSqlParser.Model_iterate_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitUntil_part([NotNull] PlSqlParser.Until_partContext context) { return VisitChildren(context); }

        public UstNode VisitOrder_by_clause([NotNull] PlSqlParser.Order_by_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitOrder_by_elements([NotNull] PlSqlParser.Order_by_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitFor_update_clause([NotNull] PlSqlParser.For_update_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitFor_update_of_part([NotNull] PlSqlParser.For_update_of_partContext context) { return VisitChildren(context); }

        public UstNode VisitFor_update_options([NotNull] PlSqlParser.For_update_optionsContext context) { return VisitChildren(context); }

        public UstNode VisitUpdate_statement([NotNull] PlSqlParser.Update_statementContext context) { return VisitChildren(context); }

        public UstNode VisitUpdate_set_clause([NotNull] PlSqlParser.Update_set_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitColumn_based_update_set_clause([NotNull] PlSqlParser.Column_based_update_set_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitDelete_statement([NotNull] PlSqlParser.Delete_statementContext context) { return VisitChildren(context); }

        public UstNode VisitInsert_statement([NotNull] PlSqlParser.Insert_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSingle_table_insert([NotNull] PlSqlParser.Single_table_insertContext context) { return VisitChildren(context); }

        public UstNode VisitMulti_table_insert([NotNull] PlSqlParser.Multi_table_insertContext context) { return VisitChildren(context); }

        public UstNode VisitMulti_table_element([NotNull] PlSqlParser.Multi_table_elementContext context) { return VisitChildren(context); }

        public UstNode VisitConditional_insert_clause([NotNull] PlSqlParser.Conditional_insert_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitConditional_insert_when_part([NotNull] PlSqlParser.Conditional_insert_when_partContext context) { return VisitChildren(context); }

        public UstNode VisitConditional_insert_else_part([NotNull] PlSqlParser.Conditional_insert_else_partContext context) { return VisitChildren(context); }

        public UstNode VisitInsert_into_clause([NotNull] PlSqlParser.Insert_into_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitValues_clause([NotNull] PlSqlParser.Values_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_statement([NotNull] PlSqlParser.Merge_statementContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_update_clause([NotNull] PlSqlParser.Merge_update_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_element([NotNull] PlSqlParser.Merge_elementContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_update_delete_part([NotNull] PlSqlParser.Merge_update_delete_partContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_insert_clause([NotNull] PlSqlParser.Merge_insert_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSelected_tableview([NotNull] PlSqlParser.Selected_tableviewContext context) { return VisitChildren(context); }

        public UstNode VisitLock_table_statement([NotNull] PlSqlParser.Lock_table_statementContext context) { return VisitChildren(context); }

        public UstNode VisitWait_nowait_part([NotNull] PlSqlParser.Wait_nowait_partContext context) { return VisitChildren(context); }

        public UstNode VisitLock_table_element([NotNull] PlSqlParser.Lock_table_elementContext context) { return VisitChildren(context); }

        public UstNode VisitLock_mode([NotNull] PlSqlParser.Lock_modeContext context) { return VisitChildren(context); }

        public UstNode VisitGeneral_table_ref([NotNull] PlSqlParser.General_table_refContext context) { return VisitChildren(context); }

        public UstNode VisitStatic_returning_clause([NotNull] PlSqlParser.Static_returning_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitError_logging_clause([NotNull] PlSqlParser.Error_logging_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitError_logging_into_part([NotNull] PlSqlParser.Error_logging_into_partContext context) { return VisitChildren(context); }

        public UstNode VisitError_logging_reject_part([NotNull] PlSqlParser.Error_logging_reject_partContext context) { return VisitChildren(context); }

        public UstNode VisitDml_table_expression_clause([NotNull] PlSqlParser.Dml_table_expression_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitTable_collection_expression([NotNull] PlSqlParser.Table_collection_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery_restriction_clause([NotNull] PlSqlParser.Subquery_restriction_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSample_clause([NotNull] PlSqlParser.Sample_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSeed_part([NotNull] PlSqlParser.Seed_partContext context) { return VisitChildren(context); }

        public UstNode VisitCursor_expression([NotNull] PlSqlParser.Cursor_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitExpression_list([NotNull] PlSqlParser.Expression_listContext context) { return VisitChildren(context); }

        public UstNode VisitCondition([NotNull] PlSqlParser.ConditionContext context) { return VisitChildren(context); }

        public UstNode VisitExpression([NotNull] PlSqlParser.ExpressionContext context) { return VisitChildren(context); }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitLogical_or_expression([NotNull] PlSqlParser.Logical_or_expressionContext context)
        {
            Expression result;
            if (context.OR() != null)
            {
                var left = (Expression)Visit(context.logical_or_expression());
                var right = (Expression)Visit(context.logical_and_expression());
                var opLiteral = new BinaryOperatorLiteral(BinaryOperator.LogicalOr, context.GetTextSpan(), FileNode);
                result = new BinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan(), FileNode);
            }
            else
            {
                result = (Expression)Visit(context.logical_and_expression());
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitLogical_and_expression([NotNull] PlSqlParser.Logical_and_expressionContext context)
        {
            Expression result;
            if (context.AND() != null)
            {
                var left = (Expression)Visit(context.negated_expression());
                var right = (Expression)Visit(context.logical_and_expression());
                var opLiteral = new BinaryOperatorLiteral(BinaryOperator.LogicalAnd, context.GetTextSpan(), FileNode);
                result = new BinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan(), FileNode);
            }
            else
            {
                result = (Expression)Visit(context.negated_expression());
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitNegated_expression([NotNull] PlSqlParser.Negated_expressionContext context)
        {
            Expression result;
            if (context.NOT() != null)
            {
                var opLiteral = new UnaryOperatorLiteral(UnaryOperator.Not, context.NOT().GetTextSpan(), FileNode);
                result = new UnaryOperatorExpression(opLiteral, (Expression)Visit(context.negated_expression()), context.GetTextSpan(), FileNode);
            }
            else
            {
                result = (Expression)Visit(context.equality_expression());
            }
            return result;
        }

        public UstNode VisitEquality_expression([NotNull] PlSqlParser.Equality_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitMultiset_expression([NotNull] PlSqlParser.Multiset_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitMultiset_type([NotNull] PlSqlParser.Multiset_typeContext context) { return VisitChildren(context); }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitRelational_expression([NotNull] PlSqlParser.Relational_expressionContext context)
        {
            Expression result;
            if (context.compound_expression() != null)
            {
                result = (Expression)Visit(context.compound_expression());
            }
            else
            {
                var left = (Expression)Visit(context.relational_expression(0));
                var right = (Expression)Visit(context.relational_expression(1));
                var op = (BinaryOperatorLiteral)Visit(context.relational_operator());
                result = new BinaryOperatorExpression(left, op, right, context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitCompound_expression([NotNull] PlSqlParser.Compound_expressionContext context) { return VisitChildren(context); }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public UstNode VisitRelational_operator([NotNull] PlSqlParser.Relational_operatorContext context)
        {
            BinaryOperatorLiteral result;
            if (context.not_equal_op() != null)
            {
                result = (BinaryOperatorLiteral)Visit(context.not_equal_op());
            }
            else if (context.less_than_or_equals_op() != null)
            {
                result = (BinaryOperatorLiteral)Visit(context.less_than_or_equals_op());
            }
            else if (context.greater_than_or_equals_op() != null)
            {
                result = (BinaryOperatorLiteral)Visit(context.greater_than_or_equals_op());
            }
            else
            {
                string opText = context.GetText();
                if (opText == "=")
                {
                    opText = "==";
                }
                var literal = BinaryOperatorLiteral.TextBinaryOperator[opText];
                result = new BinaryOperatorLiteral(literal, context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitLike_type([NotNull] PlSqlParser.Like_typeContext context) { return VisitChildren(context); }

        public UstNode VisitLike_escape_part([NotNull] PlSqlParser.Like_escape_partContext context) { return VisitChildren(context); }

        public UstNode VisitIn_elements([NotNull] PlSqlParser.In_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitBetween_elements([NotNull] PlSqlParser.Between_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitConcatenation([NotNull] PlSqlParser.ConcatenationContext context)
        {
            var result = (Expression)Visit(context.additive_expression(0));
            if (context.additive_expression().Length > 1)
            {
                for (int i = 1; i < context.additive_expression().Length; i++)
                {
                    var right = (Expression)Visit(context.additive_expression(i));
                    result = new BinaryOperatorExpression(result, (BinaryOperatorLiteral)Visit(context.concatenation_op(i - 1)), right,
                        result.TextSpan.Union(right.TextSpan), FileNode);
                }
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitAdditive_expression([NotNull] PlSqlParser.Additive_expressionContext context)
        {
            var result = (Expression)Visit(context.multiply_expression(0));
            if (context.multiply_expression().Length > 1)
            {
                for (int i = 1; i < context.multiply_expression().Length; i++)
                {
                    var op = new BinaryOperatorLiteral(BinaryOperatorLiteral.TextBinaryOperator[context._op[i - 1].Text],
                      context._op[i - 1].GetTextSpan(), FileNode);
                    var right = (Expression)Visit(context.multiply_expression(i));
                    result = new BinaryOperatorExpression(result, op, right, result.TextSpan.Union(right.TextSpan), FileNode);
                }
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitMultiply_expression([NotNull] PlSqlParser.Multiply_expressionContext context)
        {
            var result = (Expression)Visit(context.datetime_expression(0));
            if (context.datetime_expression().Length > 1)
            {
                for (int i = 1; i < context.datetime_expression().Length; i++)
                {
                    var right = (Expression)Visit(context.datetime_expression(i));
                    var op  = new BinaryOperatorLiteral(BinaryOperatorLiteral.TextBinaryOperator[context._op[i - 1].Text],
                        context._op[i - 1].GetTextSpan(), FileNode);
                    result = new BinaryOperatorExpression(result, op, right, result.TextSpan.Union(right.TextSpan), FileNode);
                }
            }
            return result;
        }

        public UstNode VisitDatetime_expression([NotNull] PlSqlParser.Datetime_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitInterval_expression([NotNull] PlSqlParser.Interval_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitModel_expression([NotNull] PlSqlParser.Model_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitModel_expression_element([NotNull] PlSqlParser.Model_expression_elementContext context) { return VisitChildren(context); }

        public UstNode VisitSingle_column_for_loop([NotNull] PlSqlParser.Single_column_for_loopContext context) { return VisitChildren(context); }

        public UstNode VisitFor_like_part([NotNull] PlSqlParser.For_like_partContext context) { return VisitChildren(context); }

        public UstNode VisitFor_increment_decrement_type([NotNull] PlSqlParser.For_increment_decrement_typeContext context) { return VisitChildren(context); }

        public UstNode VisitMulti_column_for_loop([NotNull] PlSqlParser.Multi_column_for_loopContext context) { return VisitChildren(context); }

        public UstNode VisitUnary_expression([NotNull] PlSqlParser.Unary_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitCase_statement([NotNull] PlSqlParser.Case_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSimple_case_statement([NotNull] PlSqlParser.Simple_case_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSimple_case_when_part([NotNull] PlSqlParser.Simple_case_when_partContext context) { return VisitChildren(context); }

        public UstNode VisitSearched_case_statement([NotNull] PlSqlParser.Searched_case_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSearched_case_when_part([NotNull] PlSqlParser.Searched_case_when_partContext context) { return VisitChildren(context); }

        public UstNode VisitCase_else_part([NotNull] PlSqlParser.Case_else_partContext context) { return VisitChildren(context); }

        public UstNode VisitAtom([NotNull] PlSqlParser.AtomContext context) { return VisitChildren(context); }

        public UstNode VisitExpression_or_vector([NotNull] PlSqlParser.Expression_or_vectorContext context) { return VisitChildren(context); }

        public UstNode VisitVector_expr([NotNull] PlSqlParser.Vector_exprContext context) { return VisitChildren(context); }

        public UstNode VisitQuantified_expression([NotNull] PlSqlParser.Quantified_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitStandard_function([NotNull] PlSqlParser.Standard_functionContext context) { return VisitChildren(context); }

        public UstNode VisitOver_clause_keyword([NotNull] PlSqlParser.Over_clause_keywordContext context) { return VisitChildren(context); }

        public UstNode VisitWithin_or_over_clause_keyword([NotNull] PlSqlParser.Within_or_over_clause_keywordContext context) { return VisitChildren(context); }

        public UstNode VisitStandard_prediction_function_keyword([NotNull] PlSqlParser.Standard_prediction_function_keywordContext context) { return VisitChildren(context); }

        public UstNode VisitOver_clause([NotNull] PlSqlParser.Over_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitWindowing_clause([NotNull] PlSqlParser.Windowing_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitWindowing_type([NotNull] PlSqlParser.Windowing_typeContext context) { return VisitChildren(context); }

        public UstNode VisitWindowing_elements([NotNull] PlSqlParser.Windowing_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitUsing_clause([NotNull] PlSqlParser.Using_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitUsing_element([NotNull] PlSqlParser.Using_elementContext context) { return VisitChildren(context); }

        public UstNode VisitCollect_order_by_part([NotNull] PlSqlParser.Collect_order_by_partContext context) { return VisitChildren(context); }

        public UstNode VisitWithin_or_over_part([NotNull] PlSqlParser.Within_or_over_partContext context) { return VisitChildren(context); }

        public UstNode VisitCost_matrix_clause([NotNull] PlSqlParser.Cost_matrix_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_passing_clause([NotNull] PlSqlParser.Xml_passing_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_attributes_clause([NotNull] PlSqlParser.Xml_attributes_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_namespaces_clause([NotNull] PlSqlParser.Xml_namespaces_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_table_column([NotNull] PlSqlParser.Xml_table_columnContext context) { return VisitChildren(context); }

        public UstNode VisitXml_general_default_part([NotNull] PlSqlParser.Xml_general_default_partContext context) { return VisitChildren(context); }

        public UstNode VisitXml_multiuse_expression_element([NotNull] PlSqlParser.Xml_multiuse_expression_elementContext context) { return VisitChildren(context); }

        public UstNode VisitXmlroot_param_version_part([NotNull] PlSqlParser.Xmlroot_param_version_partContext context) { return VisitChildren(context); }

        public UstNode VisitXmlroot_param_standalone_part([NotNull] PlSqlParser.Xmlroot_param_standalone_partContext context) { return VisitChildren(context); }

        public UstNode VisitXmlserialize_param_enconding_part([NotNull] PlSqlParser.Xmlserialize_param_enconding_partContext context) { return VisitChildren(context); }

        public UstNode VisitXmlserialize_param_version_part([NotNull] PlSqlParser.Xmlserialize_param_version_partContext context) { return VisitChildren(context); }

        public UstNode VisitXmlserialize_param_ident_part([NotNull] PlSqlParser.Xmlserialize_param_ident_partContext context) { return VisitChildren(context); }

        public UstNode VisitSql_plus_command([NotNull] PlSqlParser.Sql_plus_commandContext context) { return VisitChildren(context); }

        public UstNode VisitWhenever_command([NotNull] PlSqlParser.Whenever_commandContext context) { return VisitChildren(context); }

        public UstNode VisitSet_command([NotNull] PlSqlParser.Set_commandContext context) { return VisitChildren(context); }

        public UstNode VisitExit_command([NotNull] PlSqlParser.Exit_commandContext context) { return VisitChildren(context); }

        public UstNode VisitPrompt_command([NotNull] PlSqlParser.Prompt_commandContext context) { return VisitChildren(context); }

        public UstNode VisitPartition_extension_clause([NotNull] PlSqlParser.Partition_extension_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitColumn_alias([NotNull] PlSqlParser.Column_aliasContext context) { return VisitChildren(context); }

        public UstNode VisitTable_alias([NotNull] PlSqlParser.Table_aliasContext context) { return VisitChildren(context); }

        public UstNode VisitAlias_quoted_string([NotNull] PlSqlParser.Alias_quoted_stringContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitWhere_clause([NotNull] PlSqlParser.Where_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCurrent_of_clause([NotNull] PlSqlParser.Current_of_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitInto_clause([NotNull] PlSqlParser.Into_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_column_name([NotNull] PlSqlParser.Xml_column_nameContext context) { return VisitChildren(context); }

        public UstNode VisitCost_class_name([NotNull] PlSqlParser.Cost_class_nameContext context) { return VisitChildren(context); }

        public UstNode VisitAttribute_name([NotNull] PlSqlParser.Attribute_nameContext context) { return VisitChildren(context); }

        public UstNode VisitSavepoint_name([NotNull] PlSqlParser.Savepoint_nameContext context) { return VisitChildren(context); }

        public UstNode VisitRollback_segment_name([NotNull] PlSqlParser.Rollback_segment_nameContext context) { return VisitChildren(context); }

        public UstNode VisitTable_var_name([NotNull] PlSqlParser.Table_var_nameContext context) { return VisitChildren(context); }

        public UstNode VisitSchema_name([NotNull] PlSqlParser.Schema_nameContext context) { return VisitChildren(context); }

        public UstNode VisitRoutine_name([NotNull] PlSqlParser.Routine_nameContext context)
        {
            Expression result = (IdToken)Visit(context.identifier());
            if (context.id_expression().Length > 0)
            {
                var firstSpan = context.identifier().GetTextSpan();
                for (int i = 0; i < context.id_expression().Length; i++)
                {
                    result = new MemberReferenceExpression(result,
                        (Expression)Visit(context.id_expression(i)), firstSpan.Union(context.id_expression(i).GetTextSpan()), FileNode);
                }
            }
            return result;
        }

        public UstNode VisitPackage_name([NotNull] PlSqlParser.Package_nameContext context) { return VisitChildren(context); }

        public UstNode VisitImplementation_type_name([NotNull] PlSqlParser.Implementation_type_nameContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitParameter_name([NotNull] PlSqlParser.Parameter_nameContext context) { return VisitChildren(context); }

        public UstNode VisitReference_model_name([NotNull] PlSqlParser.Reference_model_nameContext context) { return VisitChildren(context); }

        public UstNode VisitMain_model_name([NotNull] PlSqlParser.Main_model_nameContext context) { return VisitChildren(context); }

        public UstNode VisitAggregate_function_name([NotNull] PlSqlParser.Aggregate_function_nameContext context) { return VisitChildren(context); }

        public UstNode VisitQuery_name([NotNull] PlSqlParser.Query_nameContext context) { return VisitChildren(context); }

        public UstNode VisitConstraint_name([NotNull] PlSqlParser.Constraint_nameContext context) { return VisitChildren(context); }

        public UstNode VisitLabel_name([NotNull] PlSqlParser.Label_nameContext context) { return VisitChildren(context); }

        public UstNode VisitType_name([NotNull] PlSqlParser.Type_nameContext context)
        {
            var typeName = new TypeToken(
                string.Join(".", context.id_expression().Select(expr => ((IdToken)Visit(expr)).TextValue)),
                context.GetTextSpan(), FileNode);
            return typeName;
        }

        public UstNode VisitSequence_name([NotNull] PlSqlParser.Sequence_nameContext context) { return VisitChildren(context); }

        /// <returns><see cref="TypeToken"/></returns>
        public UstNode VisitException_name([NotNull] PlSqlParser.Exception_nameContext context)
        {
            var result = new TypeToken(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitFunction_name([NotNull] PlSqlParser.Function_nameContext context) { return VisitChildren(context); }

        public UstNode VisitProcedure_name([NotNull] PlSqlParser.Procedure_nameContext context) { return VisitChildren(context); }

        public UstNode VisitTrigger_name([NotNull] PlSqlParser.Trigger_nameContext context) { return VisitChildren(context); }

        public UstNode VisitVariable_name([NotNull] PlSqlParser.Variable_nameContext context) { return VisitChildren(context); }

        public UstNode VisitIndex_name([NotNull] PlSqlParser.Index_nameContext context) { return VisitChildren(context); }

        public UstNode VisitCursor_name([NotNull] PlSqlParser.Cursor_nameContext context) { return VisitChildren(context); }

        public UstNode VisitRecord_name([NotNull] PlSqlParser.Record_nameContext context) { return VisitChildren(context); }

        public UstNode VisitCollection_name([NotNull] PlSqlParser.Collection_nameContext context) { return VisitChildren(context); }

        public UstNode VisitLink_name([NotNull] PlSqlParser.Link_nameContext context) { return VisitChildren(context); }

        public UstNode VisitColumn_name([NotNull] PlSqlParser.Column_nameContext context) { return VisitChildren(context); }

        public UstNode VisitTableview_name([NotNull] PlSqlParser.Tableview_nameContext context) { return VisitChildren(context); }

        public UstNode VisitChar_set_name([NotNull] PlSqlParser.Char_set_nameContext context) { return VisitChildren(context); }

        public UstNode VisitKeep_clause([NotNull] PlSqlParser.Keep_clauseContext context) { return VisitChildren(context); }

        /// <returns><see cref="ArgsNode"/></returns>
        public UstNode VisitFunction_argument([NotNull] PlSqlParser.Function_argumentContext context)
        {
            List<Expression> args = context.argument().Select(arg => (Expression)Visit(arg)).ToList();
            if (context.keep_clause() != null)
            {
                args.Add((Expression)Visit(context.keep_clause()));
            }
            var result = new ArgsNode(args, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitFunction_argument_analytic([NotNull] PlSqlParser.Function_argument_analyticContext context) { return VisitChildren(context); }

        public UstNode VisitFunction_argument_modeling([NotNull] PlSqlParser.Function_argument_modelingContext context) { return VisitChildren(context); }

        public UstNode VisitRespect_or_ignore_nulls([NotNull] PlSqlParser.Respect_or_ignore_nullsContext context) { return VisitChildren(context); }

        public UstNode VisitArgument([NotNull] PlSqlParser.ArgumentContext context) { return VisitChildren(context); }

        /// <returns><see cref="TypeToken"/></returns>
        public UstNode VisitType_spec([NotNull] PlSqlParser.Type_specContext context)
        {
            TypeToken result;
            if (context.datatype() != null)
            {
                result = (TypeToken)Visit(context.datatype());
            }
            else
            {
                result = (TypeToken)Visit(context.type_name());
            }
            return result;
        }

        /// <returns><see cref="TypeToken"/></returns>
        public UstNode VisitDatatype([NotNull] PlSqlParser.DatatypeContext context)
        {
            TypeToken result;
            if (context.native_datatype_element() != null)
            {
                result = (TypeToken)Visit(context.native_datatype_element());
            }
            else
            {
                result = new TypeToken(context.INTERVAL().GetText().ToLowerInvariant());
            }
            return result;
        }

        public UstNode VisitPrecision_part([NotNull] PlSqlParser.Precision_partContext context) { return VisitChildren(context); }

        /// <returns><see cref="TypeToken"/></returns>
        public UstNode VisitNative_datatype_element([NotNull] PlSqlParser.Native_datatype_elementContext context)
        {
            return new TypeToken(context.GetText(), context.GetTextSpan(), FileNode);
        }

        public UstNode VisitBind_variable([NotNull] PlSqlParser.Bind_variableContext context)
        {
            var result = VisitChildren(context);
            if (result == null)
            {
                result = new NullLiteral(context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitGeneral_element([NotNull] PlSqlParser.General_elementContext context) { return VisitChildren(context); }

        public UstNode VisitGeneral_element_part([NotNull] PlSqlParser.General_element_partContext context)
        {
            Expression result = (Expression)Visit(context.id_expression(0));
            var firstSpan = context.id_expression(0).GetTextSpan();
            for (int i = 1; i < context.id_expression().Length; i++)
            {
                result = new MemberReferenceExpression(result, (Expression)Visit(context.id_expression(i)),
                    firstSpan.Union(context.id_expression(i).GetTextSpan()), FileNode);
            }
            if (context.function_argument() != null)
            {
                var argsNode = (ArgsNode)Visit(context.function_argument());
                result = new InvocationExpression(result, argsNode, firstSpan.Union(argsNode.TextSpan), FileNode);
            }
            return result;
        }

        public UstNode VisitTable_element([NotNull] PlSqlParser.Table_elementContext context) { return VisitChildren(context); }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitConstant([NotNull] PlSqlParser.ConstantContext context)
        {
            Token result;
            if (context.NULL() != null)
            {
                result = new NullLiteral(context.GetTextSpan(), FileNode);
            }
            else if (context.TRUE() != null || context.FALSE() != null)
            {
                result = new BooleanLiteral(bool.Parse(context.GetText().ToLowerInvariant()), context.GetTextSpan(), FileNode);
            }
            else
            {
                result = (Token)Visit(context.GetChild(0));
            }
            return result;
        }

        public UstNode VisitNumeric([NotNull] PlSqlParser.NumericContext context)
        {
            Token result;
            string text = context.GetText();
            if (context.UNSIGNED_INTEGER() != null)
            {
                result = new IntLiteral(long.Parse(text), context.GetTextSpan(), FileNode);
            }
            else
            {
                text = text.ToLowerInvariant().Replace("f", "").Replace("d", "");
                result = new FloatLiteral(double.Parse(text), context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitQuoted_string([NotNull] PlSqlParser.Quoted_stringContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitId([NotNull] PlSqlParser.IdentifierContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitId_expression([NotNull] PlSqlParser.Id_expressionContext context)
        {
            IdToken result;
            if (context.regular_id() != null)
            {
                result = (IdToken)Visit(context.GetChild(0));
            }
            else
            {
                string text = context.GetText();
                result = new IdToken(text.Substring(1, text.Length - 2), context.GetTextSpan(), FileNode);
            }
            return result;
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public UstNode VisitNot_equal_op([NotNull] PlSqlParser.Not_equal_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.NotEqual, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public UstNode VisitGreater_than_or_equals_op([NotNull] PlSqlParser.Greater_than_or_equals_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.GreaterOrEqual, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public UstNode VisitLess_than_or_equals_op([NotNull] PlSqlParser.Less_than_or_equals_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.LessOrEqual, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitConcatenation_op([NotNull] PlSqlParser.Concatenation_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.Plus, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitOuter_join_sign([NotNull] PlSqlParser.Outer_join_signContext context) { return VisitChildren(context); }

        public UstNode VisitRegular_id([NotNull] PlSqlParser.Regular_idContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitShow_errors_command([NotNull] PlSqlParser.Show_errors_commandContext context) { return VisitChildren(context); }

        public UstNode VisitNumeric_negative([NotNull] PlSqlParser.Numeric_negativeContext context) { return VisitChildren(context); }

        public UstNode VisitTable_ref_aux_internal_three([NotNull] PlSqlParser.Table_ref_aux_internal_threeContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTable_ref_aux_internal_one([NotNull] PlSqlParser.Table_ref_aux_internal_oneContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTable_ref_aux_internal_two([NotNull] PlSqlParser.Table_ref_aux_internal_twoContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitCreate_package_body([NotNull] PlSqlParser.Create_package_bodyContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFunction_body([NotNull] PlSqlParser.Function_bodyContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitProcedure_body([NotNull] PlSqlParser.Procedure_bodyContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitRecord_type_def([NotNull] PlSqlParser.Record_type_defContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitRef_cursor_type_def([NotNull] PlSqlParser.Ref_cursor_type_defContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitType_declaration([NotNull] PlSqlParser.Type_declarationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTable_type_def([NotNull] PlSqlParser.Table_type_defContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTable_ref_aux_internal([NotNull] PlSqlParser.Table_ref_aux_internalContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitString_function([NotNull] PlSqlParser.String_functionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitNumeric_function_wrapper([NotNull] PlSqlParser.Numeric_function_wrapperContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitNumeric_function([NotNull] PlSqlParser.Numeric_functionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitOther_function([NotNull] PlSqlParser.Other_functionContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitIdentifier([NotNull] PlSqlParser.IdentifierContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitString_function_name([NotNull] PlSqlParser.String_function_nameContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitNumeric_function_name([NotNull] PlSqlParser.Numeric_function_nameContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitCreate_table([NotNull] PlSqlParser.Create_tableContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDrop_table([NotNull] PlSqlParser.Drop_tableContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitAnonymous_block([NotNull] PlSqlParser.Anonymous_blockContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitStart_command([NotNull] PlSqlParser.Start_commandContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitComment_on_column([NotNull] PlSqlParser.Comment_on_columnContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitCreate_synonym([NotNull] PlSqlParser.Create_synonymContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitComment_on_table([NotNull] PlSqlParser.Comment_on_tableContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitSynonym_name([NotNull] PlSqlParser.Synonym_nameContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitSchema_object_name([NotNull] PlSqlParser.Schema_object_nameContext context)
        {
            return VisitChildren(context);
        }
    }
}
