using Antlr4.Runtime.Misc;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using PT.PM.MySqlParseTreeUst;
using System.Linq;
using System.Collections.Generic;
using static PT.PM.Common.Nodes.UstUtils;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Sql;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;

namespace PT.PM.SqlParseTreeUst
{
    public partial class MySqlAntlrConverter : AntlrConverter, IMySqlParserVisitor<Ust>
    {
        public override Language Language => MySql.Language;


        public Ust VisitRoot([NotNull] MySqlParser.RootContext context)
        {
            var statements = context.sqlStatements().sqlStatement();
            var resultNodes = new Statement[statements.Length];
            for(int i = 0; i < statements.Length; i++)
            {
                var visited = Visit(statements[i]);
                if (visited is Statement statement)
                {
                    resultNodes[i] = statement;
                }
                else
                {
                    resultNodes[i] = new WrapperStatement(visited);
                }
            }
            root.Nodes = resultNodes.ToArray();
            return root;
        }

        public Ust VisitAdministrationStatement([NotNull] MySqlParser.AdministrationStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAggregateFunctionCall([NotNull] MySqlParser.AggregateFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAggregateWindowedFunction([NotNull] MySqlParser.AggregateWindowedFunctionContext context)
        {
            var funcMultichild = (MultichildExpression)VisitChildren(context);
            var funcExprs = ExtractMultiChild(funcMultichild, new List<Expression>());
            var result = new InvocationExpression
                {
                    Target = funcExprs.FirstOrDefault(),
                    Arguments = funcExprs.Count > 1
                    ? new ArgsUst(funcExprs.GetRange(1, funcExprs.Count - 1))
                    : new ArgsUst(new List<Expression>()),
                    TextSpan = context.GetTextSpan()
                };
            return result;
        }

        public Ust VisitAlterByAddColumn([NotNull] MySqlParser.AlterByAddColumnContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByAddColumns([NotNull] MySqlParser.AlterByAddColumnsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByAddForeignKey([NotNull] MySqlParser.AlterByAddForeignKeyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByAddIndex([NotNull] MySqlParser.AlterByAddIndexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByAddPartition([NotNull] MySqlParser.AlterByAddPartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByAddPrimaryKey([NotNull] MySqlParser.AlterByAddPrimaryKeyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByAddSpecialIndex([NotNull] MySqlParser.AlterByAddSpecialIndexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByAddUniqueKey([NotNull] MySqlParser.AlterByAddUniqueKeyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByAnalyzePartitiion([NotNull] MySqlParser.AlterByAnalyzePartitiionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByChangeColumn([NotNull] MySqlParser.AlterByChangeColumnContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByChangeDefault([NotNull] MySqlParser.AlterByChangeDefaultContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByCheckPartition([NotNull] MySqlParser.AlterByCheckPartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByCoalescePartition([NotNull] MySqlParser.AlterByCoalescePartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByConvertCharset([NotNull] MySqlParser.AlterByConvertCharsetContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByDefaultCharset([NotNull] MySqlParser.AlterByDefaultCharsetContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByDisableKeys([NotNull] MySqlParser.AlterByDisableKeysContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByDiscardPartition([NotNull] MySqlParser.AlterByDiscardPartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByDiscardTablespace([NotNull] MySqlParser.AlterByDiscardTablespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByDropColumn([NotNull] MySqlParser.AlterByDropColumnContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByDropForeignKey([NotNull] MySqlParser.AlterByDropForeignKeyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByDropIndex([NotNull] MySqlParser.AlterByDropIndexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByDropPartition([NotNull] MySqlParser.AlterByDropPartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByDropPrimaryKey([NotNull] MySqlParser.AlterByDropPrimaryKeyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByEnableKeys([NotNull] MySqlParser.AlterByEnableKeysContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByExchangePartition([NotNull] MySqlParser.AlterByExchangePartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByForce([NotNull] MySqlParser.AlterByForceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByImportPartition([NotNull] MySqlParser.AlterByImportPartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByImportTablespace([NotNull] MySqlParser.AlterByImportTablespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByLock([NotNull] MySqlParser.AlterByLockContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByModifyColumn([NotNull] MySqlParser.AlterByModifyColumnContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByOptimizePartition([NotNull] MySqlParser.AlterByOptimizePartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByOrder([NotNull] MySqlParser.AlterByOrderContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByRebuildPartition([NotNull] MySqlParser.AlterByRebuildPartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByRemovePartitioning([NotNull] MySqlParser.AlterByRemovePartitioningContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByRename([NotNull] MySqlParser.AlterByRenameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByReorganizePartition([NotNull] MySqlParser.AlterByReorganizePartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByRepairPartition([NotNull] MySqlParser.AlterByRepairPartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterBySetAlgorithm([NotNull] MySqlParser.AlterBySetAlgorithmContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByTableOption([NotNull] MySqlParser.AlterByTableOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByTruncatePartition([NotNull] MySqlParser.AlterByTruncatePartitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByUpgradePartitioning([NotNull] MySqlParser.AlterByUpgradePartitioningContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterByValidate([NotNull] MySqlParser.AlterByValidateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterDatabase([NotNull] MySqlParser.AlterDatabaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterEvent([NotNull] MySqlParser.AlterEventContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterFunction([NotNull] MySqlParser.AlterFunctionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterInstance([NotNull] MySqlParser.AlterInstanceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterLogfileGroup([NotNull] MySqlParser.AlterLogfileGroupContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterProcedure([NotNull] MySqlParser.AlterProcedureContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterServer([NotNull] MySqlParser.AlterServerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterSimpleDatabase([NotNull] MySqlParser.AlterSimpleDatabaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterSpecification([NotNull] MySqlParser.AlterSpecificationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterTable([NotNull] MySqlParser.AlterTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterTablespace([NotNull] MySqlParser.AlterTablespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterUpgradeName([NotNull] MySqlParser.AlterUpgradeNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterUser([NotNull] MySqlParser.AlterUserContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterUserMysqlV56([NotNull] MySqlParser.AlterUserMysqlV56Context context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterUserMysqlV57([NotNull] MySqlParser.AlterUserMysqlV57Context context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAlterView([NotNull] MySqlParser.AlterViewContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnalyzeTable([NotNull] MySqlParser.AnalyzeTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAssignmentField([NotNull] MySqlParser.AssignmentFieldContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAtomTableItem([NotNull] MySqlParser.AtomTableItemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAuthPlugin([NotNull] MySqlParser.AuthPluginContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAutoIncrementColumnConstraint([NotNull] MySqlParser.AutoIncrementColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBeginWork([NotNull] MySqlParser.BeginWorkContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBetweenPredicate([NotNull] MySqlParser.BetweenPredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBinaryComparasionPredicate([NotNull] MySqlParser.BinaryComparasionPredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBinaryExpressionAtom([NotNull] MySqlParser.BinaryExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBinlogStatement([NotNull] MySqlParser.BinlogStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBitExpressionAtom([NotNull] MySqlParser.BitExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBitOperator([NotNull] MySqlParser.BitOperatorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBlockStatement([NotNull] MySqlParser.BlockStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBooleanLiteral([NotNull] MySqlParser.BooleanLiteralContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBoolMasterOption([NotNull] MySqlParser.BoolMasterOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCacheIndexStatement([NotNull] MySqlParser.CacheIndexStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCallStatement([NotNull] MySqlParser.CallStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCaseAlternative([NotNull] MySqlParser.CaseAlternativeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCaseFuncAlternative([NotNull] MySqlParser.CaseFuncAlternativeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCaseFunctionCall([NotNull] MySqlParser.CaseFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCaseStatement([NotNull] MySqlParser.CaseStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitChangeMaster([NotNull] MySqlParser.ChangeMasterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitChangeReplicationFilter([NotNull] MySqlParser.ChangeReplicationFilterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitChannelFlushOption([NotNull] MySqlParser.ChannelFlushOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitChannelOption([NotNull] MySqlParser.ChannelOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCharFunctionCall([NotNull] MySqlParser.CharFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCharsetName([NotNull] MySqlParser.CharsetNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCharsetNameBase([NotNull] MySqlParser.CharsetNameBaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitChecksumTable([NotNull] MySqlParser.ChecksumTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCheckTable([NotNull] MySqlParser.CheckTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCheckTableConstraint([NotNull] MySqlParser.CheckTableConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCheckTableOption([NotNull] MySqlParser.CheckTableOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCloseCursor([NotNull] MySqlParser.CloseCursorContext context)
        {
            var cursorArg = ConvertToInOutArgument(context.uid());
            var funcName = new IdToken(context.CLOSE().GetText(), context.CLOSE().GetTextSpan());
            return new InvocationExpression(funcName, new ArgsUst(cursorArg), context.GetTextSpan());
        }

        public Ust VisitCollateExpressionAtom([NotNull] MySqlParser.CollateExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCollationName([NotNull] MySqlParser.CollationNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCollectionDataType([NotNull] MySqlParser.CollectionDataTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumnConstraint([NotNull] MySqlParser.ColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumnCreateTable([NotNull] MySqlParser.ColumnCreateTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumnDeclaration([NotNull] MySqlParser.ColumnDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitColumnDefinition([NotNull] MySqlParser.ColumnDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCommentColumnConstraint([NotNull] MySqlParser.CommentColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCommitWork([NotNull] MySqlParser.CommitWorkContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComparisonOperator([NotNull] MySqlParser.ComparisonOperatorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCompoundStatement([NotNull] MySqlParser.CompoundStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitConnectionOption([NotNull] MySqlParser.ConnectionOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitConstant([NotNull] MySqlParser.ConstantContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitConstantExpressionAtom([NotNull] MySqlParser.ConstantExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitConstants([NotNull] MySqlParser.ConstantsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitConstraintDeclaration([NotNull] MySqlParser.ConstraintDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitConvertedDataType([NotNull] MySqlParser.ConvertedDataTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCopyCreateTable([NotNull] MySqlParser.CopyCreateTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateDatabase([NotNull] MySqlParser.CreateDatabaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateDatabaseOption([NotNull] MySqlParser.CreateDatabaseOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateDefinition([NotNull] MySqlParser.CreateDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateDefinitions([NotNull] MySqlParser.CreateDefinitionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateEvent([NotNull] MySqlParser.CreateEventContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateFunction([NotNull] MySqlParser.CreateFunctionContext context)
        {
            var funcName = (IdToken)Visit(context.fullId());
            var returnType = (TypeToken)Visit(context.dataType());

            var funcParams = new List<ParameterDeclaration>();
            var paramContext = context.functionParameter();
            for (int i = 0; i < paramContext.Length; i++)
            {
                funcParams.Add((ParameterDeclaration)Visit(paramContext[i]));
            }
            var body = (BlockStatement)Visit(context.routineBody());

            return new MethodDeclaration(funcName, funcParams, body, context.GetTextSpan())
            {
                ReturnType = returnType
            };
        }

        public Ust VisitCreateIndex([NotNull] MySqlParser.CreateIndexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateLogfileGroup([NotNull] MySqlParser.CreateLogfileGroupContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateProcedure([NotNull] MySqlParser.CreateProcedureContext context)
        {
            var procName = (IdToken)Visit(context.fullId());
            var procParams = new List<ParameterDeclaration>();
            var paramContext = context.procedureParameter();
            for (int i = 0; i < paramContext.Length; i++)
            {
                procParams.Add((ParameterDeclaration)Visit(paramContext[i]));
            }
            var body = (BlockStatement)Visit(context.routineBody());

            return new MethodDeclaration(procName, procParams, body, context.GetTextSpan());
        }

        public Ust VisitCreateServer([NotNull] MySqlParser.CreateServerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateTable([NotNull] MySqlParser.CreateTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateTablespaceInnodb([NotNull] MySqlParser.CreateTablespaceInnodbContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateTablespaceNdb([NotNull] MySqlParser.CreateTablespaceNdbContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateTrigger([NotNull] MySqlParser.CreateTriggerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateUdfunction([NotNull] MySqlParser.CreateUdfunctionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateUser([NotNull] MySqlParser.CreateUserContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateUserMysqlV56([NotNull] MySqlParser.CreateUserMysqlV56Context context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateUserMysqlV57([NotNull] MySqlParser.CreateUserMysqlV57Context context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCreateView([NotNull] MySqlParser.CreateViewContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCurrentSchemaPriviLevel([NotNull] MySqlParser.CurrentSchemaPriviLevelContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCurrentTimestamp([NotNull] MySqlParser.CurrentTimestampContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCursorStatement([NotNull] MySqlParser.CursorStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDataType([NotNull] MySqlParser.DataTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDataTypeBase([NotNull] MySqlParser.DataTypeBaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDataTypeFunctionCall([NotNull] MySqlParser.DataTypeFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDdlStatement([NotNull] MySqlParser.DdlStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDeallocatePrepare([NotNull] MySqlParser.DeallocatePrepareContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDecimalLiteral([NotNull] MySqlParser.DecimalLiteralContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDecimalMasterOption([NotNull] MySqlParser.DecimalMasterOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDeclareCondition([NotNull] MySqlParser.DeclareConditionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDeclareCursor([NotNull] MySqlParser.DeclareCursorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDeclareHandler([NotNull] MySqlParser.DeclareHandlerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDeclareVariable([NotNull] MySqlParser.DeclareVariableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefaultAuthConnectionOption([NotNull] MySqlParser.DefaultAuthConnectionOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefaultColumnConstraint([NotNull] MySqlParser.DefaultColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefaultValue([NotNull] MySqlParser.DefaultValueContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefiniteFullTablePrivLevel([NotNull] MySqlParser.DefiniteFullTablePrivLevelContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefiniteSchemaPrivLevel([NotNull] MySqlParser.DefiniteSchemaPrivLevelContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefiniteTablePrivLevel([NotNull] MySqlParser.DefiniteTablePrivLevelContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDeleteStatement([NotNull] MySqlParser.DeleteStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDescribeConnection([NotNull] MySqlParser.DescribeConnectionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDescribeObjectClause([NotNull] MySqlParser.DescribeObjectClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDescribeStatements([NotNull] MySqlParser.DescribeStatementsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDetailRevoke([NotNull] MySqlParser.DetailRevokeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDimensionDataType([NotNull] MySqlParser.DimensionDataTypeContext context)
        {
            return new TypeToken(context.typeName.Text, context.GetTextSpan());
        }

        public Ust VisitDmlStatement([NotNull] MySqlParser.DmlStatementContext context)
        {
            var exprList = ExtractMultiChild((MultichildExpression)VisitChildren(context), new List<Expression>());
            return new ExpressionStatement(new SqlQuery(exprList[0], exprList.GetRange(1, exprList.Count - 1), context.GetTextSpan()));
        }

        public Ust VisitDoDbReplication([NotNull] MySqlParser.DoDbReplicationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDoStatement([NotNull] MySqlParser.DoStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDoTableReplication([NotNull] MySqlParser.DoTableReplicationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDottedId([NotNull] MySqlParser.DottedIdContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropDatabase([NotNull] MySqlParser.DropDatabaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropEvent([NotNull] MySqlParser.DropEventContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropFunction([NotNull] MySqlParser.DropFunctionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropIndex([NotNull] MySqlParser.DropIndexContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropLogfileGroup([NotNull] MySqlParser.DropLogfileGroupContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropProcedure([NotNull] MySqlParser.DropProcedureContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropServer([NotNull] MySqlParser.DropServerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropTable([NotNull] MySqlParser.DropTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropTablespace([NotNull] MySqlParser.DropTablespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropTrigger([NotNull] MySqlParser.DropTriggerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropUser([NotNull] MySqlParser.DropUserContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDropView([NotNull] MySqlParser.DropViewContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitElifAlternative([NotNull] MySqlParser.ElifAlternativeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEmptyStatement([NotNull] MySqlParser.EmptyStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEnableType([NotNull] MySqlParser.EnableTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEngineName([NotNull] MySqlParser.EngineNameContext context)
        {
            return VisitChildren(context);
        }        

        public Ust VisitExecuteStatement([NotNull] MySqlParser.ExecuteStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExistsExpessionAtom([NotNull] MySqlParser.ExistsExpessionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpression([NotNull] MySqlParser.ExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpressionAtom([NotNull] MySqlParser.ExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpressionAtomPredicate([NotNull] MySqlParser.ExpressionAtomPredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpressionOrDefault([NotNull] MySqlParser.ExpressionOrDefaultContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpressions([NotNull] MySqlParser.ExpressionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpressionsWithDefaults([NotNull] MySqlParser.ExpressionsWithDefaultsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExtractFunctionCall([NotNull] MySqlParser.ExtractFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFetchCursor([NotNull] MySqlParser.FetchCursorContext context)
        {
            var result = new SqlQuery
            {
                QueryCommand = new IdToken(context.FETCH().GetText(), context.FETCH().GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };

            var queryElements = new List<Expression>
            {
                ConvertToInOutArgument(context.uid())
            };

            for (int i = 2; i < context.ChildCount; i++)
            {
                queryElements.Add((Expression)Visit(context.GetChild(i)));
            }

            result.QueryElements = queryElements;
            return result;
        }

        public Ust VisitFileSizeLiteral([NotNull] MySqlParser.FileSizeLiteralContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFlushOption([NotNull] MySqlParser.FlushOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFlushStatement([NotNull] MySqlParser.FlushStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFlushTableOption([NotNull] MySqlParser.FlushTableOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitForeignKeyTableConstraint([NotNull] MySqlParser.ForeignKeyTableConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFormatColumnConstraint([NotNull] MySqlParser.FormatColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFromClause([NotNull] MySqlParser.FromClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFullColumnName([NotNull] MySqlParser.FullColumnNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFullColumnNameExpressionAtom([NotNull] MySqlParser.FullColumnNameExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFullDescribeStatement([NotNull] MySqlParser.FullDescribeStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFullId([NotNull] MySqlParser.FullIdContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFunctionArg([NotNull] MySqlParser.FunctionArgContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFunctionArgs([NotNull] MySqlParser.FunctionArgsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFunctionCall([NotNull] MySqlParser.FunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFunctionCallExpressionAtom([NotNull] MySqlParser.FunctionCallExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFunctionNameBase([NotNull] MySqlParser.FunctionNameBaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFunctionParameter([NotNull] MySqlParser.FunctionParameterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGetFormatFunctionCall([NotNull] MySqlParser.GetFormatFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGlobalPrivLevel([NotNull] MySqlParser.GlobalPrivLevelContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGrantProxy([NotNull] MySqlParser.GrantProxyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGrantStatement([NotNull] MySqlParser.GrantStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGroupByItem([NotNull] MySqlParser.GroupByItemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGtidsUntilOption([NotNull] MySqlParser.GtidsUntilOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGtuidSet([NotNull] MySqlParser.GtuidSetContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerCloseStatement([NotNull] MySqlParser.HandlerCloseStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerConditionCode([NotNull] MySqlParser.HandlerConditionCodeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerConditionException([NotNull] MySqlParser.HandlerConditionExceptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerConditionName([NotNull] MySqlParser.HandlerConditionNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerConditionNotfound([NotNull] MySqlParser.HandlerConditionNotfoundContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerConditionState([NotNull] MySqlParser.HandlerConditionStateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerConditionValue([NotNull] MySqlParser.HandlerConditionValueContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerConditionWarning([NotNull] MySqlParser.HandlerConditionWarningContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerOpenStatement([NotNull] MySqlParser.HandlerOpenStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerReadIndexStatement([NotNull] MySqlParser.HandlerReadIndexStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerReadStatement([NotNull] MySqlParser.HandlerReadStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHandlerStatement([NotNull] MySqlParser.HandlerStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHashAuthOption([NotNull] MySqlParser.HashAuthOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHelpStatement([NotNull] MySqlParser.HelpStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitHexadecimalLiteral([NotNull] MySqlParser.HexadecimalLiteralContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIfExists([NotNull] MySqlParser.IfExistsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIfNotExists([NotNull] MySqlParser.IfNotExistsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIfStatement([NotNull] MySqlParser.IfStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIgnoreDbReplication([NotNull] MySqlParser.IgnoreDbReplicationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIgnoreTableReplication([NotNull] MySqlParser.IgnoreTableReplicationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndexColumnDefinition([NotNull] MySqlParser.IndexColumnDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndexColumnName([NotNull] MySqlParser.IndexColumnNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndexColumnNames([NotNull] MySqlParser.IndexColumnNamesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndexDeclaration([NotNull] MySqlParser.IndexDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndexHint([NotNull] MySqlParser.IndexHintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndexHintType([NotNull] MySqlParser.IndexHintTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndexOption([NotNull] MySqlParser.IndexOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIndexType([NotNull] MySqlParser.IndexTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInnerJoin([NotNull] MySqlParser.InnerJoinContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInPredicate([NotNull] MySqlParser.InPredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInsertStatement([NotNull] MySqlParser.InsertStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInsertStatementValue([NotNull] MySqlParser.InsertStatementValueContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInstallPlugin([NotNull] MySqlParser.InstallPluginContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIntervalExpr([NotNull] MySqlParser.IntervalExprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIntervalExpressionAtom([NotNull] MySqlParser.IntervalExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIntervalSchedule([NotNull] MySqlParser.IntervalScheduleContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIntervalType([NotNull] MySqlParser.IntervalTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIntervalTypeBase([NotNull] MySqlParser.IntervalTypeBaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIsExpression([NotNull] MySqlParser.IsExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIsNullPredicate([NotNull] MySqlParser.IsNullPredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIterateStatement([NotNull] MySqlParser.IterateStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitJoinPart([NotNull] MySqlParser.JoinPartContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitKeywordsCanBeId([NotNull] MySqlParser.KeywordsCanBeIdContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitKillStatement([NotNull] MySqlParser.KillStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLeaveStatement([NotNull] MySqlParser.LeaveStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLengthOneDimension([NotNull] MySqlParser.LengthOneDimensionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLengthTwoDimension([NotNull] MySqlParser.LengthTwoDimensionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLengthTwoOptionalDimension([NotNull] MySqlParser.LengthTwoOptionalDimensionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLevelInWeightListElement([NotNull] MySqlParser.LevelInWeightListElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLevelsInWeightString([NotNull] MySqlParser.LevelsInWeightStringContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLevelWeightList([NotNull] MySqlParser.LevelWeightListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLevelWeightRange([NotNull] MySqlParser.LevelWeightRangeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLikePredicate([NotNull] MySqlParser.LikePredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLimitClause([NotNull] MySqlParser.LimitClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLoadDataStatement([NotNull] MySqlParser.LoadDataStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLoadedTableIndexes([NotNull] MySqlParser.LoadedTableIndexesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLoadIndexIntoCache([NotNull] MySqlParser.LoadIndexIntoCacheContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLoadXmlStatement([NotNull] MySqlParser.LoadXmlStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLockAction([NotNull] MySqlParser.LockActionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLockClause([NotNull] MySqlParser.LockClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLockTableElement([NotNull] MySqlParser.LockTableElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLockTables([NotNull] MySqlParser.LockTablesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLogicalExpression([NotNull] MySqlParser.LogicalExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLogicalOperator([NotNull] MySqlParser.LogicalOperatorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLoopStatement([NotNull] MySqlParser.LoopStatementContext context)
        {
            var loopBlock = new BlockStatement()
            {
                TextSpan = context.GetTextSpan()
            };

            var statementsContext = context.procedureSqlStatement();
            for (int i = 0; i < statementsContext.Length; i++)
            {
                loopBlock.Statements.Add((Statement)Visit(statementsContext[i]));
            }

            return new WhileStatement
            {
                Embedded = loopBlock,
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitMasterBoolOption([NotNull] MySqlParser.MasterBoolOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMasterDecimalOption([NotNull] MySqlParser.MasterDecimalOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMasterLogUntilOption([NotNull] MySqlParser.MasterLogUntilOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMasterOption([NotNull] MySqlParser.MasterOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMasterRealOption([NotNull] MySqlParser.MasterRealOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMasterStringOption([NotNull] MySqlParser.MasterStringOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMasterUidListOption([NotNull] MySqlParser.MasterUidListOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMathExpressionAtom([NotNull] MySqlParser.MathExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMathOperator([NotNull] MySqlParser.MathOperatorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMultipleDeleteStatement([NotNull] MySqlParser.MultipleDeleteStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMultipleUpdateStatement([NotNull] MySqlParser.MultipleUpdateStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMysqlVariable([NotNull] MySqlParser.MysqlVariableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMysqlVariableExpressionAtom([NotNull] MySqlParser.MysqlVariableExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNaturalJoin([NotNull] MySqlParser.NaturalJoinContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNestedExpressionAtom([NotNull] MySqlParser.NestedExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNestedRowExpressionAtom([NotNull] MySqlParser.NestedRowExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNotExpression([NotNull] MySqlParser.NotExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNullColumnConstraint([NotNull] MySqlParser.NullColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNullNotnull([NotNull] MySqlParser.NullNotnullContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOpenCursor([NotNull] MySqlParser.OpenCursorContext context)
        {
            var cursorArg = ConvertToInOutArgument(context.uid());
            var funcName = new IdToken(context.OPEN().GetText(), context.OPEN().GetTextSpan());
            return new InvocationExpression(funcName, new ArgsUst(cursorArg), context.GetTextSpan());
        }

        public Ust VisitOptimizeTable([NotNull] MySqlParser.OptimizeTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOrderByClause([NotNull] MySqlParser.OrderByClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOrderByExpression([NotNull] MySqlParser.OrderByExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOuterJoin([NotNull] MySqlParser.OuterJoinContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOwnerStatement([NotNull] MySqlParser.OwnerStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitParenthesisSelect([NotNull] MySqlParser.ParenthesisSelectContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionComparision([NotNull] MySqlParser.PartitionComparisionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionDefinerAtom([NotNull] MySqlParser.PartitionDefinerAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionDefinerVector([NotNull] MySqlParser.PartitionDefinerVectorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionDefinition([NotNull] MySqlParser.PartitionDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionDefinitions([NotNull] MySqlParser.PartitionDefinitionsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionFunctionDefinition([NotNull] MySqlParser.PartitionFunctionDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionFunctionHash([NotNull] MySqlParser.PartitionFunctionHashContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionFunctionKey([NotNull] MySqlParser.PartitionFunctionKeyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionFunctionList([NotNull] MySqlParser.PartitionFunctionListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionFunctionRange([NotNull] MySqlParser.PartitionFunctionRangeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionListAtom([NotNull] MySqlParser.PartitionListAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionListVector([NotNull] MySqlParser.PartitionListVectorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionOption([NotNull] MySqlParser.PartitionOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionOptionComment([NotNull] MySqlParser.PartitionOptionCommentContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionOptionDataDirectory([NotNull] MySqlParser.PartitionOptionDataDirectoryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionOptionEngine([NotNull] MySqlParser.PartitionOptionEngineContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionOptionIndexDirectory([NotNull] MySqlParser.PartitionOptionIndexDirectoryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionOptionMaxRows([NotNull] MySqlParser.PartitionOptionMaxRowsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionOptionMinRows([NotNull] MySqlParser.PartitionOptionMinRowsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionOptionNodeGroup([NotNull] MySqlParser.PartitionOptionNodeGroupContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionOptionTablespace([NotNull] MySqlParser.PartitionOptionTablespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPartitionSimple([NotNull] MySqlParser.PartitionSimpleContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPasswordAuthOption([NotNull] MySqlParser.PasswordAuthOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPasswordConnectionOption([NotNull] MySqlParser.PasswordConnectionOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPasswordFunctionCall([NotNull] MySqlParser.PasswordFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPasswordFunctionClause([NotNull] MySqlParser.PasswordFunctionClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPluginDirConnectionOption([NotNull] MySqlParser.PluginDirConnectionOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPositionFunctionCall([NotNull] MySqlParser.PositionFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPreciseSchedule([NotNull] MySqlParser.PreciseScheduleContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPredicate([NotNull] MySqlParser.PredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPredicateExpression([NotNull] MySqlParser.PredicateExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPreparedStatement([NotNull] MySqlParser.PreparedStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPrepareStatement([NotNull] MySqlParser.PrepareStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPrimaryKeyColumnConstraint([NotNull] MySqlParser.PrimaryKeyColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPrimaryKeyTableConstraint([NotNull] MySqlParser.PrimaryKeyTableConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPrivelegeClause([NotNull] MySqlParser.PrivelegeClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPrivilege([NotNull] MySqlParser.PrivilegeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPrivilegeLevel([NotNull] MySqlParser.PrivilegeLevelContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPrivilegesBase([NotNull] MySqlParser.PrivilegesBaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitProcedureParameter([NotNull] MySqlParser.ProcedureParameterContext context)
        {
            Enum.TryParse(context.direction.Text, out InOutModifier modifierType);
            var modifier = new InOutModifierLiteral(modifierType, context.direction.GetTextSpan());
            var id = (IdToken)Visit(context.uid());
            var type = (TypeToken)Visit(context.dataType());
            return new ParameterDeclaration(modifier, type, id, context.GetTextSpan());
        }

        public Ust VisitProcedureSqlStatement([NotNull] MySqlParser.ProcedureSqlStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPurgeBinaryLogs([NotNull] MySqlParser.PurgeBinaryLogsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitQueryCreateTable([NotNull] MySqlParser.QueryCreateTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitQueryExpression([NotNull] MySqlParser.QueryExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitQueryExpressionNointo([NotNull] MySqlParser.QueryExpressionNointoContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitQuerySpecification([NotNull] MySqlParser.QuerySpecificationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitQuerySpecificationNointo([NotNull] MySqlParser.QuerySpecificationNointoContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReferenceAction([NotNull] MySqlParser.ReferenceActionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReferenceColumnConstraint([NotNull] MySqlParser.ReferenceColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReferenceControlType([NotNull] MySqlParser.ReferenceControlTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReferenceDefinition([NotNull] MySqlParser.ReferenceDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRegexpPredicate([NotNull] MySqlParser.RegexpPredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRelayLogUntilOption([NotNull] MySqlParser.RelayLogUntilOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReleaseStatement([NotNull] MySqlParser.ReleaseStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRenameTable([NotNull] MySqlParser.RenameTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRenameTableClause([NotNull] MySqlParser.RenameTableClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRenameUser([NotNull] MySqlParser.RenameUserContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRenameUserClause([NotNull] MySqlParser.RenameUserClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRepairTable([NotNull] MySqlParser.RepairTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRepeatStatement([NotNull] MySqlParser.RepeatStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReplaceStatement([NotNull] MySqlParser.ReplaceStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReplicationFilter([NotNull] MySqlParser.ReplicationFilterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReplicationStatement([NotNull] MySqlParser.ReplicationStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitResetMaster([NotNull] MySqlParser.ResetMasterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitResetSlave([NotNull] MySqlParser.ResetSlaveContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitResetStatement([NotNull] MySqlParser.ResetStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReturnStatement([NotNull] MySqlParser.ReturnStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRevokeProxy([NotNull] MySqlParser.RevokeProxyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRevokeStatement([NotNull] MySqlParser.RevokeStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRewriteDbReplication([NotNull] MySqlParser.RewriteDbReplicationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRollbackStatement([NotNull] MySqlParser.RollbackStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRollbackWork([NotNull] MySqlParser.RollbackWorkContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRoutineBehavior([NotNull] MySqlParser.RoutineBehaviorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRoutineBody([NotNull] MySqlParser.RoutineBodyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRoutineComment([NotNull] MySqlParser.RoutineCommentContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRoutineData([NotNull] MySqlParser.RoutineDataContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRoutineLanguage([NotNull] MySqlParser.RoutineLanguageContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRoutineOption([NotNull] MySqlParser.RoutineOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRoutineSecurity([NotNull] MySqlParser.RoutineSecurityContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSavepointStatement([NotNull] MySqlParser.SavepointStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitScalarFunctionCall([NotNull] MySqlParser.ScalarFunctionCallContext context)
        {
            var funcName = (Expression)Visit(context.scalarFunctionName());
            var argList = context.functionArgs();
            var funcArgs = new List<Expression>();
            if (argList != null)
            {
                var visitedArgs = (Expression)VisitChildren(context.functionArgs());
                if (visitedArgs is MultichildExpression multichild)
                {
                    funcArgs = ExtractMultiChild(multichild, funcArgs);
                }
                else
                {
                    funcArgs.Add(visitedArgs);
                }
            }

            return new InvocationExpression(funcName, new ArgsUst(funcArgs), context.GetTextSpan());
        }

        public Ust VisitScalarFunctionName([NotNull] MySqlParser.ScalarFunctionNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitScheduleExpression([NotNull] MySqlParser.ScheduleExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectColumnElement([NotNull] MySqlParser.SelectColumnElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectElement([NotNull] MySqlParser.SelectElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectElements([NotNull] MySqlParser.SelectElementsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectExpressionElement([NotNull] MySqlParser.SelectExpressionElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectFieldsInto([NotNull] MySqlParser.SelectFieldsIntoContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectFunctionElement([NotNull] MySqlParser.SelectFunctionElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectIntoDumpFile([NotNull] MySqlParser.SelectIntoDumpFileContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectIntoExpression([NotNull] MySqlParser.SelectIntoExpressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectIntoTextFile([NotNull] MySqlParser.SelectIntoTextFileContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectIntoVariables([NotNull] MySqlParser.SelectIntoVariablesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectLinesInto([NotNull] MySqlParser.SelectLinesIntoContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectSpec([NotNull] MySqlParser.SelectSpecContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectStarElement([NotNull] MySqlParser.SelectStarElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSelectStatement([NotNull] MySqlParser.SelectStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitServerOption([NotNull] MySqlParser.ServerOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSetAutocommit([NotNull] MySqlParser.SetAutocommitContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSetAutocommitStatement([NotNull] MySqlParser.SetAutocommitStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSetCharset([NotNull] MySqlParser.SetCharsetContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSetNames([NotNull] MySqlParser.SetNamesContext context)
        {
            return new AssignmentExpression()
            {
                Left = new IdToken(context.NAMES().GetText(), context.NAMES().GetTextSpan()),
                Right = new StringLiteral(context.charsetName().GetText(), context.charsetName().GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitSetPassword([NotNull] MySqlParser.SetPasswordContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSetPasswordStatement([NotNull] MySqlParser.SetPasswordStatementContext context)
        {
            return new AssignmentExpression()
            {
                Left = new IdToken(context.PASSWORD().GetText(), context.PASSWORD().GetTextSpan()),
                Right = (Expression)Visit(context.passwordFunctionClause()),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitSetStatement([NotNull] MySqlParser.SetStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSetTransaction([NotNull] MySqlParser.SetTransactionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSetTransactionStatement([NotNull] MySqlParser.SetTransactionStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSetVariable([NotNull] MySqlParser.SetVariableContext context)
        {
            var variablesContext = context.variableClause();
            var expressionsContext = context.expression();
            List<AssignmentExpression> assignments = new List<AssignmentExpression>(variablesContext.Length);

            for (int i = 0; i < variablesContext.Length; i++)
            {
                assignments.Add(new AssignmentExpression
                {
                    Right = (Expression)Visit(variablesContext[i]),
                    Left = (Expression)Visit(expressionsContext[i]),
                    TextSpan = context.GetTextSpan()
                });
            }

            return new VariableDeclarationExpression
            {
                Variables = assignments,
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitShortRevoke([NotNull] MySqlParser.ShortRevokeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowColumns([NotNull] MySqlParser.ShowColumnsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowCommonEntity([NotNull] MySqlParser.ShowCommonEntityContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowCountErrors([NotNull] MySqlParser.ShowCountErrorsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowCreateDb([NotNull] MySqlParser.ShowCreateDbContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowCreateFullIdObject([NotNull] MySqlParser.ShowCreateFullIdObjectContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowCreateUser([NotNull] MySqlParser.ShowCreateUserContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowEngine([NotNull] MySqlParser.ShowEngineContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowErrors([NotNull] MySqlParser.ShowErrorsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowFilter([NotNull] MySqlParser.ShowFilterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowGlobalInfo([NotNull] MySqlParser.ShowGlobalInfoContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowGlobalInfoClause([NotNull] MySqlParser.ShowGlobalInfoClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowGrants([NotNull] MySqlParser.ShowGrantsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowIndexes([NotNull] MySqlParser.ShowIndexesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowLogEvents([NotNull] MySqlParser.ShowLogEventsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowMasterLogs([NotNull] MySqlParser.ShowMasterLogsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowObjectFilter([NotNull] MySqlParser.ShowObjectFilterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowOpenTables([NotNull] MySqlParser.ShowOpenTablesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowProfile([NotNull] MySqlParser.ShowProfileContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowProfileType([NotNull] MySqlParser.ShowProfileTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowRoutine([NotNull] MySqlParser.ShowRoutineContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowSchemaEntity([NotNull] MySqlParser.ShowSchemaEntityContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowSchemaFilter([NotNull] MySqlParser.ShowSchemaFilterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowSlaveStatus([NotNull] MySqlParser.ShowSlaveStatusContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShowStatement([NotNull] MySqlParser.ShowStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShutdownStatement([NotNull] MySqlParser.ShutdownStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimpleAuthOption([NotNull] MySqlParser.SimpleAuthOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimpleDataType([NotNull] MySqlParser.SimpleDataTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimpleDescribeStatement([NotNull] MySqlParser.SimpleDescribeStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimpleFlushOption([NotNull] MySqlParser.SimpleFlushOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimpleFunctionCall([NotNull] MySqlParser.SimpleFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimpleId([NotNull] MySqlParser.SimpleIdContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimpleIndexDeclaration([NotNull] MySqlParser.SimpleIndexDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimpleSelect([NotNull] MySqlParser.SimpleSelectContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimpleStrings([NotNull] MySqlParser.SimpleStringsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSingleDeleteStatement([NotNull] MySqlParser.SingleDeleteStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSingleUpdateStatement([NotNull] MySqlParser.SingleUpdateStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSoundsLikePredicate([NotNull] MySqlParser.SoundsLikePredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSpatialDataType([NotNull] MySqlParser.SpatialDataTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSpecialIndexDeclaration([NotNull] MySqlParser.SpecialIndexDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSpecificFunction([NotNull] MySqlParser.SpecificFunctionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSpecificFunctionCall([NotNull] MySqlParser.SpecificFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSqlGapsUntilOption([NotNull] MySqlParser.SqlGapsUntilOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSqlStatement([NotNull] MySqlParser.SqlStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSqlStatements([NotNull] MySqlParser.SqlStatementsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStartGroupReplication([NotNull] MySqlParser.StartGroupReplicationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStartSlave([NotNull] MySqlParser.StartSlaveContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStartTransaction([NotNull] MySqlParser.StartTransactionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStopGroupReplication([NotNull] MySqlParser.StopGroupReplicationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStopSlave([NotNull] MySqlParser.StopSlaveContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStorageColumnConstraint([NotNull] MySqlParser.StorageColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStraightJoin([NotNull] MySqlParser.StraightJoinContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStringAuthOption([NotNull] MySqlParser.StringAuthOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStringDataType([NotNull] MySqlParser.StringDataTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStringLiteral([NotNull] MySqlParser.StringLiteralContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStringMasterOption([NotNull] MySqlParser.StringMasterOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubpartitionDefinition([NotNull] MySqlParser.SubpartitionDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubpartitionFunctionDefinition([NotNull] MySqlParser.SubpartitionFunctionDefinitionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubPartitionFunctionHash([NotNull] MySqlParser.SubPartitionFunctionHashContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubPartitionFunctionKey([NotNull] MySqlParser.SubPartitionFunctionKeyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubqueryComparasionPredicate([NotNull] MySqlParser.SubqueryComparasionPredicateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubqueryExpessionAtom([NotNull] MySqlParser.SubqueryExpessionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubqueryTableItem([NotNull] MySqlParser.SubqueryTableItemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubstrFunctionCall([NotNull] MySqlParser.SubstrFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableConstraint([NotNull] MySqlParser.TableConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableFlushOption([NotNull] MySqlParser.TableFlushOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableIndexes([NotNull] MySqlParser.TableIndexesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableName([NotNull] MySqlParser.TableNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOption([NotNull] MySqlParser.TableOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionAutoIncrement([NotNull] MySqlParser.TableOptionAutoIncrementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionAverage([NotNull] MySqlParser.TableOptionAverageContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionCharset([NotNull] MySqlParser.TableOptionCharsetContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionChecksum([NotNull] MySqlParser.TableOptionChecksumContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionCollate([NotNull] MySqlParser.TableOptionCollateContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionComment([NotNull] MySqlParser.TableOptionCommentContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionCompression([NotNull] MySqlParser.TableOptionCompressionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionConnection([NotNull] MySqlParser.TableOptionConnectionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionDataDirectory([NotNull] MySqlParser.TableOptionDataDirectoryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionDelay([NotNull] MySqlParser.TableOptionDelayContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionEncryption([NotNull] MySqlParser.TableOptionEncryptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionEngine([NotNull] MySqlParser.TableOptionEngineContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionIndexDirectory([NotNull] MySqlParser.TableOptionIndexDirectoryContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionInsertMethod([NotNull] MySqlParser.TableOptionInsertMethodContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionKeyBlockSize([NotNull] MySqlParser.TableOptionKeyBlockSizeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionMaxRows([NotNull] MySqlParser.TableOptionMaxRowsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionMinRows([NotNull] MySqlParser.TableOptionMinRowsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionPackKeys([NotNull] MySqlParser.TableOptionPackKeysContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionPassword([NotNull] MySqlParser.TableOptionPasswordContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionPersistent([NotNull] MySqlParser.TableOptionPersistentContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionRecalculation([NotNull] MySqlParser.TableOptionRecalculationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionRowFormat([NotNull] MySqlParser.TableOptionRowFormatContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionSamplePage([NotNull] MySqlParser.TableOptionSamplePageContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionTablespace([NotNull] MySqlParser.TableOptionTablespaceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableOptionUnion([NotNull] MySqlParser.TableOptionUnionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTablePair([NotNull] MySqlParser.TablePairContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTables([NotNull] MySqlParser.TablesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableSource([NotNull] MySqlParser.TableSourceContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableSourceBase([NotNull] MySqlParser.TableSourceBaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableSourceItem([NotNull] MySqlParser.TableSourceItemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableSourceNested([NotNull] MySqlParser.TableSourceNestedContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableSources([NotNull] MySqlParser.TableSourcesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTableSourcesItem([NotNull] MySqlParser.TableSourcesItemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTablespaceStorage([NotNull] MySqlParser.TablespaceStorageContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitThreadType([NotNull] MySqlParser.ThreadTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTimestampValue([NotNull] MySqlParser.TimestampValueContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTlsOption([NotNull] MySqlParser.TlsOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTransactionLevel([NotNull] MySqlParser.TransactionLevelContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTransactionLevelBase([NotNull] MySqlParser.TransactionLevelBaseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTransactionMode([NotNull] MySqlParser.TransactionModeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTransactionOption([NotNull] MySqlParser.TransactionOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTransactionStatement([NotNull] MySqlParser.TransactionStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTrimFunctionCall([NotNull] MySqlParser.TrimFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTruncateTable([NotNull] MySqlParser.TruncateTableContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUdfFunctionCall([NotNull] MySqlParser.UdfFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUid([NotNull] MySqlParser.UidContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUidList([NotNull] MySqlParser.UidListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUnaryExpressionAtom([NotNull] MySqlParser.UnaryExpressionAtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUnaryOperator([NotNull] MySqlParser.UnaryOperatorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUninstallPlugin([NotNull] MySqlParser.UninstallPluginContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUnionParenthesis([NotNull] MySqlParser.UnionParenthesisContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUnionParenthesisSelect([NotNull] MySqlParser.UnionParenthesisSelectContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUnionSelect([NotNull] MySqlParser.UnionSelectContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUnionStatement([NotNull] MySqlParser.UnionStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUniqueKeyColumnConstraint([NotNull] MySqlParser.UniqueKeyColumnConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUniqueKeyTableConstraint([NotNull] MySqlParser.UniqueKeyTableConstraintContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUnlockTables([NotNull] MySqlParser.UnlockTablesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUntilOption([NotNull] MySqlParser.UntilOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUpdatedElement([NotNull] MySqlParser.UpdatedElementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUpdateStatement([NotNull] MySqlParser.UpdateStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUserAuthOption([NotNull] MySqlParser.UserAuthOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUserConnectionOption([NotNull] MySqlParser.UserConnectionOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUserLockOption([NotNull] MySqlParser.UserLockOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUserName([NotNull] MySqlParser.UserNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUserPasswordOption([NotNull] MySqlParser.UserPasswordOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUserResourceOption([NotNull] MySqlParser.UserResourceOptionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUserSpecification([NotNull] MySqlParser.UserSpecificationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUserVariables([NotNull] MySqlParser.UserVariablesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUseStatement([NotNull] MySqlParser.UseStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUtilityStatement([NotNull] MySqlParser.UtilityStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitUuidSet([NotNull] MySqlParser.UuidSetContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitValuesFunctionCall([NotNull] MySqlParser.ValuesFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVariableClause([NotNull] MySqlParser.VariableClauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWeightFunctionCall([NotNull] MySqlParser.WeightFunctionCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWhileStatement([NotNull] MySqlParser.WhileStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWildDoTableReplication([NotNull] MySqlParser.WildDoTableReplicationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWildIgnoreTableReplication([NotNull] MySqlParser.WildIgnoreTableReplicationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXaCommitWork([NotNull] MySqlParser.XaCommitWorkContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXaEndTransaction([NotNull] MySqlParser.XaEndTransactionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXaPrepareStatement([NotNull] MySqlParser.XaPrepareStatementContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXaRecoverWork([NotNull] MySqlParser.XaRecoverWorkContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXaRollbackWork([NotNull] MySqlParser.XaRollbackWorkContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXaStartTransaction([NotNull] MySqlParser.XaStartTransactionContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXid([NotNull] MySqlParser.XidContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXuidStringId([NotNull] MySqlParser.XuidStringIdContext context)
        {
            return VisitChildren(context);
        }
    }
}
