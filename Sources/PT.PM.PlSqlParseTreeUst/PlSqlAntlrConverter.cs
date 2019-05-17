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
using System.Globalization;
using System.Linq;
using System.Numerics;
using Antlr4.Runtime;
using static PT.PM.Common.Nodes.UstUtils;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrConverter : AntlrConverter, IPlSqlParserVisitor<Ust>
    {
        public override Language Language => Language.PlSql;

        public static PlSqlAntlrConverter Create() => new PlSqlAntlrConverter();

        /// <returns><see cref="ArgsUst"/></returns>
        public Ust VisitSwallow_to_semi([NotNull] PlSqlParser.Swallow_to_semiContext context)
        {
            Expression[] args = context.children.Select(c => (Expression)Visit(c)).ToArray();
            var result = new ArgsUst(args, context.GetTextSpan());
            return result;
        }

        public Ust VisitSql_script([NotNull] PlSqlParser.Sql_scriptContext context)
        {
            var statements = context.unit_statement().Select(statement => (Statement)Visit(statement));
            var block = new BlockStatement(statements, context.GetTextSpan());
            root.Nodes = new Ust[] { block };
            return root;
        }

        /// <returns><see cref="Statement"/></returns>
        public Ust VisitUnit_statement([NotNull] PlSqlParser.Unit_statementContext context)
        {
            var child = Visit(context.GetChild(0));
            Statement result = child.AsStatement();
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

        public Ust VisitProcedure_call([NotNull] PlSqlParser.Procedure_callContext context)
        {
            var function = new InvocationExpression(context.GetTextSpan())
            {
                Target = (Expression)Visit(context.routine_name())
            };

            var argumentsContext = context.function_argument();
            var arguments = new List<Expression>();

            for (int i = 0; i < (argumentsContext?.ChildCount ?? 0); i++)
            {
                arguments.Add((Expression)Visit(argumentsContext.argument(i)));
            }

            function.Arguments = new ArgsUst(arguments);
            return function;
        }

        public Ust VisitAlter_session([NotNull] PlSqlParser.Alter_sessionContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="AssignmentExpression"/></returns>
        public Ust VisitAlter_session_set_clause([NotNull] PlSqlParser.Alter_session_set_clauseContext context)
        {
            var assignmentExpr = new AssignmentExpression(context.GetTextSpan())
            {
                Right = (Expression)Visit(context.parameter_name()),
                Left = (Expression)Visit(context.parameter_value())
            };
            return assignmentExpr;
        }

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
            var decls = (BlockStatement)Visit(context.seq_of_declare_specs());
            var body = (BlockStatement)Visit(context.body());

            if (decls != null)
            {
                body.Statements.InsertRange(0, decls.Statements);
            }

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

        public Ust VisitOverriding_subprogram_spec([NotNull] PlSqlParser.Overriding_subprogram_specContext context) { return VisitChildren(context); }

        public Ust VisitOverriding_function_spec([NotNull] PlSqlParser.Overriding_function_specContext context) { return VisitChildren(context); }

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
            result = new ParameterDeclaration(null, typeToken, name, context.GetTextSpan());
            return result;
        }

        public Ust VisitDefault_value_part([NotNull] PlSqlParser.Default_value_partContext context)
        {
            if (context.GetChild(0).GetText() == ":=")
            {
                return Visit(context.GetChild(1));
            }
            return VisitChildren(context);
        }

        public Ust VisitDeclare_spec([NotNull] PlSqlParser.Declare_specContext context) { return VisitChildren(context); }

        public Ust VisitVariable_declaration([NotNull] PlSqlParser.Variable_declarationContext context)
        {
            var identifier = context.identifier();
            var left = new IdToken(identifier.GetText(), identifier.GetTextSpan());
            var typeName = context.type_spec();
            var defaultValue = context.default_value_part();
            var right = (Expression)Visit(defaultValue);
            var result = new VariableDeclarationExpression(
                new TypeToken(typeName.GetText(), typeName.GetTextSpan()),
                new List<AssignmentExpression> { new AssignmentExpression
                {
                    Left = left,
                    Right = right
                }},
                context.GetTextSpan());
            return result;
        }

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
            Ust result = Visit(context.GetChild(0));
            return result.AsStatement();
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
                result = new WhileStatement(textSpan)
                {
                    Embedded = block
                };
            }
            return result;
        }

        public Ust VisitCursor_loop_param([NotNull] PlSqlParser.Cursor_loop_paramContext context)
        {
            return VisitChildren(context);
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
            var declare = new BlockStatement(context.declare_spec().Select(s => Visit(s).AsStatement()).ToArray(),
                context.GetTextSpan());
            var body = (BlockStatement)Visit(context.body());

            var result = new BlockStatement(new Statement[] { declare, body }, context.GetTextSpan());
            return result;
        }

        public Ust VisitSql_statement([NotNull] PlSqlParser.Sql_statementContext context) { return VisitChildren(context); }

        public Ust VisitExecute_immediate([NotNull] PlSqlParser.Execute_immediateContext context)
        {
            var args = (Expression)VisitChildren(context.expression());
            var invocation = new InvocationExpression(context.GetTextSpan())
            {
                Target = new IdToken(context.EXECUTE().GetText(), context.EXECUTE().GetTextSpan()),
                Arguments = new ArgsUst(args)
            };

            if (context.using_clause() != null)
            {
                invocation.Arguments.Collection.Add((Expression)Visit(context.using_clause()));
            }

            if (context.dynamic_returning_clause() != null)
            {
                invocation.Arguments.Collection.AddRange(ExtractMultiChild((MultichildExpression)Visit(context.dynamic_returning_clause())));
            }

            if (context.into_clause() != null)
            {
                invocation.Arguments.Collection.AddRange(ExtractMultiChild((MultichildExpression)Visit(context.into_clause())));
            }
            return invocation;
        }

        public Ust VisitDynamic_returning_clause([NotNull] PlSqlParser.Dynamic_returning_clauseContext context) { return VisitChildren(context); }

        public Ust VisitData_manipulation_language_statements([NotNull] PlSqlParser.Data_manipulation_language_statementsContext context) { return VisitChildren(context); }

        public Ust VisitCursor_manipulation_statements([NotNull] PlSqlParser.Cursor_manipulation_statementsContext context) { return VisitChildren(context); }

        public Ust VisitOpen_statement([NotNull] PlSqlParser.Open_statementContext context)
        {
            var invocation = new InvocationExpression(context.GetTextSpan())
            {
                Target = new IdToken(context.OPEN().GetText(), context.OPEN().GetTextSpan()),
                Arguments = new ArgsUst((Expression)Visit(context.cursor_name()))
            };

            if (context.expressions() != null)
            {
                invocation.Arguments.Collection.Add((Expression)Visit(context.expressions()));
            }

            return invocation;
        }

        /// <summary>
        /// Convert OPEN cursorName FOR [QUERY|expression] to InvocationExpression
        /// </summary>
        public Ust VisitOpen_for_statement([NotNull] PlSqlParser.Open_for_statementContext context)
        {
            var invocation = new InvocationExpression(context.GetTextSpan())
            {
                Target = new IdToken(context.OPEN().GetText(), context.OPEN().GetTextSpan()),
                Arguments = new ArgsUst((Expression)Visit(context.variable_name()))
            };

            if (context.select_statement() != null)
            {
                invocation.Arguments.Collection.Add((Expression)Visit(context.select_statement()));
            }

            if (context.expression() != null)
            {
                invocation.Arguments.Collection.Add((Expression)Visit(context.expression()));
            }

            return invocation;
        }

        public Ust VisitFetch_statement([NotNull] PlSqlParser.Fetch_statementContext context)
        {
            var variables = context.variable_name();
            var argList = new List<Expression>(1 + variables.Length);

            argList.Add((Expression)Visit(context.cursor_name()));

            for (int i = 0; i < variables.Length; i++)
            {
                argList.Add((Expression) Visit(variables[i]));
            }

            return new InvocationExpression(context.GetTextSpan())
            {
                Target = new IdToken(context.FETCH().GetText(), context.FETCH().GetTextSpan()),
                Arguments = new ArgsUst(argList)
            };
        }

        public Ust VisitClose_statement([NotNull] PlSqlParser.Close_statementContext context)
        {
            var cursorArg = (Expression) Visit(context.cursor_name());

            var invocation = new InvocationExpression(context.GetTextSpan())
            {
                Target = new IdToken(context.CLOSE().GetText(), context.CLOSE().GetTextSpan()),
                Arguments = new ArgsUst(cursorArg)
            };

            return invocation;
        }

        public Ust VisitCursor_name([NotNull] PlSqlParser.Cursor_nameContext context)
        {
            return ConvertToInOutArg(context);
        }

        public Ust VisitVariable_name([NotNull] PlSqlParser.Variable_nameContext context)
        {
            return ConvertToInOutArg(context);
        }

        private static ArgumentExpression ConvertToInOutArg(ParserRuleContext context)
        {
            var argModifier = new InOutModifierLiteral(InOutModifier.InOut, TextSpan.Zero);
            TextSpan contextTextSpan = context.GetTextSpan();
            var arg = new IdToken(context.GetText(), contextTextSpan);
            return new ArgumentExpression(argModifier, arg, contextTextSpan);
        }

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

        public Ust VisitQuery_block([NotNull] PlSqlParser.Query_blockContext context)
        {
            var query = new InvocationExpression(context.GetTextSpan())
            {
                Target = new IdToken(context.SELECT().GetText(), context.SELECT().GetTextSpan())
            };
            var queryElements = new List<Expression>();
            for (int i = 1; i < context.ChildCount; i++)
            {
                var visited = Visit(context.GetChild(i));
                if (visited is Collection collection)
                {
                    queryElements.AddRange(collection.Collection.Select(x => (Expression)x));
                }
                else if (visited is MultichildExpression multichild)
                {
                    queryElements.AddRange(ExtractMultiChild(multichild));
                }
                else
                {
                    queryElements.Add((Expression)visited);
                }
            }
            query.Arguments = new ArgsUst(queryElements);
            return query;
        }

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

        public Ust VisitCondition([NotNull] PlSqlParser.ConditionContext context) { return VisitChildren(context); }

        public Ust VisitExpression([NotNull] PlSqlParser.ExpressionContext context) { return VisitChildren(context); }

        /// <returns><see cref="Expression"/></returns>
        public Ust VisitLogical_expression([NotNull] PlSqlParser.Logical_expressionContext context)
        {
            Expression result;
            if (context.multiset_expression() != null)
            {
                return VisitChildren(context);
            }
            if (context.NOT().Length == 2)
            {
                var opLiteral = new UnaryOperatorLiteral(UnaryOperator.Not, context.NOT(0).GetTextSpan());
                result = new UnaryOperatorExpression(opLiteral,
                    (Expression)Visit(context.logical_expression(0)), context.GetTextSpan());
            }
            else
            {
                // AND, OR
                var left = (Expression)Visit(context.logical_expression(0));
                var right = (Expression)Visit(context.logical_expression(1));
                BinaryOperator op = context.AND() != null
                    ? BinaryOperator.LogicalAnd
                    : BinaryOperator.LogicalOr;
                var opLiteral = new BinaryOperatorLiteral(op, context.GetTextSpan());
                result = new BinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitMultiset_expression([NotNull] PlSqlParser.Multiset_expressionContext context) { return VisitChildren(context); }

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
            BinaryOperator op;
            string opText = context.GetText();
            if (opText == "=")
            {
                op = BinaryOperator.Equal;
            }
            else if (opText == ">")
            {
                op = BinaryOperator.Greater;
            }
            else if (opText == "<")
            {
                op = BinaryOperator.Less;
            }
            else if (opText == ">=")
            {
                op = BinaryOperator.GreaterOrEqual;
            }
            else if (opText == "<=")
            {
                op = BinaryOperator.LessOrEqual;
            }
            else
            {
                op = BinaryOperator.NotEqual;
            }

            var result = new BinaryOperatorLiteral(op, context.GetTextSpan());
            return result;
        }

        public Ust VisitIn_elements([NotNull] PlSqlParser.In_elementsContext context) { return VisitChildren(context); }

        public Ust VisitBetween_elements([NotNull] PlSqlParser.Between_elementsContext context) { return VisitChildren(context); }

        /// <returns><see cref="Expression"/></returns>
        public Ust VisitConcatenation([NotNull] PlSqlParser.ConcatenationContext context)
        {
            Expression result;
            if (context.model_expression() != null)
            {
                result = (Expression)VisitChildren(context);
            }
            else
            {
                BinaryOperator op = BinaryOperator.Plus;
                if (context.BAR() == null)
                {
                    BinaryOperatorLiteral.TextBinaryOperator.TryGetValue(context.op.Text, out op);
                }
                var literal = new BinaryOperatorLiteral(op, context.op?.GetTextSpan() ?? context.BAR(0).GetTextSpan());
                result = new BinaryOperatorExpression(
                    (Expression)Visit(context.concatenation(0)),
                    literal,
                    (Expression)Visit(context.concatenation(1)),
                    context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitInterval_expression([NotNull] PlSqlParser.Interval_expressionContext context) { return VisitChildren(context); }

        public Ust VisitModel_expression([NotNull] PlSqlParser.Model_expressionContext context) { return VisitChildren(context); }

        public Ust VisitModel_expression_element([NotNull] PlSqlParser.Model_expression_elementContext context) { return VisitChildren(context); }

        public Ust VisitSingle_column_for_loop([NotNull] PlSqlParser.Single_column_for_loopContext context) { return VisitChildren(context); }

        public Ust VisitMulti_column_for_loop([NotNull] PlSqlParser.Multi_column_for_loopContext context) { return VisitChildren(context); }

        public Ust VisitUnary_expression([NotNull] PlSqlParser.Unary_expressionContext context) { return VisitChildren(context); }

        public Ust VisitCase_statement([NotNull] PlSqlParser.Case_statementContext context) { return VisitChildren(context); }

        public Ust VisitSimple_case_statement([NotNull] PlSqlParser.Simple_case_statementContext context) { return VisitChildren(context); }

        public Ust VisitSimple_case_when_part([NotNull] PlSqlParser.Simple_case_when_partContext context) { return VisitChildren(context); }

        public Ust VisitSearched_case_statement([NotNull] PlSqlParser.Searched_case_statementContext context) { return VisitChildren(context); }

        public Ust VisitSearched_case_when_part([NotNull] PlSqlParser.Searched_case_when_partContext context) { return VisitChildren(context); }

        public Ust VisitCase_else_part([NotNull] PlSqlParser.Case_else_partContext context) { return VisitChildren(context); }

        public Ust VisitAtom([NotNull] PlSqlParser.AtomContext context) { return VisitChildren(context); }

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

        public Ust VisitPartition_extension_clause([NotNull] PlSqlParser.Partition_extension_clauseContext context) { return VisitChildren(context); }

        public Ust VisitColumn_alias([NotNull] PlSqlParser.Column_aliasContext context) { return VisitChildren(context); }

        public Ust VisitTable_alias([NotNull] PlSqlParser.Table_aliasContext context) { return VisitChildren(context); }

        public Ust VisitWhere_clause([NotNull] PlSqlParser.Where_clauseContext context)
        {
            return VisitChildren(context);
        }

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

        public Ust VisitIndex_name([NotNull] PlSqlParser.Index_nameContext context) { return VisitChildren(context); }

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
            if (context.NULL_() != null)
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
            var textSpan = context.GetTextSpan();
            if (context.UNSIGNED_INTEGER() != null)
            {
                if (int.TryParse(text, out int intValue))
                {
                    result = new IntLiteral(intValue, textSpan);
                }
                else if (long.TryParse(text, out long longValue))
                {
                    result = new LongLiteral(longValue, textSpan);
                }
                else
                {
                    result = new BigIntLiteral(BigInteger.Parse(text), textSpan);
                }
            }
            else
            {
                text = text.ToLowerInvariant().Replace("f", "").Replace("d", "");
                result = new FloatLiteral(double.Parse(text, CultureInfo.InvariantCulture), context.GetTextSpan());
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

        public Ust VisitOuter_join_sign([NotNull] PlSqlParser.Outer_join_signContext context) { return VisitChildren(context); }

        public Ust VisitRegular_id([NotNull] PlSqlParser.Regular_idContext context)
        {
            return VisitChildren(context);
        }

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

        public Ust VisitCreate_index([NotNull] PlSqlParser.Create_indexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_index([NotNull] PlSqlParser.Alter_indexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDrop_index([NotNull] PlSqlParser.Drop_indexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSize_clause([NotNull] PlSqlParser.Size_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_table([NotNull] PlSqlParser.Alter_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAdd_constraint([NotNull] PlSqlParser.Add_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCheck_constraint([NotNull] PlSqlParser.Check_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDrop_constraint([NotNull] PlSqlParser.Drop_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEnable_constraint([NotNull] PlSqlParser.Enable_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDisable_constraint([NotNull] PlSqlParser.Disable_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitForeign_key_clause([NotNull] PlSqlParser.Foreign_key_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReferences_clause([NotNull] PlSqlParser.References_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOn_delete_clause([NotNull] PlSqlParser.On_delete_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUnique_key_clause([NotNull] PlSqlParser.Unique_key_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPrimary_key_clause([NotNull] PlSqlParser.Primary_key_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPipe_row_statement([NotNull] PlSqlParser.Pipe_row_statementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpressions([NotNull] PlSqlParser.ExpressionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCluster_index_clause([NotNull] PlSqlParser.Cluster_index_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCluster_name([NotNull] PlSqlParser.Cluster_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTable_index_clause([NotNull] PlSqlParser.Table_index_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBitmap_join_index_clause([NotNull] PlSqlParser.Bitmap_join_index_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndex_expr([NotNull] PlSqlParser.Index_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndex_properties([NotNull] PlSqlParser.Index_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDomain_index_clause([NotNull] PlSqlParser.Domain_index_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLocal_domain_index_clause([NotNull] PlSqlParser.Local_domain_index_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXmlindex_clause([NotNull] PlSqlParser.Xmlindex_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLocal_xmlindex_clause([NotNull] PlSqlParser.Local_xmlindex_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGlobal_partitioned_index([NotNull] PlSqlParser.Global_partitioned_indexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndex_partitioning_clause([NotNull] PlSqlParser.Index_partitioning_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLocal_partitioned_index([NotNull] PlSqlParser.Local_partitioned_indexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOn_range_partitioned_table([NotNull] PlSqlParser.On_range_partitioned_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOn_list_partitioned_table([NotNull] PlSqlParser.On_list_partitioned_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOn_hash_partitioned_table([NotNull] PlSqlParser.On_hash_partitioned_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOn_comp_partitioned_table([NotNull] PlSqlParser.On_comp_partitioned_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndex_subpartition_clause([NotNull] PlSqlParser.Index_subpartition_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOdci_parameters([NotNull] PlSqlParser.Odci_parametersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndextype([NotNull] PlSqlParser.IndextypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_index_ops_set1([NotNull] PlSqlParser.Alter_index_ops_set1Context context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_index_ops_set2([NotNull] PlSqlParser.Alter_index_ops_set2Context context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVisible_or_invisible([NotNull] PlSqlParser.Visible_or_invisibleContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMonitoring_nomonitoring([NotNull] PlSqlParser.Monitoring_nomonitoringContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRebuild_clause([NotNull] PlSqlParser.Rebuild_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_index_partitioning([NotNull] PlSqlParser.Alter_index_partitioningContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_index_default_attrs([NotNull] PlSqlParser.Modify_index_default_attrsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAdd_hash_index_partition([NotNull] PlSqlParser.Add_hash_index_partitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCoalesce_index_partition([NotNull] PlSqlParser.Coalesce_index_partitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_index_partition([NotNull] PlSqlParser.Modify_index_partitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_index_partitions_ops([NotNull] PlSqlParser.Modify_index_partitions_opsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRename_index_partition([NotNull] PlSqlParser.Rename_index_partitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDrop_index_partition([NotNull] PlSqlParser.Drop_index_partitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSplit_index_partition([NotNull] PlSqlParser.Split_index_partitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndex_partition_description([NotNull] PlSqlParser.Index_partition_descriptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_index_subpartition([NotNull] PlSqlParser.Modify_index_subpartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartition_name_old([NotNull] PlSqlParser.Partition_name_oldContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNew_partition_name([NotNull] PlSqlParser.New_partition_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNew_index_name([NotNull] PlSqlParser.New_index_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_user([NotNull] PlSqlParser.Create_userContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_user([NotNull] PlSqlParser.Alter_userContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_identified_by([NotNull] PlSqlParser.Alter_identified_byContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIdentified_by([NotNull] PlSqlParser.Identified_byContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIdentified_other_clause([NotNull] PlSqlParser.Identified_other_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUser_tablespace_clause([NotNull] PlSqlParser.User_tablespace_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitQuota_clause([NotNull] PlSqlParser.Quota_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitProfile_clause([NotNull] PlSqlParser.Profile_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRole_clause([NotNull] PlSqlParser.Role_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUser_default_role_clause([NotNull] PlSqlParser.User_default_role_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPassword_expire_clause([NotNull] PlSqlParser.Password_expire_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUser_lock_clause([NotNull] PlSqlParser.User_lock_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUser_editions_clause([NotNull] PlSqlParser.User_editions_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_user_editions_clause([NotNull] PlSqlParser.Alter_user_editions_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitProxy_clause([NotNull] PlSqlParser.Proxy_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitContainer_names([NotNull] PlSqlParser.Container_namesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSet_container_data([NotNull] PlSqlParser.Set_container_dataContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAdd_rem_container_data([NotNull] PlSqlParser.Add_rem_container_dataContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitContainer_data_clause([NotNull] PlSqlParser.Container_data_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGrant_statement([NotNull] PlSqlParser.Grant_statementContext context)
        {
            string funcName = context.GRANT(0).GetText();
            TextSpan textSpan = context.GRANT(0).GetTextSpan();
            var funcId = new IdToken(funcName, textSpan);
            var args = new List<Expression>();
            foreach(var children in context.children.Skip(1))
            {
                args.Add(Visit(children).AsExpression());
            }
            var result = new ExpressionStatement(new InvocationExpression(funcId, new ArgsUst(args), context.GetTextSpan()));
            return result;
        }

        public Ust VisitContainer_clause([NotNull] PlSqlParser.Container_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_directory([NotNull] PlSqlParser.Create_directoryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDirectory_name([NotNull] PlSqlParser.Directory_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDirectory_path([NotNull] PlSqlParser.Directory_pathContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_view([NotNull] PlSqlParser.Create_viewContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitView_options([NotNull] PlSqlParser.View_optionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitView_alias_constraint([NotNull] PlSqlParser.View_alias_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitObject_view_clause([NotNull] PlSqlParser.Object_view_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInline_constraint([NotNull] PlSqlParser.Inline_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInline_ref_constraint([NotNull] PlSqlParser.Inline_ref_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOut_of_line_ref_constraint([NotNull] PlSqlParser.Out_of_line_ref_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOut_of_line_constraint([NotNull] PlSqlParser.Out_of_line_constraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitConstraint_state([NotNull] PlSqlParser.Constraint_stateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_tablespace([NotNull] PlSqlParser.Create_tablespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPermanent_tablespace_clause([NotNull] PlSqlParser.Permanent_tablespace_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTablespace_encryption_spec([NotNull] PlSqlParser.Tablespace_encryption_specContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLogging_clause([NotNull] PlSqlParser.Logging_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExtent_management_clause([NotNull] PlSqlParser.Extent_management_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSegment_management_clause([NotNull] PlSqlParser.Segment_management_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFlashback_mode_clause([NotNull] PlSqlParser.Flashback_mode_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTemporary_tablespace_clause([NotNull] PlSqlParser.Temporary_tablespace_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTablespace_group_clause([NotNull] PlSqlParser.Tablespace_group_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUndo_tablespace_clause([NotNull] PlSqlParser.Undo_tablespace_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTablespace_retention_clause([NotNull] PlSqlParser.Tablespace_retention_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDatafile_specification([NotNull] PlSqlParser.Datafile_specificationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTempfile_specification([NotNull] PlSqlParser.Tempfile_specificationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDatafile_tempfile_spec([NotNull] PlSqlParser.Datafile_tempfile_specContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRedo_log_file_spec([NotNull] PlSqlParser.Redo_log_file_specContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAutoextend_clause([NotNull] PlSqlParser.Autoextend_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMaxsize_clause([NotNull] PlSqlParser.Maxsize_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBuild_clause([NotNull] PlSqlParser.Build_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitParallel_clause([NotNull] PlSqlParser.Parallel_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_materialized_view_log([NotNull] PlSqlParser.Create_materialized_view_logContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNew_values_clause([NotNull] PlSqlParser.New_values_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMv_log_purge_clause([NotNull] PlSqlParser.Mv_log_purge_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_materialized_view([NotNull] PlSqlParser.Create_materialized_viewContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_mv_refresh([NotNull] PlSqlParser.Create_mv_refreshContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_context([NotNull] PlSqlParser.Create_contextContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_cluster([NotNull] PlSqlParser.Create_clusterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXmltype_table([NotNull] PlSqlParser.Xmltype_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXmltype_virtual_columns([NotNull] PlSqlParser.Xmltype_virtual_columnsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXmltype_column_properties([NotNull] PlSqlParser.Xmltype_column_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXmltype_storage([NotNull] PlSqlParser.Xmltype_storageContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXmlschema_spec([NotNull] PlSqlParser.Xmlschema_specContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitObject_table([NotNull] PlSqlParser.Object_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOid_index_clause([NotNull] PlSqlParser.Oid_index_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOid_clause([NotNull] PlSqlParser.Oid_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitObject_properties([NotNull] PlSqlParser.Object_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitObject_table_substitution([NotNull] PlSqlParser.Object_table_substitutionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRelational_table([NotNull] PlSqlParser.Relational_tableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRelational_properties([NotNull] PlSqlParser.Relational_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTable_partitioning_clauses([NotNull] PlSqlParser.Table_partitioning_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRange_partitions([NotNull] PlSqlParser.Range_partitionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitList_partitions([NotNull] PlSqlParser.List_partitionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHash_partitions([NotNull] PlSqlParser.Hash_partitionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndividual_hash_partitions([NotNull] PlSqlParser.Individual_hash_partitionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHash_partitions_by_quantity([NotNull] PlSqlParser.Hash_partitions_by_quantityContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHash_partition_quantity([NotNull] PlSqlParser.Hash_partition_quantityContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComposite_range_partitions([NotNull] PlSqlParser.Composite_range_partitionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComposite_list_partitions([NotNull] PlSqlParser.Composite_list_partitionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComposite_hash_partitions([NotNull] PlSqlParser.Composite_hash_partitionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReference_partitioning([NotNull] PlSqlParser.Reference_partitioningContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReference_partition_desc([NotNull] PlSqlParser.Reference_partition_descContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSystem_partitioning([NotNull] PlSqlParser.System_partitioningContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRange_partition_desc([NotNull] PlSqlParser.Range_partition_descContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitList_partition_desc([NotNull] PlSqlParser.List_partition_descContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubpartition_template([NotNull] PlSqlParser.Subpartition_templateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHash_subpartition_quantity([NotNull] PlSqlParser.Hash_subpartition_quantityContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubpartition_by_range([NotNull] PlSqlParser.Subpartition_by_rangeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubpartition_by_list([NotNull] PlSqlParser.Subpartition_by_listContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubpartition_by_hash([NotNull] PlSqlParser.Subpartition_by_hashContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubpartition_name([NotNull] PlSqlParser.Subpartition_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRange_subpartition_desc([NotNull] PlSqlParser.Range_subpartition_descContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitList_subpartition_desc([NotNull] PlSqlParser.List_subpartition_descContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndividual_hash_subparts([NotNull] PlSqlParser.Individual_hash_subpartsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHash_subparts_by_quantity([NotNull] PlSqlParser.Hash_subparts_by_quantityContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRange_values_clause([NotNull] PlSqlParser.Range_values_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitList_values_clause([NotNull] PlSqlParser.List_values_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTable_partition_description([NotNull] PlSqlParser.Table_partition_descriptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitioning_storage_clause([NotNull] PlSqlParser.Partitioning_storage_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLob_partitioning_storage([NotNull] PlSqlParser.Lob_partitioning_storageContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDatatype_null_enable([NotNull] PlSqlParser.Datatype_null_enableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTable_compression([NotNull] PlSqlParser.Table_compressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPhysical_attributes_clause([NotNull] PlSqlParser.Physical_attributes_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStorage_clause([NotNull] PlSqlParser.Storage_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDeferred_segment_creation([NotNull] PlSqlParser.Deferred_segment_creationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSegment_attributes_clause([NotNull] PlSqlParser.Segment_attributes_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPhysical_properties([NotNull] PlSqlParser.Physical_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRow_movement_clause([NotNull] PlSqlParser.Row_movement_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFlashback_archive_clause([NotNull] PlSqlParser.Flashback_archive_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLog_grp([NotNull] PlSqlParser.Log_grpContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSupplemental_table_logging([NotNull] PlSqlParser.Supplemental_table_loggingContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSupplemental_log_grp_clause([NotNull] PlSqlParser.Supplemental_log_grp_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSupplemental_id_key_clause([NotNull] PlSqlParser.Supplemental_id_key_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAllocate_extent_clause([NotNull] PlSqlParser.Allocate_extent_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDeallocate_unused_clause([NotNull] PlSqlParser.Deallocate_unused_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShrink_clause([NotNull] PlSqlParser.Shrink_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRecords_per_block_clause([NotNull] PlSqlParser.Records_per_block_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUpgrade_table_clause([NotNull] PlSqlParser.Upgrade_table_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEnable_or_disable([NotNull] PlSqlParser.Enable_or_disableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAllow_or_disallow([NotNull] PlSqlParser.Allow_or_disallowContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_cluster([NotNull] PlSqlParser.Alter_clusterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCache_or_nocache([NotNull] PlSqlParser.Cache_or_nocacheContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDatabase_name([NotNull] PlSqlParser.Database_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_database([NotNull] PlSqlParser.Alter_databaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStartup_clauses([NotNull] PlSqlParser.Startup_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitResetlogs_or_noresetlogs([NotNull] PlSqlParser.Resetlogs_or_noresetlogsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUpgrade_or_downgrade([NotNull] PlSqlParser.Upgrade_or_downgradeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRecovery_clauses([NotNull] PlSqlParser.Recovery_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBegin_or_end([NotNull] PlSqlParser.Begin_or_endContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGeneral_recovery([NotNull] PlSqlParser.General_recoveryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFull_database_recovery([NotNull] PlSqlParser.Full_database_recoveryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartial_database_recovery([NotNull] PlSqlParser.Partial_database_recoveryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitManaged_standby_recovery([NotNull] PlSqlParser.Managed_standby_recoveryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDb_name([NotNull] PlSqlParser.Db_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDatabase_file_clauses([NotNull] PlSqlParser.Database_file_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreate_datafile_clause([NotNull] PlSqlParser.Create_datafile_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_datafile_clause([NotNull] PlSqlParser.Alter_datafile_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_tempfile_clause([NotNull] PlSqlParser.Alter_tempfile_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLogfile_clauses([NotNull] PlSqlParser.Logfile_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAdd_logfile_clauses([NotNull] PlSqlParser.Add_logfile_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDrop_logfile_clauses([NotNull] PlSqlParser.Drop_logfile_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSwitch_logfile_clause([NotNull] PlSqlParser.Switch_logfile_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSupplemental_db_logging([NotNull] PlSqlParser.Supplemental_db_loggingContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAdd_or_drop([NotNull] PlSqlParser.Add_or_dropContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSupplemental_plsql_clause([NotNull] PlSqlParser.Supplemental_plsql_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLogfile_descriptor([NotNull] PlSqlParser.Logfile_descriptorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitControlfile_clauses([NotNull] PlSqlParser.Controlfile_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTrace_file_clause([NotNull] PlSqlParser.Trace_file_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStandby_database_clauses([NotNull] PlSqlParser.Standby_database_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitActivate_standby_db_clause([NotNull] PlSqlParser.Activate_standby_db_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMaximize_standby_db_clause([NotNull] PlSqlParser.Maximize_standby_db_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRegister_logfile_clause([NotNull] PlSqlParser.Register_logfile_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCommit_switchover_clause([NotNull] PlSqlParser.Commit_switchover_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStart_standby_clause([NotNull] PlSqlParser.Start_standby_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStop_standby_clause([NotNull] PlSqlParser.Stop_standby_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitConvert_database_clause([NotNull] PlSqlParser.Convert_database_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefault_settings_clause([NotNull] PlSqlParser.Default_settings_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTablespace_group_name([NotNull] PlSqlParser.Tablespace_group_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSet_time_zone_clause([NotNull] PlSqlParser.Set_time_zone_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInstance_clauses([NotNull] PlSqlParser.Instance_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSecurity_clause([NotNull] PlSqlParser.Security_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDomain([NotNull] PlSqlParser.DomainContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDatabase([NotNull] PlSqlParser.DatabaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEdition_name([NotNull] PlSqlParser.Edition_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFilenumber([NotNull] PlSqlParser.FilenumberContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFilename([NotNull] PlSqlParser.FilenameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_table_properties([NotNull] PlSqlParser.Alter_table_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_table_properties_1([NotNull] PlSqlParser.Alter_table_properties_1Context context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_iot_clauses([NotNull] PlSqlParser.Alter_iot_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_mapping_table_clause([NotNull] PlSqlParser.Alter_mapping_table_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_overflow_clause([NotNull] PlSqlParser.Alter_overflow_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAdd_overflow_clause([NotNull] PlSqlParser.Add_overflow_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEnable_disable_clause([NotNull] PlSqlParser.Enable_disable_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUsing_index_clause([NotNull] PlSqlParser.Using_index_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndex_attributes([NotNull] PlSqlParser.Index_attributesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSort_or_nosort([NotNull] PlSqlParser.Sort_or_nosortContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExceptions_clause([NotNull] PlSqlParser.Exceptions_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMove_table_clause([NotNull] PlSqlParser.Move_table_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndex_org_table_clause([NotNull] PlSqlParser.Index_org_table_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMapping_table_clause([NotNull] PlSqlParser.Mapping_table_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitKey_compression([NotNull] PlSqlParser.Key_compressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndex_org_overflow_clause([NotNull] PlSqlParser.Index_org_overflow_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumn_clauses([NotNull] PlSqlParser.Column_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_collection_retrieval([NotNull] PlSqlParser.Modify_collection_retrievalContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCollection_item([NotNull] PlSqlParser.Collection_itemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRename_column_clause([NotNull] PlSqlParser.Rename_column_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOld_column_name([NotNull] PlSqlParser.Old_column_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNew_column_name([NotNull] PlSqlParser.New_column_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAdd_modify_drop_column_clauses([NotNull] PlSqlParser.Add_modify_drop_column_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDrop_column_clause([NotNull] PlSqlParser.Drop_column_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_column_clauses([NotNull] PlSqlParser.Modify_column_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_col_properties([NotNull] PlSqlParser.Modify_col_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_col_substitutable([NotNull] PlSqlParser.Modify_col_substitutableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAdd_column_clause([NotNull] PlSqlParser.Add_column_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_varray_col_properties([NotNull] PlSqlParser.Alter_varray_col_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVarray_col_properties([NotNull] PlSqlParser.Varray_col_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVarray_storage_clause([NotNull] PlSqlParser.Varray_storage_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLob_segname([NotNull] PlSqlParser.Lob_segnameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLob_item([NotNull] PlSqlParser.Lob_itemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLob_storage_parameters([NotNull] PlSqlParser.Lob_storage_parametersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLob_storage_clause([NotNull] PlSqlParser.Lob_storage_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_lob_storage_clause([NotNull] PlSqlParser.Modify_lob_storage_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_lob_parameters([NotNull] PlSqlParser.Modify_lob_parametersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLob_parameters([NotNull] PlSqlParser.Lob_parametersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLob_deduplicate_clause([NotNull] PlSqlParser.Lob_deduplicate_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLob_compression_clause([NotNull] PlSqlParser.Lob_compression_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLob_retention_clause([NotNull] PlSqlParser.Lob_retention_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEncryption_spec([NotNull] PlSqlParser.Encryption_specContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTablespace([NotNull] PlSqlParser.TablespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVarray_item([NotNull] PlSqlParser.Varray_itemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumn_properties([NotNull] PlSqlParser.Column_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumn_definition([NotNull] PlSqlParser.Column_definitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVirtual_column_definition([NotNull] PlSqlParser.Virtual_column_definitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOut_of_line_part_storage([NotNull] PlSqlParser.Out_of_line_part_storageContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNested_table_col_properties([NotNull] PlSqlParser.Nested_table_col_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNested_item([NotNull] PlSqlParser.Nested_itemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubstitutable_column_clause([NotNull] PlSqlParser.Substitutable_column_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartition_name([NotNull] PlSqlParser.Partition_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSupplemental_logging_props([NotNull] PlSqlParser.Supplemental_logging_propsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumn_or_attribute([NotNull] PlSqlParser.Column_or_attributeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitObject_type_col_properties([NotNull] PlSqlParser.Object_type_col_propertiesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitConstraint_clauses([NotNull] PlSqlParser.Constraint_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOld_constraint_name([NotNull] PlSqlParser.Old_constraint_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNew_constraint_name([NotNull] PlSqlParser.New_constraint_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDrop_constraint_clause([NotNull] PlSqlParser.Drop_constraint_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDrop_primary_key_or_unique_or_generic_clause([NotNull] PlSqlParser.Drop_primary_key_or_unique_or_generic_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSeq_of_declare_specs([NotNull] PlSqlParser.Seq_of_declare_specsContext context)
        {
            List<Statement> decls = new List<Statement>();
            for (int i = 0; i < context.ChildCount; i++)
            {
                decls.Add(new ExpressionStatement((Expression)Visit(context.GetChild(i))));
            }
            return new BlockStatement(decls);
        }

        public Ust VisitOffset_clause([NotNull] PlSqlParser.Offset_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFetch_clause([NotNull] PlSqlParser.Fetch_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLiteral([NotNull] PlSqlParser.LiteralContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitContainer_tableview_name([NotNull] PlSqlParser.Container_tableview_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGrantee_name([NotNull] PlSqlParser.Grantee_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRole_name([NotNull] PlSqlParser.Role_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDir_object_name([NotNull] PlSqlParser.Dir_object_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUser_object_name([NotNull] PlSqlParser.User_object_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGrant_object_name([NotNull] PlSqlParser.Grant_object_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumn_list([NotNull] PlSqlParser.Column_listContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitParen_column_list([NotNull] PlSqlParser.Paren_column_listContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitObject_privilege([NotNull] PlSqlParser.Object_privilegeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSystem_privilege([NotNull] PlSqlParser.System_privilegeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnalyze([NotNull] PlSqlParser.AnalyzeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartition_extention_clause([NotNull] PlSqlParser.Partition_extention_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitValidation_clauses([NotNull] PlSqlParser.Validation_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOnline_or_offline([NotNull] PlSqlParser.Online_or_offlineContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInto_clause1([NotNull] PlSqlParser.Into_clause1Context context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartition_key_value([NotNull] PlSqlParser.Partition_key_valueContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubpartition_key_value([NotNull] PlSqlParser.Subpartition_key_valueContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAssociate_statistics([NotNull] PlSqlParser.Associate_statisticsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumn_association([NotNull] PlSqlParser.Column_associationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFunction_association([NotNull] PlSqlParser.Function_associationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndextype_name([NotNull] PlSqlParser.Indextype_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUsing_statistics_type([NotNull] PlSqlParser.Using_statistics_typeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStatistics_type_name([NotNull] PlSqlParser.Statistics_type_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefault_cost_clause([NotNull] PlSqlParser.Default_cost_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCpu_cost([NotNull] PlSqlParser.Cpu_costContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIo_cost([NotNull] PlSqlParser.Io_costContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNetwork_cost([NotNull] PlSqlParser.Network_costContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefault_selectivity_clause([NotNull] PlSqlParser.Default_selectivity_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefault_selectivity([NotNull] PlSqlParser.Default_selectivityContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStorage_table_clause([NotNull] PlSqlParser.Storage_table_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUnified_auditing([NotNull] PlSqlParser.Unified_auditingContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPolicy_name([NotNull] PlSqlParser.Policy_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAudit_traditional([NotNull] PlSqlParser.Audit_traditionalContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAudit_direct_path([NotNull] PlSqlParser.Audit_direct_pathContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAudit_container_clause([NotNull] PlSqlParser.Audit_container_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAudit_operation_clause([NotNull] PlSqlParser.Audit_operation_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAuditing_by_clause([NotNull] PlSqlParser.Auditing_by_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAudit_user([NotNull] PlSqlParser.Audit_userContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAudit_schema_object_clause([NotNull] PlSqlParser.Audit_schema_object_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSql_operation([NotNull] PlSqlParser.Sql_operationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAuditing_on_clause([NotNull] PlSqlParser.Auditing_on_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModel_name([NotNull] PlSqlParser.Model_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitObject_name([NotNull] PlSqlParser.Object_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitProfile_name([NotNull] PlSqlParser.Profile_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSql_statement_shortcut([NotNull] PlSqlParser.Sql_statement_shortcutContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_library([NotNull] PlSqlParser.Alter_libraryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLibrary_editionable([NotNull] PlSqlParser.Library_editionableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLibrary_debug([NotNull] PlSqlParser.Library_debugContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitParameter_value([NotNull] PlSqlParser.Parameter_valueContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLibrary_name([NotNull] PlSqlParser.Library_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_view([NotNull] PlSqlParser.Alter_viewContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_view_editionable([NotNull] PlSqlParser.Alter_view_editionableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_tablespace([NotNull] PlSqlParser.Alter_tablespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDatafile_tempfile_clauses([NotNull] PlSqlParser.Datafile_tempfile_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTablespace_logging_clauses([NotNull] PlSqlParser.Tablespace_logging_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTablespace_state_clauses([NotNull] PlSqlParser.Tablespace_state_clausesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNew_tablespace_name([NotNull] PlSqlParser.New_tablespace_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_materialized_view([NotNull] PlSqlParser.Alter_materialized_viewContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_mv_option1([NotNull] PlSqlParser.Alter_mv_option1Context context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_mv_refresh([NotNull] PlSqlParser.Alter_mv_refreshContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRollback_segment([NotNull] PlSqlParser.Rollback_segmentContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitModify_mv_column_clause([NotNull] PlSqlParser.Modify_mv_column_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlter_materialized_view_log([NotNull] PlSqlParser.Alter_materialized_view_logContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAdd_mv_log_column_clause([NotNull] PlSqlParser.Add_mv_log_column_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMove_mv_log_clause([NotNull] PlSqlParser.Move_mv_log_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMv_log_augmentation([NotNull] PlSqlParser.Mv_log_augmentationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDatetime_expr([NotNull] PlSqlParser.Datetime_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInterval_expr([NotNull] PlSqlParser.Interval_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSynchronous_or_asynchronous([NotNull] PlSqlParser.Synchronous_or_asynchronousContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIncluding_or_excluding([NotNull] PlSqlParser.Including_or_excludingContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOracle_namespace([NotNull] PlSqlParser.Oracle_namespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPeriod_definition([NotNull] PlSqlParser.Period_definitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStart_time_column([NotNull] PlSqlParser.Start_time_columnContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEnd_time_column([NotNull] PlSqlParser.End_time_columnContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNon_reserved_keywords_in_12c([NotNull] PlSqlParser.Non_reserved_keywords_in_12cContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNon_reserved_keywords_pre12c([NotNull] PlSqlParser.Non_reserved_keywords_pre12cContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartial_database_recovery_10g([NotNull] PlSqlParser.Partial_database_recovery_10gContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLog_file_group([NotNull] PlSqlParser.Log_file_groupContext context)
        {
            return VisitChildren(context);
        }
    }
}
