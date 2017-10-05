using Antlr4.Runtime.Misc;
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
using PT.PM.PlSqlParseTreeUst;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.SqlParseTreeUst
{
    public partial class PlSqlAntlrConverter : AntlrConverter, IPlSqlParserVisitor<Ust>
    {
        public override LanguageInfo Language => PlSql.Language;

        /// <returns><see cref="ArgsUst"/></returns>
        public Ust VisitSwallow_to_semi([NotNull] PlSqlParser.Swallow_to_semiContext context)
        {
            Expression[] args = context.children.Select(c => (Expression)Visit(c)).ToArray();
            var result = new ArgsUst(args, context.GetTextSpan());
            return result;
        }

        public Ust VisitCompilation_unit([NotNull] PlSqlParser.Compilation_unitContext context)
        {
            var statements = context.unit_statement().Select(statement => (Statement)Visit(statement)).ToArray();
            root.Nodes = statements;
            return root;
        }

        public Ust VisitSql_script([NotNull] PlSqlParser.Sql_scriptContext context) { return VisitChildren(context); }

        /// <returns><see cref="Statement"/></returns>
        public Ust VisitUnit_statement([NotNull] PlSqlParser.Unit_statementContext context)
        {
            var child = Visit(context.GetChild(0));
            Statement result = child.ToStatementIfRequired();
            return result;
        }

        public Ust VisitDrop_function([NotNull] PlSqlParser.Drop_functionContext context) { return VisitChildren(context); }

        public Ust VisitAlter_function([NotNull] PlSqlParser.Alter_functionContext context) { return VisitChildren(context); }

        public Ust VisitCreate_function_body([NotNull] PlSqlParser.Create_function_bodyContext context)
        {
            var name = (IdToken)Visit(context.function_name());
            ParameterDeclaration[] parameters = context.parameter().Select(p => (ParameterDeclaration)Visit(p)).ToArray();
            var body = (BlockStatement)Visit(context.body());

            var result = new MethodDeclaration(name, parameters, body, context.GetTextSpan());
            result.ReturnType = (TypeToken)Visit(context.type_spec());
            return result;
        }

        public Ust VisitParallel_enable_clause([NotNull] PlSqlParser.Parallel_enable_clauseContext context) { return VisitChildren(context); }

        public Ust VisitPartition_by_clause([NotNull] PlSqlParser.Partition_by_clauseContext context) { return VisitChildren(context); }

        public Ust VisitResult_cache_clause([NotNull] PlSqlParser.Result_cache_clauseContext context) { return VisitChildren(context); }

        public Ust VisitRelies_on_part([NotNull] PlSqlParser.Relies_on_partContext context) { return VisitChildren(context); }

        public Ust VisitStreaming_clause([NotNull] PlSqlParser.Streaming_clauseContext context) { return VisitChildren(context); }

        public Ust VisitDrop_package([NotNull] PlSqlParser.Drop_packageContext context) { return VisitChildren(context); }

        public Ust VisitAlter_package([NotNull] PlSqlParser.Alter_packageContext context) { return VisitChildren(context); }

        public Ust VisitCreate_package([NotNull] PlSqlParser.Create_packageContext context) { return VisitChildren(context); }

        public Ust VisitPackage_obj_spec([NotNull] PlSqlParser.Package_obj_specContext context) { return VisitChildren(context); }

        public Ust VisitProcedure_spec([NotNull] PlSqlParser.Procedure_specContext context) { return VisitChildren(context); }

        public Ust VisitFunction_spec([NotNull] PlSqlParser.Function_specContext context) { return VisitChildren(context); }

        public Ust VisitPackage_obj_body([NotNull] PlSqlParser.Package_obj_bodyContext context) { return VisitChildren(context); }

        public Ust VisitDrop_procedure([NotNull] PlSqlParser.Drop_procedureContext context) { return VisitChildren(context); }

        public Ust VisitAlter_procedure([NotNull] PlSqlParser.Alter_procedureContext context) { return VisitChildren(context); }

        public Ust VisitCreate_procedure_body([NotNull] PlSqlParser.Create_procedure_bodyContext context)
        {
            var name = (IdToken)Visit(context.procedure_name());
            ParameterDeclaration[] parameters = context.parameter().Select(p => (ParameterDeclaration)Visit(p)).ToArray();
            var body = (BlockStatement)Visit(context.body());

            var result = new MethodDeclaration(name, parameters, body, context.GetTextSpan());
            return result;
        }

        public Ust VisitDrop_trigger([NotNull] PlSqlParser.Drop_triggerContext context) { return VisitChildren(context); }

        public Ust VisitAlter_trigger([NotNull] PlSqlParser.Alter_triggerContext context) { return VisitChildren(context); }

        public Ust VisitCreate_trigger([NotNull] PlSqlParser.Create_triggerContext context) { return VisitChildren(context); }

        public Ust VisitTrigger_follows_clause([NotNull] PlSqlParser.Trigger_follows_clauseContext context) { return VisitChildren(context); }

        public Ust VisitTrigger_when_clause([NotNull] PlSqlParser.Trigger_when_clauseContext context) { return VisitChildren(context); }

        public Ust VisitSimple_dml_trigger([NotNull] PlSqlParser.Simple_dml_triggerContext context) { return VisitChildren(context); }

        public Ust VisitFor_each_row([NotNull] PlSqlParser.For_each_rowContext context) { return VisitChildren(context); }

        public Ust VisitCompound_dml_trigger([NotNull] PlSqlParser.Compound_dml_triggerContext context) { return VisitChildren(context); }

        public Ust VisitNon_dml_trigger([NotNull] PlSqlParser.Non_dml_triggerContext context) { return VisitChildren(context); }

        public Ust VisitTrigger_body([NotNull] PlSqlParser.Trigger_bodyContext context) { return VisitChildren(context); }

        public Ust VisitRoutine_clause([NotNull] PlSqlParser.Routine_clauseContext context) { return VisitChildren(context); }

        public Ust VisitCompound_trigger_block([NotNull] PlSqlParser.Compound_trigger_blockContext context) { return VisitChildren(context); }

        public Ust VisitTiming_point_section([NotNull] PlSqlParser.Timing_point_sectionContext context) { return VisitChildren(context); }

        public Ust VisitNon_dml_event([NotNull] PlSqlParser.Non_dml_eventContext context) { return VisitChildren(context); }

        public Ust VisitDml_event_clause([NotNull] PlSqlParser.Dml_event_clauseContext context) { return VisitChildren(context); }

        public Ust VisitDml_event_element([NotNull] PlSqlParser.Dml_event_elementContext context) { return VisitChildren(context); }

        public Ust VisitDml_event_nested_clause([NotNull] PlSqlParser.Dml_event_nested_clauseContext context) { return VisitChildren(context); }

        public Ust VisitReferencing_clause([NotNull] PlSqlParser.Referencing_clauseContext context) { return VisitChildren(context); }

        public Ust VisitReferencing_element([NotNull] PlSqlParser.Referencing_elementContext context) { return VisitChildren(context); }

        public Ust VisitDrop_type([NotNull] PlSqlParser.Drop_typeContext context) { return VisitChildren(context); }

        public Ust VisitAlter_type([NotNull] PlSqlParser.Alter_typeContext context) { return VisitChildren(context); }

        public Ust VisitCompile_type_clause([NotNull] PlSqlParser.Compile_type_clauseContext context) { return VisitChildren(context); }

        public Ust VisitReplace_type_clause([NotNull] PlSqlParser.Replace_type_clauseContext context) { return VisitChildren(context); }

        public Ust VisitAlter_method_spec([NotNull] PlSqlParser.Alter_method_specContext context) { return VisitChildren(context); }

        public Ust VisitAlter_method_element([NotNull] PlSqlParser.Alter_method_elementContext context) { return VisitChildren(context); }

        public Ust VisitAlter_attribute_definition([NotNull] PlSqlParser.Alter_attribute_definitionContext context) { return VisitChildren(context); }

        public Ust VisitAttribute_definition([NotNull] PlSqlParser.Attribute_definitionContext context) { return VisitChildren(context); }

        public Ust VisitAlter_collection_clauses([NotNull] PlSqlParser.Alter_collection_clausesContext context) { return VisitChildren(context); }

        public Ust VisitDependent_handling_clause([NotNull] PlSqlParser.Dependent_handling_clauseContext context) { return VisitChildren(context); }

        public Ust VisitDependent_exceptions_part([NotNull] PlSqlParser.Dependent_exceptions_partContext context) { return VisitChildren(context); }

        public Ust VisitCreate_type([NotNull] PlSqlParser.Create_typeContext context) { return VisitChildren(context); }

        public Ust VisitType_definition([NotNull] PlSqlParser.Type_definitionContext context) { return VisitChildren(context); }

        public Ust VisitObject_type_def([NotNull] PlSqlParser.Object_type_defContext context) { return VisitChildren(context); }

        public Ust VisitObject_as_part([NotNull] PlSqlParser.Object_as_partContext context) { return VisitChildren(context); }

        public Ust VisitObject_under_part([NotNull] PlSqlParser.Object_under_partContext context) { return VisitChildren(context); }

        public Ust VisitNested_table_type_def([NotNull] PlSqlParser.Nested_table_type_defContext context) { return VisitChildren(context); }

        public Ust VisitSqlj_object_type([NotNull] PlSqlParser.Sqlj_object_typeContext context) { return VisitChildren(context); }

        public Ust VisitType_body([NotNull] PlSqlParser.Type_bodyContext context) { return VisitChildren(context); }

        public Ust VisitType_body_elements([NotNull] PlSqlParser.Type_body_elementsContext context) { return VisitChildren(context); }

        public Ust VisitMap_order_func_declaration([NotNull] PlSqlParser.Map_order_func_declarationContext context) { return VisitChildren(context); }

        public Ust VisitSubprog_decl_in_type([NotNull] PlSqlParser.Subprog_decl_in_typeContext context) { return VisitChildren(context); }

        public Ust VisitProc_decl_in_type([NotNull] PlSqlParser.Proc_decl_in_typeContext context) { return VisitChildren(context); }

        public Ust VisitFunc_decl_in_type([NotNull] PlSqlParser.Func_decl_in_typeContext context) { return VisitChildren(context); }

        public Ust VisitConstructor_declaration([NotNull] PlSqlParser.Constructor_declarationContext context) { return VisitChildren(context); }

        public Ust VisitModifier_clause([NotNull] PlSqlParser.Modifier_clauseContext context) { return VisitChildren(context); }

        public Ust VisitObject_member_spec([NotNull] PlSqlParser.Object_member_specContext context) { return VisitChildren(context); }

        public Ust VisitSqlj_object_type_attr([NotNull] PlSqlParser.Sqlj_object_type_attrContext context) { return VisitChildren(context); }

        public Ust VisitElement_spec([NotNull] PlSqlParser.Element_specContext context) { return VisitChildren(context); }

        public Ust VisitElement_spec_options([NotNull] PlSqlParser.Element_spec_optionsContext context) { return VisitChildren(context); }

        public Ust VisitSubprogram_spec([NotNull] PlSqlParser.Subprogram_specContext context) { return VisitChildren(context); }

        public Ust VisitType_procedure_spec([NotNull] PlSqlParser.Type_procedure_specContext context) { return VisitChildren(context); }

        public Ust VisitType_function_spec([NotNull] PlSqlParser.Type_function_specContext context) { return VisitChildren(context); }

        public Ust VisitConstructor_spec([NotNull] PlSqlParser.Constructor_specContext context) { return VisitChildren(context); }

        public Ust VisitMap_order_function_spec([NotNull] PlSqlParser.Map_order_function_specContext context) { return VisitChildren(context); }

        public Ust VisitPragma_clause([NotNull] PlSqlParser.Pragma_clauseContext context) { return VisitChildren(context); }

        public Ust VisitPragma_elements([NotNull] PlSqlParser.Pragma_elementsContext context) { return VisitChildren(context); }

        public Ust VisitType_elements_parameter([NotNull] PlSqlParser.Type_elements_parameterContext context) { return VisitChildren(context); }

        public Ust VisitDrop_sequence([NotNull] PlSqlParser.Drop_sequenceContext context) { return VisitChildren(context); }

        public Ust VisitAlter_sequence([NotNull] PlSqlParser.Alter_sequenceContext context) { return VisitChildren(context); }

        public Ust VisitCreate_sequence([NotNull] PlSqlParser.Create_sequenceContext context) { return VisitChildren(context); }

        public Ust VisitSequence_spec([NotNull] PlSqlParser.Sequence_specContext context) { return VisitChildren(context); }

        public Ust VisitSequence_start_clause([NotNull] PlSqlParser.Sequence_start_clauseContext context) { return VisitChildren(context); }

        public Ust VisitInvoker_rights_clause([NotNull] PlSqlParser.Invoker_rights_clauseContext context) { return VisitChildren(context); }

        public Ust VisitCompiler_parameters_clause([NotNull] PlSqlParser.Compiler_parameters_clauseContext context) { return VisitChildren(context); }

        public Ust VisitCall_spec([NotNull] PlSqlParser.Call_specContext context) { return VisitChildren(context); }

        public Ust VisitJava_spec([NotNull] PlSqlParser.Java_specContext context) { return VisitChildren(context); }

        public Ust VisitC_spec([NotNull] PlSqlParser.C_specContext context) { return VisitChildren(context); }

        public Ust VisitC_agent_in_clause([NotNull] PlSqlParser.C_agent_in_clauseContext context) { return VisitChildren(context); }

        public Ust VisitC_parameters_clause([NotNull] PlSqlParser.C_parameters_clauseContext context) { return VisitChildren(context); }

        /// <returns><see cref="ParameterDeclaration"/></returns>
        public Ust VisitParameter([NotNull] PlSqlParser.ParameterContext context)
        {
            ParameterDeclaration result;
            TypeToken typeToken = null;
            if (context.type_spec() != null)
            {
                typeToken = (TypeToken)Visit(context.type_spec());
            }
            var name = (IdToken)Visit(context.parameter_name());
            result = new ParameterDeclaration(typeToken, name, context.GetTextSpan());
            return result;
        }

        public Ust VisitDefault_value_part([NotNull] PlSqlParser.Default_value_partContext context) { return VisitChildren(context); }

        public Ust VisitDeclare_spec([NotNull] PlSqlParser.Declare_specContext context) { return VisitChildren(context); }

        public Ust VisitVariable_declaration([NotNull] PlSqlParser.Variable_declarationContext context) { return VisitChildren(context); }

        public Ust VisitSubtype_declaration([NotNull] PlSqlParser.Subtype_declarationContext context) { return VisitChildren(context); }

        public Ust VisitCursor_declaration([NotNull] PlSqlParser.Cursor_declarationContext context) { return VisitChildren(context); }

        public Ust VisitParameter_spec([NotNull] PlSqlParser.Parameter_specContext context) { return VisitChildren(context); }

        public Ust VisitException_declaration([NotNull] PlSqlParser.Exception_declarationContext context) { return VisitChildren(context); }

        public Ust VisitPragma_declaration([NotNull] PlSqlParser.Pragma_declarationContext context) { return VisitChildren(context); }

        public Ust VisitField_spec([NotNull] PlSqlParser.Field_specContext context) { return VisitChildren(context); }

        public Ust VisitTable_indexed_by_part([NotNull] PlSqlParser.Table_indexed_by_partContext context) { return VisitChildren(context); }

        public Ust VisitVarray_type_def([NotNull] PlSqlParser.Varray_type_defContext context) { return VisitChildren(context); }

        /// <returns><see cref="BlockStatement"/></returns>
        public Ust VisitSeq_of_statements([NotNull] PlSqlParser.Seq_of_statementsContext context)
        {
            var result = new BlockStatement(context.statement().Select(s => (Statement)Visit(s)).ToArray(),
                context.GetTextSpan());
            return result;
        }

        public Ust VisitLabel_declaration([NotNull] PlSqlParser.Label_declarationContext context) { return VisitChildren(context); }

        /// <returns><see cref="Statement"/></returns>
        public Ust VisitStatement([NotNull] PlSqlParser.StatementContext context)
        {
            Statement result;
            if (context.CREATE() != null || context.ALTER() != null || context.GRANT() != null || context.TRUNCATE() != null)
            {
                string str = context.GetChild(0).GetText().ToLowerInvariant();
                if (context.ALL() != null)
                {
                    str += "_" + context.ALL().GetText().ToLowerInvariant();
                }
                var args = (ArgsUst)Visit(context.swallow_to_semi());
                var funcName = new IdToken(str, context.GetTextSpan());
                result = new ExpressionStatement(new InvocationExpression(funcName, args, context.GetTextSpan()));
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

        public Ust VisitAssignment_statement([NotNull] PlSqlParser.Assignment_statementContext context)
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
            var result = new AssignmentExpression(left, right, context.GetTextSpan());
            return result;
        }

        public Ust VisitContinue_statement([NotNull] PlSqlParser.Continue_statementContext context)
        {
            return new ContinueStatement(context.GetTextSpan());
        }

        public Ust VisitExit_statement([NotNull] PlSqlParser.Exit_statementContext context)
        {
            return new BreakStatement(context.GetTextSpan());
        }

        public Ust VisitGoto_statement([NotNull] PlSqlParser.Goto_statementContext context) { return VisitChildren(context); }

        /// <returns><see cref="IfElseStatement"/></returns>
        public Ust VisitIf_statement([NotNull] PlSqlParser.If_statementContext context)
        {
            var condition = (Expression)Visit(context.condition());
            var block = (BlockStatement)Visit(context.seq_of_statements());
            var result = new IfElseStatement(condition, block, context.GetTextSpan());
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
        public Ust VisitElsif_part([NotNull] PlSqlParser.Elsif_partContext context)
        {
            var condition = (Expression)Visit(context.condition());
            var block = (BlockStatement)Visit(context.seq_of_statements());
            var result = new IfElseStatement(condition, block, context.GetTextSpan());
            return result;
        }

        /// <returns><see cref="Statement"/></returns>
        public Ust VisitElse_part([NotNull] PlSqlParser.Else_partContext context)
        {
            return (BlockStatement)Visit(context.seq_of_statements());
        }

        /// <returns><see cref="Statement"/></returns>
        public Ust VisitLoop_statement([NotNull] PlSqlParser.Loop_statementContext context)
        {
            Ust result;
            var block = (BlockStatement)Visit(context.seq_of_statements());
            var textSpan = context.GetTextSpan();
            if (context.WHILE() != null)
            {
                result = new WhileStatement((Expression)Visit(context.condition()), block, textSpan);
            }
            else
            {
                var cursorLoopParam = context.cursor_loop_param();
                if (context.cursor_loop_param()?.lower_bound() != null)
                {
                    var varName = (IdToken)Visit(cursorLoopParam.index_name());
                    bool reverse = cursorLoopParam.REVERSE() != null;
                    var lowerBound = (Expression)Visit(cursorLoopParam.lower_bound());
                    var upperBound = (Expression)Visit(cursorLoopParam.upper_bound());
                    if (reverse)
                    {
                        Expression t = lowerBound;
                        lowerBound = upperBound;
                        upperBound = t;
                    }
                    var init = new ExpressionStatement(new AssignmentExpression(varName, lowerBound, varName.TextSpan.Union(lowerBound.TextSpan)));
                    var condition = new BinaryOperatorExpression(varName,
                        new BinaryOperatorLiteral(BinaryOperator.Less, cursorLoopParam.range.GetTextSpan()), upperBound,
                        lowerBound.TextSpan.Union(upperBound.TextSpan));
                    var iterator = new UnaryOperatorExpression(
                        new UnaryOperatorLiteral(UnaryOperator.PostIncrement, default(TextSpan)), varName,
                        cursorLoopParam.range.GetTextSpan());
                    result = new ForStatement(new Statement[] { init }, condition, new Expression[] { iterator },
                        block, textSpan);
                }
                else
                {
                    result = VisitChildren(context);
                }
            }
            return result;
        }

        public Ust VisitCursor_loop_param([NotNull] PlSqlParser.Cursor_loop_paramContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitForall_statement([NotNull] PlSqlParser.Forall_statementContext context) { return VisitChildren(context); }

        public Ust VisitBounds_clause([NotNull] PlSqlParser.Bounds_clauseContext context) { return VisitChildren(context); }

        public Ust VisitBetween_bound([NotNull] PlSqlParser.Between_boundContext context) { return VisitChildren(context); }

        public Ust VisitLower_bound([NotNull] PlSqlParser.Lower_boundContext context) { return VisitChildren(context); }

        public Ust VisitUpper_bound([NotNull] PlSqlParser.Upper_boundContext context) { return VisitChildren(context); }

        public Ust VisitNull_statement([NotNull] PlSqlParser.Null_statementContext context)
        {
            return new NullLiteral(context.GetTextSpan());
        }

        public Ust VisitRaise_statement([NotNull] PlSqlParser.Raise_statementContext context) { return VisitChildren(context); }

        public Ust VisitReturn_statement([NotNull] PlSqlParser.Return_statementContext context)
        {
            Expression returnExpression = null;
            if (context.expression() != null)
            {
                returnExpression = (Expression)Visit(context.expression());
            }
            var result = new ReturnStatement(returnExpression, context.GetTextSpan());
            return result;
        }

        public Ust VisitFunction_call([NotNull] PlSqlParser.Function_callContext context)
        {
            var target = (Expression)Visit(context.routine_name());
            ArgsUst args;
            if (context.function_argument() != null)
            {
                args = (ArgsUst)Visit(context.function_argument());
            }
            else
            {
                args = new ArgsUst();
            }
            var result = new InvocationExpression(target, args, context.GetTextSpan());
            return result;
        }

        /// <returns><see cref="BlockStatement"/></returns>
        public Ust VisitBody([NotNull] PlSqlParser.BodyContext context)
        {
            BlockStatement result;
            var block = (BlockStatement)Visit(context.seq_of_statements());
            if (context.exception_handler().Length > 0)
            {
                var tryCatch = new TryCatchStatement(block, context.GetTextSpan());
                tryCatch.CatchClauses = context.exception_handler().Select(handler =>
                    (CatchClause)Visit(handler)).ToList();
                result = new BlockStatement(new Statement[] { tryCatch }, context.GetTextSpan());
            }
            else
            {
                result = block;
            }
            return result;
        }

        /// <returns><see cref="CatchClause"/></returns>
        public Ust VisitException_handler([NotNull] PlSqlParser.Exception_handlerContext context)
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
                body.Statements[0] is ExpressionStatement expressionStatement &&
                expressionStatement.Expression is NullLiteral)
            {
                body = new BlockStatement(new Statement[0], context.seq_of_statements().GetTextSpan());
            }
            var result = new CatchClause(type, null, body, context.GetTextSpan());
            return result;
        }

        public Ust VisitTrigger_block([NotNull] PlSqlParser.Trigger_blockContext context) { return VisitChildren(context); }

        /// <returns><see cref="BlockStatement"/></returns>
        public Ust VisitBlock([NotNull] PlSqlParser.BlockContext context)
        {
            var declare = new BlockStatement(context.declare_spec().Select(s => Visit(s).ToStatementIfRequired()).ToArray(),
                context.GetTextSpan());
            var body = (BlockStatement)Visit(context.body());

            var result = new BlockStatement(new Statement[] { declare, body }, context.GetTextSpan());
            return result;
        }

        public Ust VisitSql_statement([NotNull] PlSqlParser.Sql_statementContext context) { return VisitChildren(context); }

        public Ust VisitExecute_immediate([NotNull] PlSqlParser.Execute_immediateContext context) { return VisitChildren(context); }

        public Ust VisitDynamic_returning_clause([NotNull] PlSqlParser.Dynamic_returning_clauseContext context) { return VisitChildren(context); }

        public Ust VisitData_manipulation_language_statements([NotNull] PlSqlParser.Data_manipulation_language_statementsContext context) { return VisitChildren(context); }

        public Ust VisitCursor_manipulation_statements([NotNull] PlSqlParser.Cursor_manipulation_statementsContext context) { return VisitChildren(context); }

        public Ust VisitClose_statement([NotNull] PlSqlParser.Close_statementContext context) { return VisitChildren(context); }

        public Ust VisitOpen_statement([NotNull] PlSqlParser.Open_statementContext context) { return VisitChildren(context); }

        public Ust VisitFetch_statement([NotNull] PlSqlParser.Fetch_statementContext context) { return VisitChildren(context); }

        public Ust VisitOpen_for_statement([NotNull] PlSqlParser.Open_for_statementContext context) { return VisitChildren(context); }

        public Ust VisitTransaction_control_statements([NotNull] PlSqlParser.Transaction_control_statementsContext context) { return VisitChildren(context); }

        public Ust VisitSet_transaction_command([NotNull] PlSqlParser.Set_transaction_commandContext context) { return VisitChildren(context); }

        public Ust VisitSet_constraint_command([NotNull] PlSqlParser.Set_constraint_commandContext context) { return VisitChildren(context); }

        public Ust VisitCommit_statement([NotNull] PlSqlParser.Commit_statementContext context) { return VisitChildren(context); }

        public Ust VisitWrite_clause([NotNull] PlSqlParser.Write_clauseContext context) { return VisitChildren(context); }

        public Ust VisitRollback_statement([NotNull] PlSqlParser.Rollback_statementContext context) { return VisitChildren(context); }

        public Ust VisitSavepoint_statement([NotNull] PlSqlParser.Savepoint_statementContext context) { return VisitChildren(context); }

        public Ust VisitExplain_statement([NotNull] PlSqlParser.Explain_statementContext context) { return VisitChildren(context); }

        public Ust VisitSelect_statement([NotNull] PlSqlParser.Select_statementContext context) { return VisitChildren(context); }

        public Ust VisitSubquery_factoring_clause([NotNull] PlSqlParser.Subquery_factoring_clauseContext context) { return VisitChildren(context); }

        public Ust VisitFactoring_element([NotNull] PlSqlParser.Factoring_elementContext context) { return VisitChildren(context); }

        public Ust VisitSearch_clause([NotNull] PlSqlParser.Search_clauseContext context) { return VisitChildren(context); }

        public Ust VisitCycle_clause([NotNull] PlSqlParser.Cycle_clauseContext context) { return VisitChildren(context); }

        public Ust VisitSubquery([NotNull] PlSqlParser.SubqueryContext context) { return VisitChildren(context); }

        public Ust VisitSubquery_operation_part([NotNull] PlSqlParser.Subquery_operation_partContext context) { return VisitChildren(context); }

        public Ust VisitSubquery_basic_elements([NotNull] PlSqlParser.Subquery_basic_elementsContext context) { return VisitChildren(context); }

        public Ust VisitQuery_block([NotNull] PlSqlParser.Query_blockContext context) { return VisitChildren(context); }

        public Ust VisitSelected_element([NotNull] PlSqlParser.Selected_elementContext context) { return VisitChildren(context); }

        public Ust VisitFrom_clause([NotNull] PlSqlParser.From_clauseContext context) { return VisitChildren(context); }

        public Ust VisitSelect_list_elements([NotNull] PlSqlParser.Select_list_elementsContext context) { return VisitChildren(context); }

        public Ust VisitTable_ref_list([NotNull] PlSqlParser.Table_ref_listContext context) { return VisitChildren(context); }

        public Ust VisitTable_ref([NotNull] PlSqlParser.Table_refContext context) { return VisitChildren(context); }

        public Ust VisitTable_ref_aux([NotNull] PlSqlParser.Table_ref_auxContext context) { return VisitChildren(context); }

        public Ust VisitJoin_clause([NotNull] PlSqlParser.Join_clauseContext context) { return VisitChildren(context); }

        public Ust VisitJoin_on_part([NotNull] PlSqlParser.Join_on_partContext context) { return VisitChildren(context); }

        public Ust VisitJoin_using_part([NotNull] PlSqlParser.Join_using_partContext context) { return VisitChildren(context); }

        public Ust VisitOuter_join_type([NotNull] PlSqlParser.Outer_join_typeContext context) { return VisitChildren(context); }

        public Ust VisitQuery_partition_clause([NotNull] PlSqlParser.Query_partition_clauseContext context) { return VisitChildren(context); }

        public Ust VisitFlashback_query_clause([NotNull] PlSqlParser.Flashback_query_clauseContext context) { return VisitChildren(context); }

        public Ust VisitPivot_clause([NotNull] PlSqlParser.Pivot_clauseContext context) { return VisitChildren(context); }

        public Ust VisitPivot_element([NotNull] PlSqlParser.Pivot_elementContext context) { return VisitChildren(context); }

        public Ust VisitPivot_for_clause([NotNull] PlSqlParser.Pivot_for_clauseContext context) { return VisitChildren(context); }

        public Ust VisitPivot_in_clause([NotNull] PlSqlParser.Pivot_in_clauseContext context) { return VisitChildren(context); }

        public Ust VisitPivot_in_clause_element([NotNull] PlSqlParser.Pivot_in_clause_elementContext context) { return VisitChildren(context); }

        public Ust VisitPivot_in_clause_elements([NotNull] PlSqlParser.Pivot_in_clause_elementsContext context) { return VisitChildren(context); }

        public Ust VisitUnpivot_clause([NotNull] PlSqlParser.Unpivot_clauseContext context) { return VisitChildren(context); }

        public Ust VisitUnpivot_in_clause([NotNull] PlSqlParser.Unpivot_in_clauseContext context) { return VisitChildren(context); }

        public Ust VisitUnpivot_in_elements([NotNull] PlSqlParser.Unpivot_in_elementsContext context) { return VisitChildren(context); }

        public Ust VisitHierarchical_query_clause([NotNull] PlSqlParser.Hierarchical_query_clauseContext context) { return VisitChildren(context); }

        public Ust VisitStart_part([NotNull] PlSqlParser.Start_partContext context) { return VisitChildren(context); }

        public Ust VisitGroup_by_clause([NotNull] PlSqlParser.Group_by_clauseContext context) { return VisitChildren(context); }

        public Ust VisitGroup_by_elements([NotNull] PlSqlParser.Group_by_elementsContext context) { return VisitChildren(context); }

        public Ust VisitRollup_cube_clause([NotNull] PlSqlParser.Rollup_cube_clauseContext context) { return VisitChildren(context); }

        public Ust VisitGrouping_sets_clause([NotNull] PlSqlParser.Grouping_sets_clauseContext context) { return VisitChildren(context); }

        public Ust VisitGrouping_sets_elements([NotNull] PlSqlParser.Grouping_sets_elementsContext context) { return VisitChildren(context); }

        public Ust VisitHaving_clause([NotNull] PlSqlParser.Having_clauseContext context) { return VisitChildren(context); }

        public Ust VisitModel_clause([NotNull] PlSqlParser.Model_clauseContext context) { return VisitChildren(context); }

        public Ust VisitCell_reference_options([NotNull] PlSqlParser.Cell_reference_optionsContext context) { return VisitChildren(context); }

        public Ust VisitReturn_rows_clause([NotNull] PlSqlParser.Return_rows_clauseContext context) { return VisitChildren(context); }

        public Ust VisitReference_model([NotNull] PlSqlParser.Reference_modelContext context) { return VisitChildren(context); }

        public Ust VisitMain_model([NotNull] PlSqlParser.Main_modelContext context) { return VisitChildren(context); }

        public Ust VisitModel_column_clauses([NotNull] PlSqlParser.Model_column_clausesContext context) { return VisitChildren(context); }

        public Ust VisitModel_column_partition_part([NotNull] PlSqlParser.Model_column_partition_partContext context) { return VisitChildren(context); }

        public Ust VisitModel_column_list([NotNull] PlSqlParser.Model_column_listContext context) { return VisitChildren(context); }

        public Ust VisitModel_column([NotNull] PlSqlParser.Model_columnContext context) { return VisitChildren(context); }

        public Ust VisitModel_rules_clause([NotNull] PlSqlParser.Model_rules_clauseContext context) { return VisitChildren(context); }

        public Ust VisitModel_rules_part([NotNull] PlSqlParser.Model_rules_partContext context) { return VisitChildren(context); }

        public Ust VisitModel_rules_element([NotNull] PlSqlParser.Model_rules_elementContext context) { return VisitChildren(context); }

        public Ust VisitCell_assignment([NotNull] PlSqlParser.Cell_assignmentContext context) { return VisitChildren(context); }

        public Ust VisitModel_iterate_clause([NotNull] PlSqlParser.Model_iterate_clauseContext context) { return VisitChildren(context); }

        public Ust VisitUntil_part([NotNull] PlSqlParser.Until_partContext context) { return VisitChildren(context); }

        public Ust VisitOrder_by_clause([NotNull] PlSqlParser.Order_by_clauseContext context) { return VisitChildren(context); }

        public Ust VisitOrder_by_elements([NotNull] PlSqlParser.Order_by_elementsContext context) { return VisitChildren(context); }

        public Ust VisitFor_update_clause([NotNull] PlSqlParser.For_update_clauseContext context) { return VisitChildren(context); }

        public Ust VisitFor_update_of_part([NotNull] PlSqlParser.For_update_of_partContext context) { return VisitChildren(context); }

        public Ust VisitFor_update_options([NotNull] PlSqlParser.For_update_optionsContext context) { return VisitChildren(context); }

        public Ust VisitUpdate_statement([NotNull] PlSqlParser.Update_statementContext context) { return VisitChildren(context); }

        public Ust VisitUpdate_set_clause([NotNull] PlSqlParser.Update_set_clauseContext context) { return VisitChildren(context); }

        public Ust VisitColumn_based_update_set_clause([NotNull] PlSqlParser.Column_based_update_set_clauseContext context) { return VisitChildren(context); }

        public Ust VisitDelete_statement([NotNull] PlSqlParser.Delete_statementContext context) { return VisitChildren(context); }

        public Ust VisitInsert_statement([NotNull] PlSqlParser.Insert_statementContext context) { return VisitChildren(context); }

        public Ust VisitSingle_table_insert([NotNull] PlSqlParser.Single_table_insertContext context) { return VisitChildren(context); }

        public Ust VisitMulti_table_insert([NotNull] PlSqlParser.Multi_table_insertContext context) { return VisitChildren(context); }

        public Ust VisitMulti_table_element([NotNull] PlSqlParser.Multi_table_elementContext context) { return VisitChildren(context); }

        public Ust VisitConditional_insert_clause([NotNull] PlSqlParser.Conditional_insert_clauseContext context) { return VisitChildren(context); }

        public Ust VisitConditional_insert_when_part([NotNull] PlSqlParser.Conditional_insert_when_partContext context) { return VisitChildren(context); }

        public Ust VisitConditional_insert_else_part([NotNull] PlSqlParser.Conditional_insert_else_partContext context) { return VisitChildren(context); }

        public Ust VisitInsert_into_clause([NotNull] PlSqlParser.Insert_into_clauseContext context) { return VisitChildren(context); }

        public Ust VisitValues_clause([NotNull] PlSqlParser.Values_clauseContext context) { return VisitChildren(context); }

        public Ust VisitMerge_statement([NotNull] PlSqlParser.Merge_statementContext context) { return VisitChildren(context); }

        public Ust VisitMerge_update_clause([NotNull] PlSqlParser.Merge_update_clauseContext context) { return VisitChildren(context); }

        public Ust VisitMerge_element([NotNull] PlSqlParser.Merge_elementContext context) { return VisitChildren(context); }

        public Ust VisitMerge_update_delete_part([NotNull] PlSqlParser.Merge_update_delete_partContext context) { return VisitChildren(context); }

        public Ust VisitMerge_insert_clause([NotNull] PlSqlParser.Merge_insert_clauseContext context) { return VisitChildren(context); }

        public Ust VisitSelected_tableview([NotNull] PlSqlParser.Selected_tableviewContext context) { return VisitChildren(context); }

        public Ust VisitLock_table_statement([NotNull] PlSqlParser.Lock_table_statementContext context) { return VisitChildren(context); }

        public Ust VisitWait_nowait_part([NotNull] PlSqlParser.Wait_nowait_partContext context) { return VisitChildren(context); }

        public Ust VisitLock_table_element([NotNull] PlSqlParser.Lock_table_elementContext context) { return VisitChildren(context); }

        public Ust VisitLock_mode([NotNull] PlSqlParser.Lock_modeContext context) { return VisitChildren(context); }

        public Ust VisitGeneral_table_ref([NotNull] PlSqlParser.General_table_refContext context) { return VisitChildren(context); }

        public Ust VisitStatic_returning_clause([NotNull] PlSqlParser.Static_returning_clauseContext context) { return VisitChildren(context); }

        public Ust VisitError_logging_clause([NotNull] PlSqlParser.Error_logging_clauseContext context) { return VisitChildren(context); }

        public Ust VisitError_logging_into_part([NotNull] PlSqlParser.Error_logging_into_partContext context) { return VisitChildren(context); }

        public Ust VisitError_logging_reject_part([NotNull] PlSqlParser.Error_logging_reject_partContext context) { return VisitChildren(context); }

        public Ust VisitDml_table_expression_clause([NotNull] PlSqlParser.Dml_table_expression_clauseContext context) { return VisitChildren(context); }

        public Ust VisitTable_collection_expression([NotNull] PlSqlParser.Table_collection_expressionContext context) { return VisitChildren(context); }

        public Ust VisitSubquery_restriction_clause([NotNull] PlSqlParser.Subquery_restriction_clauseContext context) { return VisitChildren(context); }

        public Ust VisitSample_clause([NotNull] PlSqlParser.Sample_clauseContext context) { return VisitChildren(context); }

        public Ust VisitSeed_part([NotNull] PlSqlParser.Seed_partContext context) { return VisitChildren(context); }

        public Ust VisitCursor_expression([NotNull] PlSqlParser.Cursor_expressionContext context) { return VisitChildren(context); }

        public Ust VisitExpression_list([NotNull] PlSqlParser.Expression_listContext context) { return VisitChildren(context); }

        public Ust VisitCondition([NotNull] PlSqlParser.ConditionContext context) { return VisitChildren(context); }

        public Ust VisitExpression([NotNull] PlSqlParser.ExpressionContext context) { return VisitChildren(context); }

        /// <returns><see cref="Expression"/></returns>
        public Ust VisitLogical_or_expression([NotNull] PlSqlParser.Logical_or_expressionContext context)
        {
            Expression result;
            if (context.OR() != null)
            {
                var left = (Expression)Visit(context.logical_or_expression());
                var right = (Expression)Visit(context.logical_and_expression());
                var opLiteral = new BinaryOperatorLiteral(BinaryOperator.LogicalOr, context.GetTextSpan());
                result = new BinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan());
            }
            else
            {
                result = (Expression)Visit(context.logical_and_expression());
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public Ust VisitLogical_and_expression([NotNull] PlSqlParser.Logical_and_expressionContext context)
        {
            Expression result;
            if (context.AND() != null)
            {
                var left = (Expression)Visit(context.negated_expression());
                var right = (Expression)Visit(context.logical_and_expression());
                var opLiteral = new BinaryOperatorLiteral(BinaryOperator.LogicalAnd, context.GetTextSpan());
                result = new BinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan());
            }
            else
            {
                result = (Expression)Visit(context.negated_expression());
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public Ust VisitNegated_expression([NotNull] PlSqlParser.Negated_expressionContext context)
        {
            Expression result;
            if (context.NOT() != null)
            {
                var opLiteral = new UnaryOperatorLiteral(UnaryOperator.Not, context.NOT().GetTextSpan());
                result = new UnaryOperatorExpression(opLiteral, (Expression)Visit(context.negated_expression()), context.GetTextSpan());
            }
            else
            {
                result = (Expression)Visit(context.equality_expression());
            }
            return result;
        }

        public Ust VisitEquality_expression([NotNull] PlSqlParser.Equality_expressionContext context) { return VisitChildren(context); }

        public Ust VisitMultiset_expression([NotNull] PlSqlParser.Multiset_expressionContext context) { return VisitChildren(context); }

        public Ust VisitMultiset_type([NotNull] PlSqlParser.Multiset_typeContext context) { return VisitChildren(context); }

        /// <returns><see cref="Expression"/></returns>
        public Ust VisitRelational_expression([NotNull] PlSqlParser.Relational_expressionContext context)
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
                result = new BinaryOperatorExpression(left, op, right, context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitCompound_expression([NotNull] PlSqlParser.Compound_expressionContext context) { return VisitChildren(context); }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public Ust VisitRelational_operator([NotNull] PlSqlParser.Relational_operatorContext context)
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
                result = new BinaryOperatorLiteral(literal, context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitLike_type([NotNull] PlSqlParser.Like_typeContext context) { return VisitChildren(context); }

        public Ust VisitLike_escape_part([NotNull] PlSqlParser.Like_escape_partContext context) { return VisitChildren(context); }

        public Ust VisitIn_elements([NotNull] PlSqlParser.In_elementsContext context) { return VisitChildren(context); }

        public Ust VisitBetween_elements([NotNull] PlSqlParser.Between_elementsContext context) { return VisitChildren(context); }

        public Ust VisitConcatenation([NotNull] PlSqlParser.ConcatenationContext context)
        {
            var result = (Expression)Visit(context.additive_expression(0));
            if (context.additive_expression().Length > 1)
            {
                for (int i = 1; i < context.additive_expression().Length; i++)
                {
                    var right = (Expression)Visit(context.additive_expression(i));
                    result = new BinaryOperatorExpression(result, (BinaryOperatorLiteral)Visit(context.concatenation_op(i - 1)), right,
                        result.TextSpan.Union(right.TextSpan));
                }
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public Ust VisitAdditive_expression([NotNull] PlSqlParser.Additive_expressionContext context)
        {
            var result = (Expression)Visit(context.multiply_expression(0));
            if (context.multiply_expression().Length > 1)
            {
                for (int i = 1; i < context.multiply_expression().Length; i++)
                {
                    var op = new BinaryOperatorLiteral(BinaryOperatorLiteral.TextBinaryOperator[context._op[i - 1].Text],
                      context._op[i - 1].GetTextSpan());
                    var right = (Expression)Visit(context.multiply_expression(i));
                    result = new BinaryOperatorExpression(result, op, right, result.TextSpan.Union(right.TextSpan));
                }
            }
            return result;
        }

        /// <returns><see cref="Expression"/></returns>
        public Ust VisitMultiply_expression([NotNull] PlSqlParser.Multiply_expressionContext context)
        {
            var result = (Expression)Visit(context.datetime_expression(0));
            if (context.datetime_expression().Length > 1)
            {
                for (int i = 1; i < context.datetime_expression().Length; i++)
                {
                    var right = (Expression)Visit(context.datetime_expression(i));
                    var op  = new BinaryOperatorLiteral(BinaryOperatorLiteral.TextBinaryOperator[context._op[i - 1].Text],
                        context._op[i - 1].GetTextSpan());
                    result = new BinaryOperatorExpression(result, op, right, result.TextSpan.Union(right.TextSpan));
                }
            }
            return result;
        }

        public Ust VisitDatetime_expression([NotNull] PlSqlParser.Datetime_expressionContext context) { return VisitChildren(context); }

        public Ust VisitInterval_expression([NotNull] PlSqlParser.Interval_expressionContext context) { return VisitChildren(context); }

        public Ust VisitModel_expression([NotNull] PlSqlParser.Model_expressionContext context) { return VisitChildren(context); }

        public Ust VisitModel_expression_element([NotNull] PlSqlParser.Model_expression_elementContext context) { return VisitChildren(context); }

        public Ust VisitSingle_column_for_loop([NotNull] PlSqlParser.Single_column_for_loopContext context) { return VisitChildren(context); }

        public Ust VisitFor_like_part([NotNull] PlSqlParser.For_like_partContext context) { return VisitChildren(context); }

        public Ust VisitFor_increment_decrement_type([NotNull] PlSqlParser.For_increment_decrement_typeContext context) { return VisitChildren(context); }

        public Ust VisitMulti_column_for_loop([NotNull] PlSqlParser.Multi_column_for_loopContext context) { return VisitChildren(context); }

        public Ust VisitUnary_expression([NotNull] PlSqlParser.Unary_expressionContext context) { return VisitChildren(context); }

        public Ust VisitCase_statement([NotNull] PlSqlParser.Case_statementContext context) { return VisitChildren(context); }

        public Ust VisitSimple_case_statement([NotNull] PlSqlParser.Simple_case_statementContext context) { return VisitChildren(context); }

        public Ust VisitSimple_case_when_part([NotNull] PlSqlParser.Simple_case_when_partContext context) { return VisitChildren(context); }

        public Ust VisitSearched_case_statement([NotNull] PlSqlParser.Searched_case_statementContext context) { return VisitChildren(context); }

        public Ust VisitSearched_case_when_part([NotNull] PlSqlParser.Searched_case_when_partContext context) { return VisitChildren(context); }

        public Ust VisitCase_else_part([NotNull] PlSqlParser.Case_else_partContext context) { return VisitChildren(context); }

        public Ust VisitAtom([NotNull] PlSqlParser.AtomContext context) { return VisitChildren(context); }

        public Ust VisitExpression_or_vector([NotNull] PlSqlParser.Expression_or_vectorContext context) { return VisitChildren(context); }

        public Ust VisitVector_expr([NotNull] PlSqlParser.Vector_exprContext context) { return VisitChildren(context); }

        public Ust VisitQuantified_expression([NotNull] PlSqlParser.Quantified_expressionContext context) { return VisitChildren(context); }

        public Ust VisitStandard_function([NotNull] PlSqlParser.Standard_functionContext context) { return VisitChildren(context); }

        public Ust VisitOver_clause_keyword([NotNull] PlSqlParser.Over_clause_keywordContext context) { return VisitChildren(context); }

        public Ust VisitWithin_or_over_clause_keyword([NotNull] PlSqlParser.Within_or_over_clause_keywordContext context) { return VisitChildren(context); }

        public Ust VisitStandard_prediction_function_keyword([NotNull] PlSqlParser.Standard_prediction_function_keywordContext context) { return VisitChildren(context); }

        public Ust VisitOver_clause([NotNull] PlSqlParser.Over_clauseContext context) { return VisitChildren(context); }

        public Ust VisitWindowing_clause([NotNull] PlSqlParser.Windowing_clauseContext context) { return VisitChildren(context); }

        public Ust VisitWindowing_type([NotNull] PlSqlParser.Windowing_typeContext context) { return VisitChildren(context); }

        public Ust VisitWindowing_elements([NotNull] PlSqlParser.Windowing_elementsContext context) { return VisitChildren(context); }

        public Ust VisitUsing_clause([NotNull] PlSqlParser.Using_clauseContext context) { return VisitChildren(context); }

        public Ust VisitUsing_element([NotNull] PlSqlParser.Using_elementContext context) { return VisitChildren(context); }

        public Ust VisitCollect_order_by_part([NotNull] PlSqlParser.Collect_order_by_partContext context) { return VisitChildren(context); }

        public Ust VisitWithin_or_over_part([NotNull] PlSqlParser.Within_or_over_partContext context) { return VisitChildren(context); }

        public Ust VisitCost_matrix_clause([NotNull] PlSqlParser.Cost_matrix_clauseContext context) { return VisitChildren(context); }

        public Ust VisitXml_passing_clause([NotNull] PlSqlParser.Xml_passing_clauseContext context) { return VisitChildren(context); }

        public Ust VisitXml_attributes_clause([NotNull] PlSqlParser.Xml_attributes_clauseContext context) { return VisitChildren(context); }

        public Ust VisitXml_namespaces_clause([NotNull] PlSqlParser.Xml_namespaces_clauseContext context) { return VisitChildren(context); }

        public Ust VisitXml_table_column([NotNull] PlSqlParser.Xml_table_columnContext context) { return VisitChildren(context); }

        public Ust VisitXml_general_default_part([NotNull] PlSqlParser.Xml_general_default_partContext context) { return VisitChildren(context); }

        public Ust VisitXml_multiuse_expression_element([NotNull] PlSqlParser.Xml_multiuse_expression_elementContext context) { return VisitChildren(context); }

        public Ust VisitXmlroot_param_version_part([NotNull] PlSqlParser.Xmlroot_param_version_partContext context) { return VisitChildren(context); }

        public Ust VisitXmlroot_param_standalone_part([NotNull] PlSqlParser.Xmlroot_param_standalone_partContext context) { return VisitChildren(context); }

        public Ust VisitXmlserialize_param_enconding_part([NotNull] PlSqlParser.Xmlserialize_param_enconding_partContext context) { return VisitChildren(context); }

        public Ust VisitXmlserialize_param_version_part([NotNull] PlSqlParser.Xmlserialize_param_version_partContext context) { return VisitChildren(context); }

        public Ust VisitXmlserialize_param_ident_part([NotNull] PlSqlParser.Xmlserialize_param_ident_partContext context) { return VisitChildren(context); }

        public Ust VisitSql_plus_command([NotNull] PlSqlParser.Sql_plus_commandContext context) { return VisitChildren(context); }

        public Ust VisitWhenever_command([NotNull] PlSqlParser.Whenever_commandContext context) { return VisitChildren(context); }

        public Ust VisitSet_command([NotNull] PlSqlParser.Set_commandContext context) { return VisitChildren(context); }

        public Ust VisitExit_command([NotNull] PlSqlParser.Exit_commandContext context) { return VisitChildren(context); }

        public Ust VisitPrompt_command([NotNull] PlSqlParser.Prompt_commandContext context) { return VisitChildren(context); }

        public Ust VisitPartition_extension_clause([NotNull] PlSqlParser.Partition_extension_clauseContext context) { return VisitChildren(context); }

        public Ust VisitColumn_alias([NotNull] PlSqlParser.Column_aliasContext context) { return VisitChildren(context); }

        public Ust VisitTable_alias([NotNull] PlSqlParser.Table_aliasContext context) { return VisitChildren(context); }

        public Ust VisitAlias_quoted_string([NotNull] PlSqlParser.Alias_quoted_stringContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWhere_clause([NotNull] PlSqlParser.Where_clauseContext context) { return VisitChildren(context); }

        public Ust VisitCurrent_of_clause([NotNull] PlSqlParser.Current_of_clauseContext context) { return VisitChildren(context); }

        public Ust VisitInto_clause([NotNull] PlSqlParser.Into_clauseContext context) { return VisitChildren(context); }

        public Ust VisitXml_column_name([NotNull] PlSqlParser.Xml_column_nameContext context) { return VisitChildren(context); }

        public Ust VisitCost_class_name([NotNull] PlSqlParser.Cost_class_nameContext context) { return VisitChildren(context); }

        public Ust VisitAttribute_name([NotNull] PlSqlParser.Attribute_nameContext context) { return VisitChildren(context); }

        public Ust VisitSavepoint_name([NotNull] PlSqlParser.Savepoint_nameContext context) { return VisitChildren(context); }

        public Ust VisitRollback_segment_name([NotNull] PlSqlParser.Rollback_segment_nameContext context) { return VisitChildren(context); }

        public Ust VisitTable_var_name([NotNull] PlSqlParser.Table_var_nameContext context) { return VisitChildren(context); }

        public Ust VisitSchema_name([NotNull] PlSqlParser.Schema_nameContext context) { return VisitChildren(context); }

        public Ust VisitRoutine_name([NotNull] PlSqlParser.Routine_nameContext context)
        {
            Expression result = (IdToken)Visit(context.identifier());
            if (context.id_expression().Length > 0)
            {
                var firstSpan = context.identifier().GetTextSpan();
                for (int i = 0; i < context.id_expression().Length; i++)
                {
                    result = new MemberReferenceExpression(result,
                        (Expression)Visit(context.id_expression(i)), firstSpan.Union(context.id_expression(i).GetTextSpan()));
                }
            }
            return result;
        }

        public Ust VisitPackage_name([NotNull] PlSqlParser.Package_nameContext context) { return VisitChildren(context); }

        public Ust VisitImplementation_type_name([NotNull] PlSqlParser.Implementation_type_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitParameter_name([NotNull] PlSqlParser.Parameter_nameContext context) { return VisitChildren(context); }

        public Ust VisitReference_model_name([NotNull] PlSqlParser.Reference_model_nameContext context) { return VisitChildren(context); }

        public Ust VisitMain_model_name([NotNull] PlSqlParser.Main_model_nameContext context) { return VisitChildren(context); }

        public Ust VisitAggregate_function_name([NotNull] PlSqlParser.Aggregate_function_nameContext context) { return VisitChildren(context); }

        public Ust VisitQuery_name([NotNull] PlSqlParser.Query_nameContext context) { return VisitChildren(context); }

        public Ust VisitConstraint_name([NotNull] PlSqlParser.Constraint_nameContext context) { return VisitChildren(context); }

        public Ust VisitLabel_name([NotNull] PlSqlParser.Label_nameContext context) { return VisitChildren(context); }

        public Ust VisitType_name([NotNull] PlSqlParser.Type_nameContext context)
        {
            var typeName = new TypeToken(
                string.Join(".", context.id_expression().Select(expr => ((IdToken)Visit(expr)).TextValue)),
                context.GetTextSpan());
            return typeName;
        }

        public Ust VisitSequence_name([NotNull] PlSqlParser.Sequence_nameContext context) { return VisitChildren(context); }

        /// <returns><see cref="TypeToken"/></returns>
        public Ust VisitException_name([NotNull] PlSqlParser.Exception_nameContext context)
        {
            var result = new TypeToken(context.GetText(), context.GetTextSpan());
            return result;
        }

        public Ust VisitFunction_name([NotNull] PlSqlParser.Function_nameContext context) { return VisitChildren(context); }

        public Ust VisitProcedure_name([NotNull] PlSqlParser.Procedure_nameContext context) { return VisitChildren(context); }

        public Ust VisitTrigger_name([NotNull] PlSqlParser.Trigger_nameContext context) { return VisitChildren(context); }

        public Ust VisitVariable_name([NotNull] PlSqlParser.Variable_nameContext context) { return VisitChildren(context); }

        public Ust VisitIndex_name([NotNull] PlSqlParser.Index_nameContext context) { return VisitChildren(context); }

        public Ust VisitCursor_name([NotNull] PlSqlParser.Cursor_nameContext context) { return VisitChildren(context); }

        public Ust VisitRecord_name([NotNull] PlSqlParser.Record_nameContext context) { return VisitChildren(context); }

        public Ust VisitCollection_name([NotNull] PlSqlParser.Collection_nameContext context) { return VisitChildren(context); }

        public Ust VisitLink_name([NotNull] PlSqlParser.Link_nameContext context) { return VisitChildren(context); }

        public Ust VisitColumn_name([NotNull] PlSqlParser.Column_nameContext context) { return VisitChildren(context); }

        public Ust VisitTableview_name([NotNull] PlSqlParser.Tableview_nameContext context) { return VisitChildren(context); }

        public Ust VisitChar_set_name([NotNull] PlSqlParser.Char_set_nameContext context) { return VisitChildren(context); }

        public Ust VisitKeep_clause([NotNull] PlSqlParser.Keep_clauseContext context) { return VisitChildren(context); }

        /// <returns><see cref="ArgsUst"/></returns>
        public Ust VisitFunction_argument([NotNull] PlSqlParser.Function_argumentContext context)
        {
            List<Expression> args = context.argument().Select(arg => (Expression)Visit(arg)).ToList();
            if (context.keep_clause() != null)
            {
                args.Add((Expression)Visit(context.keep_clause()));
            }
            var result = new ArgsUst(args, context.GetTextSpan());
            return result;
        }

        public Ust VisitFunction_argument_analytic([NotNull] PlSqlParser.Function_argument_analyticContext context) { return VisitChildren(context); }

        public Ust VisitFunction_argument_modeling([NotNull] PlSqlParser.Function_argument_modelingContext context) { return VisitChildren(context); }

        public Ust VisitRespect_or_ignore_nulls([NotNull] PlSqlParser.Respect_or_ignore_nullsContext context) { return VisitChildren(context); }

        public Ust VisitArgument([NotNull] PlSqlParser.ArgumentContext context) { return VisitChildren(context); }

        /// <returns><see cref="TypeToken"/></returns>
        public Ust VisitType_spec([NotNull] PlSqlParser.Type_specContext context)
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
        public Ust VisitDatatype([NotNull] PlSqlParser.DatatypeContext context)
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

        public Ust VisitPrecision_part([NotNull] PlSqlParser.Precision_partContext context) { return VisitChildren(context); }

        /// <returns><see cref="TypeToken"/></returns>
        public Ust VisitNative_datatype_element([NotNull] PlSqlParser.Native_datatype_elementContext context)
        {
            return new TypeToken(context.GetText(), context.GetTextSpan());
        }

        public Ust VisitBind_variable([NotNull] PlSqlParser.Bind_variableContext context)
        {
            var result = VisitChildren(context);
            if (result == null)
            {
                result = new NullLiteral(context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitGeneral_element([NotNull] PlSqlParser.General_elementContext context) { return VisitChildren(context); }

        public Ust VisitGeneral_element_part([NotNull] PlSqlParser.General_element_partContext context)
        {
            Expression result = (Expression)Visit(context.id_expression(0));
            var firstSpan = context.id_expression(0).GetTextSpan();
            for (int i = 1; i < context.id_expression().Length; i++)
            {
                result = new MemberReferenceExpression(result, (Expression)Visit(context.id_expression(i)),
                    firstSpan.Union(context.id_expression(i).GetTextSpan()));
            }
            if (context.function_argument() != null)
            {
                var argsNode = (ArgsUst)Visit(context.function_argument());
                result = new InvocationExpression(result, argsNode, firstSpan.Union(argsNode.TextSpan));
            }
            return result;
        }

        public Ust VisitTable_element([NotNull] PlSqlParser.Table_elementContext context) { return VisitChildren(context); }

        /// <returns><see cref="Token"/></returns>
        public Ust VisitConstant([NotNull] PlSqlParser.ConstantContext context)
        {
            Token result;
            if (context.NULL() != null)
            {
                result = new NullLiteral(context.GetTextSpan());
            }
            else if (context.TRUE() != null || context.FALSE() != null)
            {
                result = new BooleanLiteral(bool.Parse(context.GetText().ToLowerInvariant()), context.GetTextSpan());
            }
            else
            {
                result = (Token)Visit(context.GetChild(0));
            }
            return result;
        }

        public Ust VisitNumeric([NotNull] PlSqlParser.NumericContext context)
        {
            Token result;
            string text = context.GetText();
            if (context.UNSIGNED_INTEGER() != null)
            {
                result = new IntLiteral(long.Parse(text), context.GetTextSpan());
            }
            else
            {
                text = text.ToLowerInvariant().Replace("f", "").Replace("d", "");
                result = new FloatLiteral(double.Parse(text), context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitQuoted_string([NotNull] PlSqlParser.Quoted_stringContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitId([NotNull] PlSqlParser.IdentifierContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="IdToken"/></returns>
        public Ust VisitId_expression([NotNull] PlSqlParser.Id_expressionContext context)
        {
            IdToken result;
            if (context.regular_id() != null)
            {
                result = (IdToken)Visit(context.GetChild(0));
            }
            else
            {
                string text = context.GetText();
                result = new IdToken(text.Substring(1, text.Length - 2), context.GetTextSpan());
            }
            return result;
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public Ust VisitNot_equal_op([NotNull] PlSqlParser.Not_equal_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.NotEqual, context.GetTextSpan());
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public Ust VisitGreater_than_or_equals_op([NotNull] PlSqlParser.Greater_than_or_equals_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.GreaterOrEqual, context.GetTextSpan());
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public Ust VisitLess_than_or_equals_op([NotNull] PlSqlParser.Less_than_or_equals_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.LessOrEqual, context.GetTextSpan());
        }

        public Ust VisitConcatenation_op([NotNull] PlSqlParser.Concatenation_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.Plus, context.GetTextSpan());
        }

        public Ust VisitOuter_join_sign([NotNull] PlSqlParser.Outer_join_signContext context) { return VisitChildren(context); }

        public Ust VisitRegular_id([NotNull] PlSqlParser.Regular_idContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShow_errors_command([NotNull] PlSqlParser.Show_errors_commandContext context) { return VisitChildren(context); }

        public Ust VisitNumeric_negative([NotNull] PlSqlParser.Numeric_negativeContext context) { return VisitChildren(context); }

        public Ust VisitTable_ref_aux_internal_three([NotNull] PlSqlParser.Table_ref_aux_internal_threeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTable_ref_aux_internal_one([NotNull] PlSqlParser.Table_ref_aux_internal_oneContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTable_ref_aux_internal_two([NotNull] PlSqlParser.Table_ref_aux_internal_twoContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_package_body([NotNull] PlSqlParser.Create_package_bodyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFunction_body([NotNull] PlSqlParser.Function_bodyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitProcedure_body([NotNull] PlSqlParser.Procedure_bodyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRecord_type_def([NotNull] PlSqlParser.Record_type_defContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRef_cursor_type_def([NotNull] PlSqlParser.Ref_cursor_type_defContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitType_declaration([NotNull] PlSqlParser.Type_declarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTable_type_def([NotNull] PlSqlParser.Table_type_defContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTable_ref_aux_internal([NotNull] PlSqlParser.Table_ref_aux_internalContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitString_function([NotNull] PlSqlParser.String_functionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNumeric_function_wrapper([NotNull] PlSqlParser.Numeric_function_wrapperContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNumeric_function([NotNull] PlSqlParser.Numeric_functionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOther_function([NotNull] PlSqlParser.Other_functionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIdentifier([NotNull] PlSqlParser.IdentifierContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitString_function_name([NotNull] PlSqlParser.String_function_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNumeric_function_name([NotNull] PlSqlParser.Numeric_function_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_table([NotNull] PlSqlParser.Create_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDrop_table([NotNull] PlSqlParser.Drop_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnonymous_block([NotNull] PlSqlParser.Anonymous_blockContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStart_command([NotNull] PlSqlParser.Start_commandContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComment_on_column([NotNull] PlSqlParser.Comment_on_columnContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_synonym([NotNull] PlSqlParser.Create_synonymContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComment_on_table([NotNull] PlSqlParser.Comment_on_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSynonym_name([NotNull] PlSqlParser.Synonym_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSchema_object_name([NotNull] PlSqlParser.Schema_object_nameContext context)
        {
            return VisitChildren(context);
        }
    }
}
