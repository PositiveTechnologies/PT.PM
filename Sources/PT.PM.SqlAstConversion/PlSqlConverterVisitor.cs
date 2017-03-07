using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.SqlAstConversion.Parser;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.SqlAstConversion
{
    public partial class PlSqlConverterVisitor : AntlrDefaultVisitor, IplsqlVisitor<UstNode>
    {
        public PlSqlConverterVisitor(string fileName, string fileData)
            : base(fileName, fileData)
        {
            FileNode = new FileNode(fileName, fileData);
        }

        /// <returns><see cref="ArgsNode"/></returns>
        public UstNode VisitSwallow_to_semi([NotNull] plsqlParser.Swallow_to_semiContext context)
        {
            Expression[] args = context.children.Select(c => (Expression)Visit(c)).ToArray();
            var result = new ArgsNode(args, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitCompilation_unit([NotNull] plsqlParser.Compilation_unitContext context)
        {
            var roots = context.unit_statement().Select(statement => (Statement)Visit(statement)).ToArray();
            FileNode.Root = roots.CreateRootNamespace(Language.PlSql, FileNode);
            return FileNode;
        }

        public UstNode VisitSql_script([NotNull] plsqlParser.Sql_scriptContext context) { return VisitChildren(context); }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitUnit_statement([NotNull] plsqlParser.Unit_statementContext context)
        {
            var child = Visit(context.GetChild(0));
            Statement result = child.ToStatementIfRequired();
            return result;
        }

        public UstNode VisitDrop_function([NotNull] plsqlParser.Drop_functionContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_function([NotNull] plsqlParser.Alter_functionContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_function_body([NotNull] plsqlParser.Create_function_bodyContext context)
        {
            var name = (IdToken)Visit(context.function_name());
            ParameterDeclaration[] parameters = context.parameter().Select(p => (ParameterDeclaration)Visit(p)).ToArray();
            var body = (BlockStatement)Visit(context.body());

            var result = new MethodDeclaration(name, parameters, body, context.GetTextSpan(), FileNode);
            result.ReturnType = (TypeToken)Visit(context.type_spec());
            return result;
        }

        public UstNode VisitParallel_enable_clause([NotNull] plsqlParser.Parallel_enable_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPartition_by_clause([NotNull] plsqlParser.Partition_by_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitResult_cache_clause([NotNull] plsqlParser.Result_cache_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitRelies_on_part([NotNull] plsqlParser.Relies_on_partContext context) { return VisitChildren(context); }

        public UstNode VisitStreaming_clause([NotNull] plsqlParser.Streaming_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitDrop_package([NotNull] plsqlParser.Drop_packageContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_package([NotNull] plsqlParser.Alter_packageContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_package([NotNull] plsqlParser.Create_packageContext context) { return VisitChildren(context); }

        public UstNode VisitPackage_body([NotNull] plsqlParser.Package_bodyContext context) { return VisitChildren(context); }

        public UstNode VisitPackage_spec([NotNull] plsqlParser.Package_specContext context) { return VisitChildren(context); }

        public UstNode VisitPackage_obj_spec([NotNull] plsqlParser.Package_obj_specContext context) { return VisitChildren(context); }

        public UstNode VisitProcedure_spec([NotNull] plsqlParser.Procedure_specContext context) { return VisitChildren(context); }

        public UstNode VisitFunction_spec([NotNull] plsqlParser.Function_specContext context) { return VisitChildren(context); }

        public UstNode VisitPackage_obj_body([NotNull] plsqlParser.Package_obj_bodyContext context) { return VisitChildren(context); }

        public UstNode VisitDrop_procedure([NotNull] plsqlParser.Drop_procedureContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_procedure([NotNull] plsqlParser.Alter_procedureContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_procedure_body([NotNull] plsqlParser.Create_procedure_bodyContext context)
        {
            var name = (IdToken)Visit(context.procedure_name());
            ParameterDeclaration[] parameters = context.parameter().Select(p => (ParameterDeclaration)Visit(p)).ToArray();
            var body = (BlockStatement)Visit(context.body());

            var result = new MethodDeclaration(name, parameters, body, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitDrop_trigger([NotNull] plsqlParser.Drop_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_trigger([NotNull] plsqlParser.Alter_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_trigger([NotNull] plsqlParser.Create_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitTrigger_follows_clause([NotNull] plsqlParser.Trigger_follows_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitTrigger_when_clause([NotNull] plsqlParser.Trigger_when_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSimple_dml_trigger([NotNull] plsqlParser.Simple_dml_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitFor_each_row([NotNull] plsqlParser.For_each_rowContext context) { return VisitChildren(context); }

        public UstNode VisitCompound_dml_trigger([NotNull] plsqlParser.Compound_dml_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitNon_dml_trigger([NotNull] plsqlParser.Non_dml_triggerContext context) { return VisitChildren(context); }

        public UstNode VisitTrigger_body([NotNull] plsqlParser.Trigger_bodyContext context) { return VisitChildren(context); }

        public UstNode VisitRoutine_clause([NotNull] plsqlParser.Routine_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCompound_trigger_block([NotNull] plsqlParser.Compound_trigger_blockContext context) { return VisitChildren(context); }

        public UstNode VisitTiming_point_section([NotNull] plsqlParser.Timing_point_sectionContext context) { return VisitChildren(context); }

        public UstNode VisitNon_dml_event([NotNull] plsqlParser.Non_dml_eventContext context) { return VisitChildren(context); }

        public UstNode VisitDml_event_clause([NotNull] plsqlParser.Dml_event_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitDml_event_element([NotNull] plsqlParser.Dml_event_elementContext context) { return VisitChildren(context); }

        public UstNode VisitDml_event_nested_clause([NotNull] plsqlParser.Dml_event_nested_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitReferencing_clause([NotNull] plsqlParser.Referencing_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitReferencing_element([NotNull] plsqlParser.Referencing_elementContext context) { return VisitChildren(context); }

        public UstNode VisitDrop_type([NotNull] plsqlParser.Drop_typeContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_type([NotNull] plsqlParser.Alter_typeContext context) { return VisitChildren(context); }

        public UstNode VisitCompile_type_clause([NotNull] plsqlParser.Compile_type_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitReplace_type_clause([NotNull] plsqlParser.Replace_type_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_method_spec([NotNull] plsqlParser.Alter_method_specContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_method_element([NotNull] plsqlParser.Alter_method_elementContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_attribute_definition([NotNull] plsqlParser.Alter_attribute_definitionContext context) { return VisitChildren(context); }

        public UstNode VisitAttribute_definition([NotNull] plsqlParser.Attribute_definitionContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_collection_clauses([NotNull] plsqlParser.Alter_collection_clausesContext context) { return VisitChildren(context); }

        public UstNode VisitDependent_handling_clause([NotNull] plsqlParser.Dependent_handling_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitDependent_exceptions_part([NotNull] plsqlParser.Dependent_exceptions_partContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_type([NotNull] plsqlParser.Create_typeContext context) { return VisitChildren(context); }

        public UstNode VisitType_definition([NotNull] plsqlParser.Type_definitionContext context) { return VisitChildren(context); }

        public UstNode VisitObject_type_def([NotNull] plsqlParser.Object_type_defContext context) { return VisitChildren(context); }

        public UstNode VisitObject_as_part([NotNull] plsqlParser.Object_as_partContext context) { return VisitChildren(context); }

        public UstNode VisitObject_under_part([NotNull] plsqlParser.Object_under_partContext context) { return VisitChildren(context); }

        public UstNode VisitNested_table_type_def([NotNull] plsqlParser.Nested_table_type_defContext context) { return VisitChildren(context); }

        public UstNode VisitSqlj_object_type([NotNull] plsqlParser.Sqlj_object_typeContext context) { return VisitChildren(context); }

        public UstNode VisitType_body([NotNull] plsqlParser.Type_bodyContext context) { return VisitChildren(context); }

        public UstNode VisitType_body_elements([NotNull] plsqlParser.Type_body_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitMap_order_func_declaration([NotNull] plsqlParser.Map_order_func_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitSubprog_decl_in_type([NotNull] plsqlParser.Subprog_decl_in_typeContext context) { return VisitChildren(context); }

        public UstNode VisitProc_decl_in_type([NotNull] plsqlParser.Proc_decl_in_typeContext context) { return VisitChildren(context); }

        public UstNode VisitFunc_decl_in_type([NotNull] plsqlParser.Func_decl_in_typeContext context) { return VisitChildren(context); }

        public UstNode VisitConstructor_declaration([NotNull] plsqlParser.Constructor_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitModifier_clause([NotNull] plsqlParser.Modifier_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitObject_member_spec([NotNull] plsqlParser.Object_member_specContext context) { return VisitChildren(context); }

        public UstNode VisitSqlj_object_type_attr([NotNull] plsqlParser.Sqlj_object_type_attrContext context) { return VisitChildren(context); }

        public UstNode VisitElement_spec([NotNull] plsqlParser.Element_specContext context) { return VisitChildren(context); }

        public UstNode VisitElement_spec_options([NotNull] plsqlParser.Element_spec_optionsContext context) { return VisitChildren(context); }

        public UstNode VisitSubprogram_spec([NotNull] plsqlParser.Subprogram_specContext context) { return VisitChildren(context); }

        public UstNode VisitType_procedure_spec([NotNull] plsqlParser.Type_procedure_specContext context) { return VisitChildren(context); }

        public UstNode VisitType_function_spec([NotNull] plsqlParser.Type_function_specContext context) { return VisitChildren(context); }

        public UstNode VisitConstructor_spec([NotNull] plsqlParser.Constructor_specContext context) { return VisitChildren(context); }

        public UstNode VisitMap_order_function_spec([NotNull] plsqlParser.Map_order_function_specContext context) { return VisitChildren(context); }

        public UstNode VisitPragma_clause([NotNull] plsqlParser.Pragma_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPragma_elements([NotNull] plsqlParser.Pragma_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitType_elements_parameter([NotNull] plsqlParser.Type_elements_parameterContext context) { return VisitChildren(context); }

        public UstNode VisitDrop_sequence([NotNull] plsqlParser.Drop_sequenceContext context) { return VisitChildren(context); }

        public UstNode VisitAlter_sequence([NotNull] plsqlParser.Alter_sequenceContext context) { return VisitChildren(context); }

        public UstNode VisitCreate_sequence([NotNull] plsqlParser.Create_sequenceContext context) { return VisitChildren(context); }

        public UstNode VisitSequence_spec([NotNull] plsqlParser.Sequence_specContext context) { return VisitChildren(context); }

        public UstNode VisitSequence_start_clause([NotNull] plsqlParser.Sequence_start_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitInvoker_rights_clause([NotNull] plsqlParser.Invoker_rights_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCompiler_parameters_clause([NotNull] plsqlParser.Compiler_parameters_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCall_spec([NotNull] plsqlParser.Call_specContext context) { return VisitChildren(context); }

        public UstNode VisitJava_spec([NotNull] plsqlParser.Java_specContext context) { return VisitChildren(context); }

        public UstNode VisitC_spec([NotNull] plsqlParser.C_specContext context) { return VisitChildren(context); }

        public UstNode VisitC_agent_in_clause([NotNull] plsqlParser.C_agent_in_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitC_parameters_clause([NotNull] plsqlParser.C_parameters_clauseContext context) { return VisitChildren(context); }

        /// <returns><see cref="ParameterDeclaration"/></returns>
        public UstNode VisitParameter([NotNull] plsqlParser.ParameterContext context)
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

        public UstNode VisitDefault_value_part([NotNull] plsqlParser.Default_value_partContext context) { return VisitChildren(context); }

        public UstNode VisitDeclare_spec([NotNull] plsqlParser.Declare_specContext context) { return VisitChildren(context); }

        public UstNode VisitVariable_declaration([NotNull] plsqlParser.Variable_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitSubtype_declaration([NotNull] plsqlParser.Subtype_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitCursor_declaration([NotNull] plsqlParser.Cursor_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitParameter_spec([NotNull] plsqlParser.Parameter_specContext context) { return VisitChildren(context); }

        public UstNode VisitException_declaration([NotNull] plsqlParser.Exception_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitPragma_declaration([NotNull] plsqlParser.Pragma_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitRecord_declaration([NotNull] plsqlParser.Record_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitRecord_type_dec([NotNull] plsqlParser.Record_type_decContext context) { return VisitChildren(context); }

        public UstNode VisitField_spec([NotNull] plsqlParser.Field_specContext context) { return VisitChildren(context); }

        public UstNode VisitRecord_var_dec([NotNull] plsqlParser.Record_var_decContext context) { return VisitChildren(context); }

        public UstNode VisitTable_declaration([NotNull] plsqlParser.Table_declarationContext context) { return VisitChildren(context); }

        public UstNode VisitTable_type_dec([NotNull] plsqlParser.Table_type_decContext context) { return VisitChildren(context); }

        public UstNode VisitTable_indexed_by_part([NotNull] plsqlParser.Table_indexed_by_partContext context) { return VisitChildren(context); }

        public UstNode VisitVarray_type_def([NotNull] plsqlParser.Varray_type_defContext context) { return VisitChildren(context); }

        public UstNode VisitTable_var_dec([NotNull] plsqlParser.Table_var_decContext context) { return VisitChildren(context); }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitSeq_of_statements([NotNull] plsqlParser.Seq_of_statementsContext context)
        {
            var result = new BlockStatement(context.statement().Select(s => (Statement)Visit(s)).ToArray(),
                context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitLabel_declaration([NotNull] plsqlParser.Label_declarationContext context) { return VisitChildren(context); }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitStatement([NotNull] plsqlParser.StatementContext context)
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

        public UstNode VisitAssignment_statement([NotNull] plsqlParser.Assignment_statementContext context)
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

        public UstNode VisitContinue_statement([NotNull] plsqlParser.Continue_statementContext context)
        {
            return new ContinueStatement(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitExit_statement([NotNull] plsqlParser.Exit_statementContext context)
        {
            return new BreakStatement(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitGoto_statement([NotNull] plsqlParser.Goto_statementContext context) { return VisitChildren(context); }

        /// <returns><see cref="IfElseStatement"/></returns>
        public UstNode VisitIf_statement([NotNull] plsqlParser.If_statementContext context)
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
        public UstNode VisitElsif_part([NotNull] plsqlParser.Elsif_partContext context)
        {
            var condition = (Expression)Visit(context.condition());
            var block = (BlockStatement)Visit(context.seq_of_statements());
            var result = new IfElseStatement(condition, block, context.GetTextSpan(), FileNode);
            return result;
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitElse_part([NotNull] plsqlParser.Else_partContext context)
        {
            return (BlockStatement)Visit(context.seq_of_statements());
        }

        /// <returns><see cref="Statement"/></returns>
        public UstNode VisitLoop_statement([NotNull] plsqlParser.Loop_statementContext context)
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
                    throw new NotImplementedException();
                }
            }
            return result;
        }

        public UstNode VisitCursor_loop_param([NotNull] plsqlParser.Cursor_loop_paramContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitForall_statement([NotNull] plsqlParser.Forall_statementContext context) { return VisitChildren(context); }

        public UstNode VisitBounds_clause([NotNull] plsqlParser.Bounds_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitBetween_bound([NotNull] plsqlParser.Between_boundContext context) { return VisitChildren(context); }

        public UstNode VisitLower_bound([NotNull] plsqlParser.Lower_boundContext context) { return VisitChildren(context); }

        public UstNode VisitUpper_bound([NotNull] plsqlParser.Upper_boundContext context) { return VisitChildren(context); }

        public UstNode VisitNull_statement([NotNull] plsqlParser.Null_statementContext context)
        {
            return new NullLiteral(context.GetTextSpan(), FileNode);
        }

        public UstNode VisitRaise_statement([NotNull] plsqlParser.Raise_statementContext context) { return VisitChildren(context); }

        public UstNode VisitReturn_statement([NotNull] plsqlParser.Return_statementContext context)
        {
            Expression returnExpression = null;
            if (context.condition() != null)
            {
                returnExpression = (Expression)Visit(context.condition());
            }
            var result = new ReturnStatement(returnExpression, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitFunction_call([NotNull] plsqlParser.Function_callContext context)
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
        public UstNode VisitBody([NotNull] plsqlParser.BodyContext context)
        {
            BlockStatement result;
            var block = (BlockStatement)Visit(context.seq_of_statements());
            if (context.exception_handler().Length > 0)
            {
                var tryCatch = new TryCatchStatement(block, context.GetTextSpan(), FileNode);
                tryCatch.CatchClauses = context.exception_handler().Select(handler =>
                    (CatchClause)Visit(handler)).ToArray();
                result = new BlockStatement(new Statement[] { tryCatch }, context.GetTextSpan(), FileNode);
            }
            else
            {
                result = block;
            }
            return result;
        }

        /// <returns><see cref="CatchClause"/></returns>
        public UstNode VisitException_handler([NotNull] plsqlParser.Exception_handlerContext context)
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

        public UstNode VisitTrigger_block([NotNull] plsqlParser.Trigger_blockContext context) { return VisitChildren(context); }

        /// <returns><see cref="BlockStatement"/></returns>
        public UstNode VisitBlock([NotNull] plsqlParser.BlockContext context)
        {
            var declare = new BlockStatement(context.declare_spec().Select(s => Visit(s).ToStatementIfRequired()).ToArray(),
                context.GetTextSpan(), FileNode);
            var body = (BlockStatement)Visit(context.body());

            var result = new BlockStatement(new Statement[] { declare, body }, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitSql_statement([NotNull] plsqlParser.Sql_statementContext context) { return VisitChildren(context); }

        public UstNode VisitExecute_immediate([NotNull] plsqlParser.Execute_immediateContext context) { return VisitChildren(context); }

        public UstNode VisitDynamic_returning_clause([NotNull] plsqlParser.Dynamic_returning_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitData_manipulation_language_statements([NotNull] plsqlParser.Data_manipulation_language_statementsContext context) { return VisitChildren(context); }

        public UstNode VisitCursor_manipulation_statements([NotNull] plsqlParser.Cursor_manipulation_statementsContext context) { return VisitChildren(context); }

        public UstNode VisitClose_statement([NotNull] plsqlParser.Close_statementContext context) { return VisitChildren(context); }

        public UstNode VisitOpen_statement([NotNull] plsqlParser.Open_statementContext context) { return VisitChildren(context); }

        public UstNode VisitFetch_statement([NotNull] plsqlParser.Fetch_statementContext context) { return VisitChildren(context); }

        public UstNode VisitOpen_for_statement([NotNull] plsqlParser.Open_for_statementContext context) { return VisitChildren(context); }

        public UstNode VisitTransaction_control_statements([NotNull] plsqlParser.Transaction_control_statementsContext context) { return VisitChildren(context); }

        public UstNode VisitSet_transaction_command([NotNull] plsqlParser.Set_transaction_commandContext context) { return VisitChildren(context); }

        public UstNode VisitSet_constraint_command([NotNull] plsqlParser.Set_constraint_commandContext context) { return VisitChildren(context); }

        public UstNode VisitCommit_statement([NotNull] plsqlParser.Commit_statementContext context) { return VisitChildren(context); }

        public UstNode VisitWrite_clause([NotNull] plsqlParser.Write_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitRollback_statement([NotNull] plsqlParser.Rollback_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSavepoint_statement([NotNull] plsqlParser.Savepoint_statementContext context) { return VisitChildren(context); }

        public UstNode VisitExplain_statement([NotNull] plsqlParser.Explain_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSelect_statement([NotNull] plsqlParser.Select_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery_factoring_clause([NotNull] plsqlParser.Subquery_factoring_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitFactoring_element([NotNull] plsqlParser.Factoring_elementContext context) { return VisitChildren(context); }

        public UstNode VisitSearch_clause([NotNull] plsqlParser.Search_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCycle_clause([NotNull] plsqlParser.Cycle_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery([NotNull] plsqlParser.SubqueryContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery_operation_part([NotNull] plsqlParser.Subquery_operation_partContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery_basic_elements([NotNull] plsqlParser.Subquery_basic_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitQuery_block([NotNull] plsqlParser.Query_blockContext context) { return VisitChildren(context); }

        public UstNode VisitSelected_element([NotNull] plsqlParser.Selected_elementContext context) { return VisitChildren(context); }

        public UstNode VisitFrom_clause([NotNull] plsqlParser.From_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSelect_list_elements([NotNull] plsqlParser.Select_list_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitTable_ref_list([NotNull] plsqlParser.Table_ref_listContext context) { return VisitChildren(context); }

        public UstNode VisitTable_ref([NotNull] plsqlParser.Table_refContext context) { return VisitChildren(context); }

        public UstNode VisitTable_ref_aux([NotNull] plsqlParser.Table_ref_auxContext context) { return VisitChildren(context); }

        public UstNode VisitJoin_clause([NotNull] plsqlParser.Join_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitJoin_on_part([NotNull] plsqlParser.Join_on_partContext context) { return VisitChildren(context); }

        public UstNode VisitJoin_using_part([NotNull] plsqlParser.Join_using_partContext context) { return VisitChildren(context); }

        public UstNode VisitOuter_join_type([NotNull] plsqlParser.Outer_join_typeContext context) { return VisitChildren(context); }

        public UstNode VisitQuery_partition_clause([NotNull] plsqlParser.Query_partition_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitFlashback_query_clause([NotNull] plsqlParser.Flashback_query_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_clause([NotNull] plsqlParser.Pivot_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_element([NotNull] plsqlParser.Pivot_elementContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_for_clause([NotNull] plsqlParser.Pivot_for_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_in_clause([NotNull] plsqlParser.Pivot_in_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_in_clause_element([NotNull] plsqlParser.Pivot_in_clause_elementContext context) { return VisitChildren(context); }

        public UstNode VisitPivot_in_clause_elements([NotNull] plsqlParser.Pivot_in_clause_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitUnpivot_clause([NotNull] plsqlParser.Unpivot_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitUnpivot_in_clause([NotNull] plsqlParser.Unpivot_in_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitUnpivot_in_elements([NotNull] plsqlParser.Unpivot_in_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitHierarchical_query_clause([NotNull] plsqlParser.Hierarchical_query_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitStart_part([NotNull] plsqlParser.Start_partContext context) { return VisitChildren(context); }

        public UstNode VisitGroup_by_clause([NotNull] plsqlParser.Group_by_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitGroup_by_elements([NotNull] plsqlParser.Group_by_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitRollup_cube_clause([NotNull] plsqlParser.Rollup_cube_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitGrouping_sets_clause([NotNull] plsqlParser.Grouping_sets_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitGrouping_sets_elements([NotNull] plsqlParser.Grouping_sets_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitHaving_clause([NotNull] plsqlParser.Having_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitModel_clause([NotNull] plsqlParser.Model_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCell_reference_options([NotNull] plsqlParser.Cell_reference_optionsContext context) { return VisitChildren(context); }

        public UstNode VisitReturn_rows_clause([NotNull] plsqlParser.Return_rows_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitReference_model([NotNull] plsqlParser.Reference_modelContext context) { return VisitChildren(context); }

        public UstNode VisitMain_model([NotNull] plsqlParser.Main_modelContext context) { return VisitChildren(context); }

        public UstNode VisitModel_column_clauses([NotNull] plsqlParser.Model_column_clausesContext context) { return VisitChildren(context); }

        public UstNode VisitModel_column_partition_part([NotNull] plsqlParser.Model_column_partition_partContext context) { return VisitChildren(context); }

        public UstNode VisitModel_column_list([NotNull] plsqlParser.Model_column_listContext context) { return VisitChildren(context); }

        public UstNode VisitModel_column([NotNull] plsqlParser.Model_columnContext context) { return VisitChildren(context); }

        public UstNode VisitModel_rules_clause([NotNull] plsqlParser.Model_rules_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitModel_rules_part([NotNull] plsqlParser.Model_rules_partContext context) { return VisitChildren(context); }

        public UstNode VisitModel_rules_element([NotNull] plsqlParser.Model_rules_elementContext context) { return VisitChildren(context); }

        public UstNode VisitCell_assignment([NotNull] plsqlParser.Cell_assignmentContext context) { return VisitChildren(context); }

        public UstNode VisitModel_iterate_clause([NotNull] plsqlParser.Model_iterate_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitUntil_part([NotNull] plsqlParser.Until_partContext context) { return VisitChildren(context); }

        public UstNode VisitOrder_by_clause([NotNull] plsqlParser.Order_by_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitOrder_by_elements([NotNull] plsqlParser.Order_by_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitFor_update_clause([NotNull] plsqlParser.For_update_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitFor_update_of_part([NotNull] plsqlParser.For_update_of_partContext context) { return VisitChildren(context); }

        public UstNode VisitFor_update_options([NotNull] plsqlParser.For_update_optionsContext context) { return VisitChildren(context); }

        public UstNode VisitUpdate_statement([NotNull] plsqlParser.Update_statementContext context) { return VisitChildren(context); }

        public UstNode VisitUpdate_set_clause([NotNull] plsqlParser.Update_set_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitColumn_based_update_set_clause([NotNull] plsqlParser.Column_based_update_set_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitDelete_statement([NotNull] plsqlParser.Delete_statementContext context) { return VisitChildren(context); }

        public UstNode VisitInsert_statement([NotNull] plsqlParser.Insert_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSingle_table_insert([NotNull] plsqlParser.Single_table_insertContext context) { return VisitChildren(context); }

        public UstNode VisitMulti_table_insert([NotNull] plsqlParser.Multi_table_insertContext context) { return VisitChildren(context); }

        public UstNode VisitMulti_table_element([NotNull] plsqlParser.Multi_table_elementContext context) { return VisitChildren(context); }

        public UstNode VisitConditional_insert_clause([NotNull] plsqlParser.Conditional_insert_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitConditional_insert_when_part([NotNull] plsqlParser.Conditional_insert_when_partContext context) { return VisitChildren(context); }

        public UstNode VisitConditional_insert_else_part([NotNull] plsqlParser.Conditional_insert_else_partContext context) { return VisitChildren(context); }

        public UstNode VisitInsert_into_clause([NotNull] plsqlParser.Insert_into_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitValues_clause([NotNull] plsqlParser.Values_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_statement([NotNull] plsqlParser.Merge_statementContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_update_clause([NotNull] plsqlParser.Merge_update_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_element([NotNull] plsqlParser.Merge_elementContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_update_delete_part([NotNull] plsqlParser.Merge_update_delete_partContext context) { return VisitChildren(context); }

        public UstNode VisitMerge_insert_clause([NotNull] plsqlParser.Merge_insert_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSelected_tableview([NotNull] plsqlParser.Selected_tableviewContext context) { return VisitChildren(context); }

        public UstNode VisitLock_table_statement([NotNull] plsqlParser.Lock_table_statementContext context) { return VisitChildren(context); }

        public UstNode VisitWait_nowait_part([NotNull] plsqlParser.Wait_nowait_partContext context) { return VisitChildren(context); }

        public UstNode VisitLock_table_element([NotNull] plsqlParser.Lock_table_elementContext context) { return VisitChildren(context); }

        public UstNode VisitLock_mode([NotNull] plsqlParser.Lock_modeContext context) { return VisitChildren(context); }

        public UstNode VisitGeneral_table_ref([NotNull] plsqlParser.General_table_refContext context) { return VisitChildren(context); }

        public UstNode VisitStatic_returning_clause([NotNull] plsqlParser.Static_returning_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitError_logging_clause([NotNull] plsqlParser.Error_logging_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitError_logging_into_part([NotNull] plsqlParser.Error_logging_into_partContext context) { return VisitChildren(context); }

        public UstNode VisitError_logging_reject_part([NotNull] plsqlParser.Error_logging_reject_partContext context) { return VisitChildren(context); }

        public UstNode VisitDml_table_expression_clause([NotNull] plsqlParser.Dml_table_expression_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitTable_collection_expression([NotNull] plsqlParser.Table_collection_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitSubquery_restriction_clause([NotNull] plsqlParser.Subquery_restriction_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSample_clause([NotNull] plsqlParser.Sample_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitSeed_part([NotNull] plsqlParser.Seed_partContext context) { return VisitChildren(context); }

        public UstNode VisitCursor_expression([NotNull] plsqlParser.Cursor_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitExpression_list([NotNull] plsqlParser.Expression_listContext context) { return VisitChildren(context); }

        public UstNode VisitCondition([NotNull] plsqlParser.ConditionContext context) { return VisitChildren(context); }

        public UstNode VisitExpression([NotNull] plsqlParser.ExpressionContext context) { return VisitChildren(context); }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitLogical_or_expression([NotNull] plsqlParser.Logical_or_expressionContext context)
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
        public UstNode VisitLogical_and_expression([NotNull] plsqlParser.Logical_and_expressionContext context)
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
        public UstNode VisitNegated_expression([NotNull] plsqlParser.Negated_expressionContext context)
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

        public UstNode VisitEquality_expression([NotNull] plsqlParser.Equality_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitMultiset_expression([NotNull] plsqlParser.Multiset_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitMultiset_type([NotNull] plsqlParser.Multiset_typeContext context) { return VisitChildren(context); }

        /// <returns><see cref="Expression"/></returns>
        public UstNode VisitRelational_expression([NotNull] plsqlParser.Relational_expressionContext context)
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

        public UstNode VisitCompound_expression([NotNull] plsqlParser.Compound_expressionContext context) { return VisitChildren(context); }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public UstNode VisitRelational_operator([NotNull] plsqlParser.Relational_operatorContext context)
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

        public UstNode VisitLike_type([NotNull] plsqlParser.Like_typeContext context) { return VisitChildren(context); }

        public UstNode VisitLike_escape_part([NotNull] plsqlParser.Like_escape_partContext context) { return VisitChildren(context); }

        public UstNode VisitIn_elements([NotNull] plsqlParser.In_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitBetween_elements([NotNull] plsqlParser.Between_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitConcatenation([NotNull] plsqlParser.ConcatenationContext context)
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
        public UstNode VisitAdditive_expression([NotNull] plsqlParser.Additive_expressionContext context)
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
        public UstNode VisitMultiply_expression([NotNull] plsqlParser.Multiply_expressionContext context)
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

        public UstNode VisitDatetime_expression([NotNull] plsqlParser.Datetime_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitInterval_expression([NotNull] plsqlParser.Interval_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitModel_expression([NotNull] plsqlParser.Model_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitModel_expression_element([NotNull] plsqlParser.Model_expression_elementContext context) { return VisitChildren(context); }

        public UstNode VisitSingle_column_for_loop([NotNull] plsqlParser.Single_column_for_loopContext context) { return VisitChildren(context); }

        public UstNode VisitFor_like_part([NotNull] plsqlParser.For_like_partContext context) { return VisitChildren(context); }

        public UstNode VisitFor_increment_decrement_type([NotNull] plsqlParser.For_increment_decrement_typeContext context) { return VisitChildren(context); }

        public UstNode VisitMulti_column_for_loop([NotNull] plsqlParser.Multi_column_for_loopContext context) { return VisitChildren(context); }

        public UstNode VisitUnary_expression([NotNull] plsqlParser.Unary_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitCase_statement([NotNull] plsqlParser.Case_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSimple_case_statement([NotNull] plsqlParser.Simple_case_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSimple_case_when_part([NotNull] plsqlParser.Simple_case_when_partContext context) { return VisitChildren(context); }

        public UstNode VisitSearched_case_statement([NotNull] plsqlParser.Searched_case_statementContext context) { return VisitChildren(context); }

        public UstNode VisitSearched_case_when_part([NotNull] plsqlParser.Searched_case_when_partContext context) { return VisitChildren(context); }

        public UstNode VisitCase_else_part([NotNull] plsqlParser.Case_else_partContext context) { return VisitChildren(context); }

        public UstNode VisitAtom([NotNull] plsqlParser.AtomContext context) { return VisitChildren(context); }

        public UstNode VisitExpression_or_vector([NotNull] plsqlParser.Expression_or_vectorContext context) { return VisitChildren(context); }

        public UstNode VisitVector_expr([NotNull] plsqlParser.Vector_exprContext context) { return VisitChildren(context); }

        public UstNode VisitQuantified_expression([NotNull] plsqlParser.Quantified_expressionContext context) { return VisitChildren(context); }

        public UstNode VisitStandard_function([NotNull] plsqlParser.Standard_functionContext context) { return VisitChildren(context); }

        public UstNode VisitOver_clause_keyword([NotNull] plsqlParser.Over_clause_keywordContext context) { return VisitChildren(context); }

        public UstNode VisitWithin_or_over_clause_keyword([NotNull] plsqlParser.Within_or_over_clause_keywordContext context) { return VisitChildren(context); }

        public UstNode VisitStandard_prediction_function_keyword([NotNull] plsqlParser.Standard_prediction_function_keywordContext context) { return VisitChildren(context); }

        public UstNode VisitOver_clause([NotNull] plsqlParser.Over_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitWindowing_clause([NotNull] plsqlParser.Windowing_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitWindowing_type([NotNull] plsqlParser.Windowing_typeContext context) { return VisitChildren(context); }

        public UstNode VisitWindowing_elements([NotNull] plsqlParser.Windowing_elementsContext context) { return VisitChildren(context); }

        public UstNode VisitUsing_clause([NotNull] plsqlParser.Using_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitUsing_element([NotNull] plsqlParser.Using_elementContext context) { return VisitChildren(context); }

        public UstNode VisitCollect_order_by_part([NotNull] plsqlParser.Collect_order_by_partContext context) { return VisitChildren(context); }

        public UstNode VisitWithin_or_over_part([NotNull] plsqlParser.Within_or_over_partContext context) { return VisitChildren(context); }

        public UstNode VisitCost_matrix_clause([NotNull] plsqlParser.Cost_matrix_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_passing_clause([NotNull] plsqlParser.Xml_passing_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_attributes_clause([NotNull] plsqlParser.Xml_attributes_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_namespaces_clause([NotNull] plsqlParser.Xml_namespaces_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_table_column([NotNull] plsqlParser.Xml_table_columnContext context) { return VisitChildren(context); }

        public UstNode VisitXml_general_default_part([NotNull] plsqlParser.Xml_general_default_partContext context) { return VisitChildren(context); }

        public UstNode VisitXml_multiuse_expression_element([NotNull] plsqlParser.Xml_multiuse_expression_elementContext context) { return VisitChildren(context); }

        public UstNode VisitXmlroot_param_version_part([NotNull] plsqlParser.Xmlroot_param_version_partContext context) { return VisitChildren(context); }

        public UstNode VisitXmlroot_param_standalone_part([NotNull] plsqlParser.Xmlroot_param_standalone_partContext context) { return VisitChildren(context); }

        public UstNode VisitXmlserialize_param_enconding_part([NotNull] plsqlParser.Xmlserialize_param_enconding_partContext context) { return VisitChildren(context); }

        public UstNode VisitXmlserialize_param_version_part([NotNull] plsqlParser.Xmlserialize_param_version_partContext context) { return VisitChildren(context); }

        public UstNode VisitXmlserialize_param_ident_part([NotNull] plsqlParser.Xmlserialize_param_ident_partContext context) { return VisitChildren(context); }

        public UstNode VisitSql_plus_command([NotNull] plsqlParser.Sql_plus_commandContext context) { return VisitChildren(context); }

        public UstNode VisitWhenever_command([NotNull] plsqlParser.Whenever_commandContext context) { return VisitChildren(context); }

        public UstNode VisitSet_command([NotNull] plsqlParser.Set_commandContext context) { return VisitChildren(context); }

        public UstNode VisitExit_command([NotNull] plsqlParser.Exit_commandContext context) { return VisitChildren(context); }

        public UstNode VisitPrompt_command([NotNull] plsqlParser.Prompt_commandContext context) { return VisitChildren(context); }

        public UstNode VisitPartition_extension_clause([NotNull] plsqlParser.Partition_extension_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitColumn_alias([NotNull] plsqlParser.Column_aliasContext context) { return VisitChildren(context); }

        public UstNode VisitTable_alias([NotNull] plsqlParser.Table_aliasContext context) { return VisitChildren(context); }

        public UstNode VisitAlias_quoted_string([NotNull] plsqlParser.Alias_quoted_stringContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitWhere_clause([NotNull] plsqlParser.Where_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitCurrent_of_clause([NotNull] plsqlParser.Current_of_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitInto_clause([NotNull] plsqlParser.Into_clauseContext context) { return VisitChildren(context); }

        public UstNode VisitXml_column_name([NotNull] plsqlParser.Xml_column_nameContext context) { return VisitChildren(context); }

        public UstNode VisitCost_class_name([NotNull] plsqlParser.Cost_class_nameContext context) { return VisitChildren(context); }

        public UstNode VisitAttribute_name([NotNull] plsqlParser.Attribute_nameContext context) { return VisitChildren(context); }

        public UstNode VisitSavepoint_name([NotNull] plsqlParser.Savepoint_nameContext context) { return VisitChildren(context); }

        public UstNode VisitRollback_segment_name([NotNull] plsqlParser.Rollback_segment_nameContext context) { return VisitChildren(context); }

        public UstNode VisitTable_var_name([NotNull] plsqlParser.Table_var_nameContext context) { return VisitChildren(context); }

        public UstNode VisitSchema_name([NotNull] plsqlParser.Schema_nameContext context) { return VisitChildren(context); }

        public UstNode VisitRoutine_name([NotNull] plsqlParser.Routine_nameContext context)
        {
            Expression result = (IdToken)Visit(context.id());
            if (context.id_expression().Length > 0)
            {
                var firstSpan = context.id().GetTextSpan();
                for (int i = 0; i < context.id_expression().Length; i++)
                {
                    result = new MemberReferenceExpression(result,
                        (Expression)Visit(context.id_expression(i)), firstSpan.Union(context.id_expression(i).GetTextSpan()), FileNode);
                }
            }
            return result;
        }

        public UstNode VisitPackage_name([NotNull] plsqlParser.Package_nameContext context) { return VisitChildren(context); }

        public UstNode VisitImplementation_type_name([NotNull] plsqlParser.Implementation_type_nameContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitParameter_name([NotNull] plsqlParser.Parameter_nameContext context) { return VisitChildren(context); }

        public UstNode VisitReference_model_name([NotNull] plsqlParser.Reference_model_nameContext context) { return VisitChildren(context); }

        public UstNode VisitMain_model_name([NotNull] plsqlParser.Main_model_nameContext context) { return VisitChildren(context); }

        public UstNode VisitAggregate_function_name([NotNull] plsqlParser.Aggregate_function_nameContext context) { return VisitChildren(context); }

        public UstNode VisitQuery_name([NotNull] plsqlParser.Query_nameContext context) { return VisitChildren(context); }

        public UstNode VisitConstraint_name([NotNull] plsqlParser.Constraint_nameContext context) { return VisitChildren(context); }

        public UstNode VisitLabel_name([NotNull] plsqlParser.Label_nameContext context) { return VisitChildren(context); }

        public UstNode VisitType_name([NotNull] plsqlParser.Type_nameContext context)
        {
            var typeName = new TypeToken(
                string.Join(".", context.id_expression().Select(expr => ((IdToken)Visit(expr)).TextValue)),
                context.GetTextSpan(), FileNode);
            return typeName;
        }

        public UstNode VisitSequence_name([NotNull] plsqlParser.Sequence_nameContext context) { return VisitChildren(context); }

        /// <returns><see cref="TypeToken"/></returns>
        public UstNode VisitException_name([NotNull] plsqlParser.Exception_nameContext context)
        {
            var result = new TypeToken(context.GetText(), context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitFunction_name([NotNull] plsqlParser.Function_nameContext context) { return VisitChildren(context); }

        public UstNode VisitProcedure_name([NotNull] plsqlParser.Procedure_nameContext context) { return VisitChildren(context); }

        public UstNode VisitTrigger_name([NotNull] plsqlParser.Trigger_nameContext context) { return VisitChildren(context); }

        public UstNode VisitVariable_name([NotNull] plsqlParser.Variable_nameContext context) { return VisitChildren(context); }

        public UstNode VisitIndex_name([NotNull] plsqlParser.Index_nameContext context) { return VisitChildren(context); }

        public UstNode VisitCursor_name([NotNull] plsqlParser.Cursor_nameContext context) { return VisitChildren(context); }

        public UstNode VisitRecord_name([NotNull] plsqlParser.Record_nameContext context) { return VisitChildren(context); }

        public UstNode VisitCollection_name([NotNull] plsqlParser.Collection_nameContext context) { return VisitChildren(context); }

        public UstNode VisitLink_name([NotNull] plsqlParser.Link_nameContext context) { return VisitChildren(context); }

        public UstNode VisitColumn_name([NotNull] plsqlParser.Column_nameContext context) { return VisitChildren(context); }

        public UstNode VisitTableview_name([NotNull] plsqlParser.Tableview_nameContext context) { return VisitChildren(context); }

        public UstNode VisitChar_set_name([NotNull] plsqlParser.Char_set_nameContext context) { return VisitChildren(context); }

        public UstNode VisitKeep_clause([NotNull] plsqlParser.Keep_clauseContext context) { return VisitChildren(context); }

        /// <returns><see cref="ArgsNode"/></returns>
        public UstNode VisitFunction_argument([NotNull] plsqlParser.Function_argumentContext context)
        {
            List<Expression> args = context.argument().Select(arg => (Expression)Visit(arg)).ToList();
            if (context.keep_clause() != null)
            {
                args.Add((Expression)Visit(context.keep_clause()));
            }
            var result = new ArgsNode(args, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitFunction_argument_analytic([NotNull] plsqlParser.Function_argument_analyticContext context) { return VisitChildren(context); }

        public UstNode VisitFunction_argument_modeling([NotNull] plsqlParser.Function_argument_modelingContext context) { return VisitChildren(context); }

        public UstNode VisitRespect_or_ignore_nulls([NotNull] plsqlParser.Respect_or_ignore_nullsContext context) { return VisitChildren(context); }

        public UstNode VisitArgument([NotNull] plsqlParser.ArgumentContext context) { return VisitChildren(context); }

        /// <returns><see cref="TypeToken"/></returns>
        public UstNode VisitType_spec([NotNull] plsqlParser.Type_specContext context)
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
        public UstNode VisitDatatype([NotNull] plsqlParser.DatatypeContext context)
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

        public UstNode VisitPrecision_part([NotNull] plsqlParser.Precision_partContext context) { return VisitChildren(context); }

        /// <returns><see cref="TypeToken"/></returns>
        public UstNode VisitNative_datatype_element([NotNull] plsqlParser.Native_datatype_elementContext context)
        {
            return new TypeToken(context.GetText(), context.GetTextSpan(), FileNode);
        }

        public UstNode VisitBind_variable([NotNull] plsqlParser.Bind_variableContext context)
        {
            var result = VisitChildren(context);
            if (result == null)
            {
                result = new NullLiteral(context.GetTextSpan(), FileNode);
            }
            return result;
        }

        public UstNode VisitGeneral_element([NotNull] plsqlParser.General_elementContext context) { return VisitChildren(context); }

        public UstNode VisitGeneral_element_part([NotNull] plsqlParser.General_element_partContext context)
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

        public UstNode VisitTable_element([NotNull] plsqlParser.Table_elementContext context) { return VisitChildren(context); }

        /// <returns><see cref="Token"/></returns>
        public UstNode VisitConstant([NotNull] plsqlParser.ConstantContext context)
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

        public UstNode VisitNumeric([NotNull] plsqlParser.NumericContext context)
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

        public UstNode VisitQuoted_string([NotNull] plsqlParser.Quoted_stringContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitId([NotNull] plsqlParser.IdContext context)
        {
            return VisitChildren(context);
        }

        /// <returns><see cref="IdToken"/></returns>
        public UstNode VisitId_expression([NotNull] plsqlParser.Id_expressionContext context)
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
        public UstNode VisitNot_equal_op([NotNull] plsqlParser.Not_equal_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.NotEqual, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public UstNode VisitGreater_than_or_equals_op([NotNull] plsqlParser.Greater_than_or_equals_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.GreaterOrEqual, context.GetTextSpan(), FileNode);
        }

        /// <returns><see cref="BinaryOperatorLiteral"/></returns>
        public UstNode VisitLess_than_or_equals_op([NotNull] plsqlParser.Less_than_or_equals_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.LessOrEqual, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitConcatenation_op([NotNull] plsqlParser.Concatenation_opContext context)
        {
            return new BinaryOperatorLiteral(BinaryOperator.Plus, context.GetTextSpan(), FileNode);
        }

        public UstNode VisitOuter_join_sign([NotNull] plsqlParser.Outer_join_signContext context) { return VisitChildren(context); }

        public UstNode VisitRegular_id([NotNull] plsqlParser.Regular_idContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitShow_errors_command([NotNull] plsqlParser.Show_errors_commandContext context) { return VisitChildren(context); }

        public UstNode VisitNumeric_negative([NotNull] plsqlParser.Numeric_negativeContext context) { return VisitChildren(context); }
    }
}
