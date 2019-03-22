using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Python3ParseTreeUst
{
    public partial class Python3AntlrConverter : AntlrConverter, IPython3ParserVisitor<Ust>
    {
        public override Language Language => Language.Python3;

        public static Python3AntlrConverter Create() => new Python3AntlrConverter();


        public Ust VisitRoot(Python3Parser.RootContext context)
        {
            root.Node = VisitChildren(context);
            return root;
        }

        public Ust VisitSingle_input(Python3Parser.Single_inputContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFile_input(Python3Parser.File_inputContext context)
        {
            var block = new BlockStatement(context.GetTextSpan());
            foreach (var child in context.stmt())
            {
                block.Statements.Add(Visit(child).ToStatementIfRequired());
            }
            return block;
        }

        public Ust VisitEval_input(Python3Parser.Eval_inputContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDecorator(Python3Parser.DecoratorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDecorators(Python3Parser.DecoratorsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDecorated(Python3Parser.DecoratedContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFuncdef(Python3Parser.FuncdefContext context)
        {
            var result = new MethodDeclaration
            {
                Name = new IdToken(context.NAME().GetText(), context.NAME().GetTextSpan()),
                ReturnType = (TypeToken)Visit(context.func_annotation()),
                TextSpan = context.GetTextSpan()
            };

            if (context.ASYNC() != null)
            {
                result.Modifiers = new List<ModifierLiteral>(1)
                {
                    new ModifierLiteral(Modifier.Async, context.ASYNC().GetTextSpan())
                };
            }

            result.Body = (BlockStatement)Visit(context.suite());
            result.Parameters = ((Collection)Visit(context.parameters()))
                .Collection.Cast<ParameterDeclaration>().ToList();
            return result;
        }

        public Ust VisitFunc_annotation(Python3Parser.Func_annotationContext context)
        {
            return new TypeToken(context.test().GetText(), context.test().GetTextSpan());
        }

        public Ust VisitParameters(Python3Parser.ParametersContext context)
        {
            return context.ChildCount == 3
                ? Visit(context.GetChild(1))
                : new Collection();
        }

        public Ust VisitDef_parameter([NotNull] Python3Parser.Def_parameterContext context)
        {
            var parameter = (ParameterDeclaration)Visit(context.named_parameter());
            var defaultContext = context.test();
            if (defaultContext != null)
            {
                parameter.Initializer = (Expression)Visit(defaultContext);
            }
            return parameter;
        }

        public Ust VisitNamed_parameter([NotNull] Python3Parser.Named_parameterContext context)
        {
            var result = new ParameterDeclaration
            {
                Name = new IdToken(context.NAME().GetText(), context.NAME().GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
            var anotationContext = context.test();
            if (anotationContext != null)
            {
                result.Type = new TypeToken(anotationContext.GetText(), anotationContext.GetTextSpan());
            }
            return result;
        }

        public Ust VisitDef_parameters([NotNull] Python3Parser.Def_parametersContext context)
        {
            return new Collection(context.def_parameter().Select(Visit));
        }

        public Ust VisitVardef_parameters([NotNull] Python3Parser.Vardef_parametersContext context)
        {
            return new Collection(context.vardef_parameter().Select(Visit));
        }

        public Ust VisitVardef_parameter([NotNull] Python3Parser.Vardef_parameterContext context)
        {
            var parameter = new ParameterDeclaration
            {
                Name = new IdToken(context.NAME().GetText(), context.NAME().GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
            var defaultContext = context.test();
            if (defaultContext != null)
            {
                parameter.Initializer = (Expression)Visit(defaultContext);
            }
            return parameter;
        }

        public Ust VisitTypedargslist(Python3Parser.TypedargslistContext context)
        {
            return CreateParametersCollection(context);
        }

        public Ust VisitArgs([NotNull] Python3Parser.ArgsContext context)
        {
            var result = (ParameterDeclaration)Visit(context.named_parameter());
            result.Name.Id = "*" + result.Name.Id;
            return result;
        }

        public Ust VisitKwargs([NotNull] Python3Parser.KwargsContext context)
        {
            var result = (ParameterDeclaration)Visit(context.named_parameter());
            result.Name.Id = "**" + result.Name.Id;
            return result;
        }

        public Ust VisitVarargs([NotNull] Python3Parser.VarargsContext context)
        {
            return new ParameterDeclaration
            {
                Name = new IdToken("*" + context.NAME().GetText(), context.GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitVarkwargs([NotNull] Python3Parser.VarkwargsContext context)
        {
            return new ParameterDeclaration
            {
                Name = new IdToken("**" + context.NAME().GetText(), context.GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitVarargslist(Python3Parser.VarargslistContext context)
        {
            return CreateParametersCollection(context);
        }

        public Ust VisitVfpdef(Python3Parser.VfpdefContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStmt(Python3Parser.StmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimple_stmt(Python3Parser.Simple_stmtContext context)
        {
            var statement = context.children.FirstOrDefault(x => !x.GetText().Contains("\n"));
            return Visit(statement).ToStatementIfRequired();
        }

        public Ust VisitSmall_stmt(Python3Parser.Small_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpr_stmt(Python3Parser.Expr_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAssign([NotNull] Python3Parser.AssignContext context)
        {
            var leftContext = (ParserRuleContext)context.GetChild(0);
            var rightContext = (ParserRuleContext)context.GetChild(2);
            var textSpan = context.GetTextSpan();
            Expression left, right;
            if (leftContext.ChildCount == rightContext.ChildCount)
            {
                var assignments = new List<AssignmentExpression>(leftContext.ChildCount);
                for (int i = 0; i < leftContext.ChildCount; i++)
                {
                    var assignment = new AssignmentExpression
                    {
                        Left = (Expression)Visit(leftContext.GetChild(i)),
                        Right = (Expression)Visit(rightContext.GetChild(i))
                    };
                    assignment.TextSpan = assignment.Left.TextSpan.Union(assignment.Right.TextSpan);
                    assignments.Add(assignment);
                }
                return new VariableDeclarationExpression(null, assignments, textSpan);
            }
            else if (leftContext.ChildCount < rightContext.ChildCount)
            {
                left = (Expression)Visit(leftContext);
                right = new TupleCreateExpression()
                {
                    TextSpan = rightContext.GetTextSpan(),
                    Initializers = rightContext.children
                    .Where(x => x.GetText() != ",")
                    .Select(x => (Expression)Visit(x)).ToList()
                };
            }
            else
            {
                left = (Expression)Visit(leftContext);
                right = (Expression)Visit(rightContext);
            }
            return new AssignmentExpression(left, right, textSpan);
        }

        public Ust VisitAnnassign(Python3Parser.AnnassignContext context)
        {
            if (context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            var typeContext = context.test()[0];
            var result = new VariableDeclarationExpression
            {
                Type = new TypeToken(typeContext.GetText(), typeContext.GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
            var leftContexts = context.testlist_star_expr().children.Where(x => x.GetText() != ",");
            var assignments = new List<AssignmentExpression>(leftContexts.Count());
            var textSpan = context.GetTextSpan();
            foreach (var leftContext in leftContexts)
            {
                assignments.Add(
                    new AssignmentExpression
                    {
                        Left = Visit(leftContext).ToExpressionIfRequired(),
                        TextSpan = textSpan
                    });
            }
            if (context.yield_expr() != null)
            {
                var right = Visit(context.yield_expr()).ToExpressionIfRequired();
                result.Variables.AddRange(assignments.Select(x =>
                   {
                       x.Right = right;
                       return x;
                   }));
            }
            else
            {
                var rightContexts = context.testlist().children.Where(x => x.GetText() != ",");
                if (rightContexts.Count() == assignments.Count)
                {
                    result.Variables.AddRange(assignments.Select((assign, index) =>
                    {
                        assign.Right = Visit(rightContexts.ElementAt(index)).ToExpressionIfRequired();
                        return assign;
                    }));
                }
            }
            return result;
        }

        public Ust VisitTestlist_star_expr(Python3Parser.Testlist_star_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAugassign(Python3Parser.AugassignContext context)
        {
            if (context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            ParserRuleContext rightContext = null;
            if (context.yield_expr() != null)
            {
                rightContext = context.yield_expr();
            }
            else
            {
                rightContext = context.testlist();
            }
            return CreateBinaryOperatorExpression(context.testlist_star_expr(), context.op, rightContext);
        }

        public Ust VisitDel_stmt(Python3Parser.Del_stmtContext context)
        {
            return new UnaryOperatorExpression
            {
                Operator = new UnaryOperatorLiteral(UnaryOperator.Delete),
                Expression = Visit(context.exprlist()).ToExpressionIfRequired(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitPass_stmt(Python3Parser.Pass_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFlow_stmt(Python3Parser.Flow_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBreak_stmt(Python3Parser.Break_stmtContext context)
        {
            return new BreakStatement(context.GetTextSpan());
        }

        public Ust VisitContinue_stmt(Python3Parser.Continue_stmtContext context)
        {
            return new ContinueStatement(context.GetTextSpan());
        }

        public Ust VisitReturn_stmt(Python3Parser.Return_stmtContext context)
        {
            return new ReturnStatement(Visit(context.testlist()).ToExpressionIfRequired(), context.GetTextSpan());
        }

        public Ust VisitYield_stmt(Python3Parser.Yield_stmtContext context)
        {
            return new YieldExpression(Visit(context.yield_expr()).ToExpressionIfRequired(), context.GetTextSpan());
        }

        public Ust VisitRaise_stmt(Python3Parser.Raise_stmtContext context)
        {
            return new ThrowStatement(Visit(context.test().FirstOrDefault()).ToExpressionIfRequired(), context.GetTextSpan());
        }

        public Ust VisitImport_stmt(Python3Parser.Import_stmtContext context)
        {
            var importFrom = context.import_from();
            var importName = context.import_name();
            string name = string.Empty;
            TextSpan textSpan = default;
            if (importFrom != null)
            {
                name = importFrom.children.LastOrDefault()?.GetText();
                textSpan = importFrom.GetTextSpan();
            }
            else if (importName != null)
            {
                name = importName.children.LastOrDefault()?.GetText();
                textSpan = importName.GetTextSpan();
            }
            return new UsingDeclaration(new StringLiteral(name, textSpan), context.GetTextSpan());
        }

        public Ust VisitImport_name(Python3Parser.Import_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImport_from(Python3Parser.Import_fromContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImport_as_name(Python3Parser.Import_as_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDotted_as_name(Python3Parser.Dotted_as_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImport_as_names(Python3Parser.Import_as_namesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDotted_as_names(Python3Parser.Dotted_as_namesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDotted_name(Python3Parser.Dotted_nameContext context)
        {
            if (context.NAME() != null)
            {
                return new IdToken(context.NAME().GetText(), context.NAME().GetTextSpan());
            }

            return new MemberReferenceExpression
            {
                Target = (Expression)Visit(context.dotted_name()[0]),
                Name = (Expression)Visit(context.dotted_name()[1]),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitGlobal_stmt(Python3Parser.Global_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNonlocal_stmt(Python3Parser.Nonlocal_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAssert_stmt(Python3Parser.Assert_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCompound_stmt(Python3Parser.Compound_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAsync_stmt(Python3Parser.Async_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIf_stmt(Python3Parser.If_stmtContext context)
        {
            var condition = (Expression)Visit(context.test());
            List<IfElseStatement> ifElseStatements;
            Statement elseStatement = null;
            var result = new IfElseStatement
            {
                Condition = condition,
                TextSpan = context.GetTextSpan()
            };

            if (context.suite() != null)
            {
                result.TrueStatement = (Statement)Visit(context.suite());
                ifElseStatements = context.elif_clause()
                    .Select(statement => (IfElseStatement)Visit(statement))
                    .Where(statement => statement != null).ToList();
                if (context.else_clause() != null)
                {
                    elseStatement = (Statement)Visit(context.else_clause());
                }
                IfElseStatement s = result;
                foreach (var elseIfStatement in ifElseStatements)
                {
                    s.FalseStatement = elseIfStatement;
                    s = elseIfStatement;
                }
                s.FalseStatement = elseStatement;
            }
            return result;
        }

        public Ust VisitElif_clause([NotNull] Python3Parser.Elif_clauseContext context)
        {
            return new IfElseStatement
            {
                Condition = (Expression)Visit(context.test()),
                TrueStatement = (BlockStatement)Visit(context.suite()),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitWhile_stmt(Python3Parser.While_stmtContext context)
        {
            return new WhileStatement
            {
                Condition = Visit(context.test()).ToExpressionIfRequired(),
                Embedded = Visit(context.suite()).ToStatementIfRequired(),
                TextSpan = context.GetTextSpan()
            };
        }

        //TODO: Add specific for statement with few VarNames and InExpressions and else statement
        public Ust VisitFor_stmt(Python3Parser.For_stmtContext context)
        {
            return new ForeachStatement
            {
                VarName = new IdToken(context.exprlist().GetText(), context.exprlist().GetTextSpan()),
                InExpression = Visit(context.testlist()).ToExpressionIfRequired(),
                EmbeddedStatement = Visit(context.suite()).ToStatementIfRequired(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitTry_stmt(Python3Parser.Try_stmtContext context)
        {
            var result = new TryCatchStatement
            {
                TextSpan = context.GetTextSpan()
            };
            result.TryBlock = (BlockStatement)Visit(context.suite());
            if (context.except_clause().Length > 0)
            {
                result.CatchClauses
                    .AddRange(context.except_clause()
                        .Select(x => (CatchClause)Visit(x)));
            }
            if (context.else_clause() != null)
            {
                result.ElseBlock = (BlockStatement)Visit(context.else_clause());
            }
            if (context.finaly_clause() != null)
            {
                result.FinallyBlock = (BlockStatement)Visit(context.finaly_clause());
            }
            return result;
        }

        public Ust VisitExcept_clause(Python3Parser.Except_clauseContext context)
        {
            var result = new CatchClause
            {
                VarName = (IdToken)Visit(context.NAME()),
                Body = (BlockStatement)Visit(context.suite())
            };
            if (context.test() != null)
            {
                result.Type = new TypeToken(context.test().GetText(), context.test().GetTextSpan());
            }
            return result;
        }

        public Ust VisitElse_clause([NotNull] Python3Parser.Else_clauseContext context)
        {
            return Visit(context.suite());
        }

        public Ust VisitFinaly_clause([NotNull] Python3Parser.Finaly_clauseContext context)
        {
            return Visit(context.suite());
        }

        public Ust VisitWith_stmt(Python3Parser.With_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWith_item(Python3Parser.With_itemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSuite(Python3Parser.SuiteContext context)
        {
            var result = new BlockStatement
            {
                TextSpan = context.GetTextSpan()
            };
            if (context.simple_stmt() != null)
            {
                result.Statements.Add(Visit(context.simple_stmt()).ToStatementIfRequired());
            }
            else if (context.stmt().Length > 0)
            {
                result.Statements.AddRange(context.stmt().Select(x => Visit(x).ToStatementIfRequired()));
            }
            return result;
        }

        public Ust VisitTest(Python3Parser.TestContext context)
        {
            if (context.IF() != null)
            {
                var expressions = context.logical_test();
                return new ConditionalExpression
                {
                    Condition = (Expression)Visit(expressions[1]),
                    TrueExpression = Visit(expressions[0]).ToExpressionIfRequired(),
                    FalseExpression = Visit(context.test()).ToExpressionIfRequired(),
                    TextSpan = context.GetTextSpan()
                };
            }
            return VisitChildren(context);
        }

        public Ust VisitTest_nocond(Python3Parser.Test_nocondContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLambdef(Python3Parser.LambdefContext context)
        {
            return CreateLambdaMethod(context.test(), context.varargslist(), context.GetTextSpan());
        }

        public Ust VisitLambdef_nocond(Python3Parser.Lambdef_nocondContext context)
        {
            return CreateLambdaMethod(context.test_nocond(), context.varargslist(), context.GetTextSpan());
        }

        public Ust VisitLogical_test([NotNull] Python3Parser.Logical_testContext context)
        {
            if (context.NOT() != null)
            {
                return new UnaryOperatorExpression
                {
                    Operator = new UnaryOperatorLiteral(UnaryOperator.Not, context.NOT().GetTextSpan()),
                    Expression = (Expression)Visit(context.GetChild(1))
                };
            }
            if (context.op != null)
            {
                return CreateBinaryOperatorExpression(context.logical_test()[0], context.op, context.logical_test()[1]);
            }
            return VisitChildren(context);
        }

        public Ust VisitComparison(Python3Parser.ComparisonContext context)
        {
            if (context.op != null)
            {
                return CreateBinaryOperatorExpression(context.comparison()[0], context.op, context.comparison()[1]);
            }
            if (context.NOT() != null)
            {
                return new UnaryOperatorExpression
                {
                    Operator = new UnaryOperatorLiteral(UnaryOperator.Not, context.NOT().GetTextSpan()),
                    Expression = CreateBinaryOperatorExpression(context.comparison()[0], context.IN() ?? context.IS(), context.comparison()[1])
                };
            }
            return VisitChildren(context);
        }

        public Ust VisitStar_expr(Python3Parser.Star_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpr(Python3Parser.ExprContext context)
        {
            if (context.op != null)
            {
                return CreateBinaryOperatorExpression(context.expr()[0], context.op, context.expr()[0]);
            }
            return VisitChildren(context);
        }

        public Ust VisitAtom_expr(Python3Parser.Atom_exprContext context)
        {
            var target = (Expression)Visit(context.atom());
            var textSpan = context.GetTextSpan();

            if (context.trailer().Length > 0)
            {
                Expression resultExpression = null;

                foreach (var trailer in context.trailer())
                {
                    var visited = Visit(trailer);
                    if (visited is ArgsUst argsUst)
                    {
                        if (resultExpression == null)
                        {
                            resultExpression = trailer.ChildCount == 2
                                || trailer.GetChild(1) is Python3Parser.ArglistContext
                                ? CreateInvocationExpression(target, argsUst, textSpan)
                                : CreateIndexerExpression(target, argsUst, textSpan);
                            continue;
                        }

                        resultExpression = trailer.ChildCount == 2
                                || trailer.GetChild(1) is Python3Parser.ArglistContext
                                ? CreateInvocationExpression(resultExpression, argsUst, textSpan)
                                : CreateIndexerExpression(resultExpression, argsUst, textSpan);
                    }
                }
                return resultExpression;
            }


            return target;
        }

        public Ust VisitAtom(Python3Parser.AtomContext context)
        {
            if (context.ChildCount == 3)
            {
                if (context.GetChild(0).GetText() == "("
                   && context.GetChild(2).GetText() == ")")
                {
                    return Visit(context.GetChild(1));
                }
            }
            return VisitChildren(context);
        }

        public Ust VisitTestlist_comp(Python3Parser.Testlist_compContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTrailer(Python3Parser.TrailerContext context)
        {
            return context.ChildCount == 3
                ? Visit(context.GetChild(1))
                : new ArgsUst { TextSpan = context.GetTextSpan() };
        }

        public Ust VisitSubscriptlist(Python3Parser.SubscriptlistContext context)
        {
            return new ArgsUst
            {
                Collection = context.subscript().Select(x => (Expression)Visit(x)).ToList(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitSubscript(Python3Parser.SubscriptContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSliceop(Python3Parser.SliceopContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExprlist(Python3Parser.ExprlistContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTestlist(Python3Parser.TestlistContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDictorsetmaker(Python3Parser.DictorsetmakerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitClassdef(Python3Parser.ClassdefContext context)
        {
            var result = new TypeDeclaration
            {
                Name = new IdToken(context.NAME().GetText(), context.NAME().GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
            if (context.arglist() != null)
            {
                result.BaseTypes.AddRange(context.arglist().argument().Select(x =>
                    new TypeToken(x.GetText(), x.GetTextSpan())
                ));
            }

            var typeBodyContext = context.suite();
            if (typeBodyContext != null)
            {
                if (typeBodyContext.simple_stmt() != null)
                {
                    result.TypeMembers.Add(
                        Visit(typeBodyContext.simple_stmt())
                        .ToStatementIfRequired());
                }
                else
                {
                    result.TypeMembers.AddRange(typeBodyContext.stmt().Select(Visit));
                }
            }
            return result;
        }

        public Ust VisitArglist(Python3Parser.ArglistContext context)
        {
            return new ArgsUst
            {
                Collection = context.argument().Select(x => Visit(x).ToExpressionIfRequired()).ToList(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitArgument(Python3Parser.ArgumentContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComp_iter(Python3Parser.Comp_iterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComp_for(Python3Parser.Comp_forContext context)
        {
            return new ForeachStatement
            {
                VarName = new IdToken(context.exprlist().GetText(), context.exprlist().GetTextSpan()),
                InExpression = Visit(context.logical_test()).ToExpressionIfRequired(),
                EmbeddedStatement = Visit(context.comp_iter()).ToStatementIfRequired(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitComp_if(Python3Parser.Comp_ifContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEncoding_decl(Python3Parser.Encoding_declContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitYield_expr(Python3Parser.Yield_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitYield_arg(Python3Parser.Yield_argContext context)
        {
            return VisitChildren(context);
        }
    }
}