using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
using PythonParseTree;
using System;
using System.Collections.Generic;
using System.Linq;
using Attribute = PT.PM.Common.Nodes.Attribute;

namespace PT.PM.PythonParseTreeUst
{
    public partial class PythonAntlrConverter : AntlrConverter, IPythonParserVisitor<Ust>
    {
        public override Language Language => Language.Python;

        public static PythonAntlrConverter Create() => new PythonAntlrConverter();

        public Ust VisitRoot(PythonParser.RootContext context)
        {
            root.Node = context.ChildCount == 2
                ? Visit(context.GetChild(0))
                : new BlockStatement(context.GetTextSpan());
            return root;
        }

        public Ust VisitSingle_input(PythonParser.Single_inputContext context)
        {
            return new BlockStatement
            {
                TextSpan = context.GetTextSpan(),
                Statements = context.children.Select(x => Visit(x).ToStatementIfRequired()).ToList()
            };
        }

        public Ust VisitFile_input(PythonParser.File_inputContext context)
        {
            var block = new BlockStatement(context.GetTextSpan());
            foreach (var child in context.stmt())
            {
                block.Statements.Add(Visit(child).ToStatementIfRequired());
            }

            if (root.CurrentSourceFile?.Name.Length > 0)
            {
                var dotIndex = root.CurrentSourceFile.Name.LastIndexOf('.');
                return new NamespaceDeclaration(
                    new StringLiteral(root.CurrentSourceFile?.Name.Remove(dotIndex < 0 ? 0 : dotIndex))
                    {
                        EscapeCharsLength = 0
                    },
                    block,
                    context.GetTextSpan());
            }
            return block;
        }

        public Ust VisitEval_input(PythonParser.Eval_inputContext context)
        {
            return new BlockStatement
            {
                TextSpan = context.GetTextSpan(),
                Statements = context.testlist().children.Select(x => Visit(x).ToStatementIfRequired()).ToList()
            };
        }

        public Ust VisitDecorator(PythonParser.DecoratorContext context)
        {
            var target = Visit(context.dotted_name()).ToExpressionIfRequired();
            var textSpan = context.GetTextSpan();
            if (context.arglist() != null)
            {
                return new Attribute(
                    new InvocationExpression
                    {
                        Target = target,
                        Arguments = (ArgsUst)Visit(context.arglist()),
                        TextSpan = textSpan
                    },
                    textSpan);
            }
            return new Attribute(target, textSpan);
        }

        public Ust VisitFuncdef(PythonParser.FuncdefContext context)
        {
            var returnType = context.test() != null
                ? new TypeToken(context.test().GetText(), context.test().GetTextSpan())
                : null;

            var result = new MethodDeclaration
            {
                Name = new IdToken(context.name().GetText(), context.name().GetTextSpan()),
                ReturnType = returnType,
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

            if (context.typedargslist() != null)
            {
                result.Parameters = ((Collection)Visit(context.typedargslist()))
                    .Collection.Cast<ParameterDeclaration>().ToList();
            }

            return result;
        }

        public Ust VisitDef_parameter([NotNull] PythonParser.Def_parameterContext context)
        {
            if (context.STAR() != null)
            {
                return new ParameterDeclaration()
                {
                    Name = new IdToken("*", context.GetTextSpan())
                };
            }
            var parameter = (ParameterDeclaration)Visit(context.named_parameter());
            var defaultContext = context.test();
            if (defaultContext != null)
            {
                parameter.Initializer = (Expression)Visit(defaultContext);
            }
            return parameter;
        }

        public Ust VisitNamed_parameter([NotNull] PythonParser.Named_parameterContext context)
        {
            var result = new ParameterDeclaration
            {
                Name = new IdToken(context.name().GetText(), context.name().GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };

            var annotationContext = context.test();
            if (annotationContext != null)
            {
                result.Type = new TypeToken(annotationContext.GetText(), annotationContext.GetTextSpan());
            }

            return result;
        }

        public Ust VisitDef_parameters([NotNull] PythonParser.Def_parametersContext context)
        {
            return new Collection(context.def_parameter().Select(Visit));
        }

        public Ust VisitVardef_parameters([NotNull] PythonParser.Vardef_parametersContext context)
        {
            return new Collection(context.vardef_parameter().Select(Visit));
        }

        public Ust VisitVardef_parameter([NotNull] PythonParser.Vardef_parameterContext context)
        {
            var parameter = new ParameterDeclaration
            {
                Name = new IdToken(context.name().GetText(), context.name().GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
            var defaultContext = context.test();
            if (defaultContext != null)
            {
                parameter.Initializer = (Expression)Visit(defaultContext);
            }
            return parameter;
        }

        public Ust VisitTypedargslist(PythonParser.TypedargslistContext context)
        {
            return CreateParametersCollection(context);
        }

        public Ust VisitArgs([NotNull] PythonParser.ArgsContext context)
        {
            var result = (ParameterDeclaration)Visit(context.named_parameter());
            result.Name.Id = "*" + result.Name.Id;
            return result;
        }

        public Ust VisitKwargs([NotNull] PythonParser.KwargsContext context)
        {
            var result = (ParameterDeclaration)Visit(context.named_parameter());
            result.Name.Id = "**" + result.Name.Id;
            return result;
        }

        public Ust VisitVarargs([NotNull] PythonParser.VarargsContext context)
        {
            return new ParameterDeclaration
            {
                Name = new IdToken("*" + context.name().GetText(), context.GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitVarkwargs([NotNull] PythonParser.VarkwargsContext context)
        {
            return new ParameterDeclaration
            {
                Name = new IdToken("**" + context.name().GetText(), context.GetTextSpan()),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitVarargslist(PythonParser.VarargslistContext context)
        {
            return CreateParametersCollection(context);
        }

        public Ust VisitStmt(PythonParser.StmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimple_stmt(PythonParser.Simple_stmtContext context)
        {
            var statement = context.children.FirstOrDefault(x => !x.GetText().Contains("\n"));
            return Visit(statement).ToStatementIfRequired();
        }

        public Ust VisitSmall_stmt(PythonParser.Small_stmtContext context)
        {
            return Visit(context);
        }

        public Ust VisitExpr_stmt(PythonParser.Expr_stmtContext context)
        {
            if (context.assign_part() == null)
            {
                return Visit(context.testlist_star_expr());
            }

            PythonParser.Assign_partContext assign_part = context.assign_part();

            if (assign_part.COLON() != null)
            {
                return ProcessAnnassign(context, assign_part);
            }

            if (assign_part.op != null)
            {
                return ProcessAugmented_assign(context, assign_part);
            }

            return ProcessAssign(context, assign_part);
        }

        private Expression ProcessAssign(PythonParser.Expr_stmtContext parent,
            PythonParser.Assign_partContext context)
        {
            var leftContext = (ParserRuleContext)parent.testlist_star_expr();
            var rightContext = (ParserRuleContext)context.GetChild(1);
            var textSpan = parent.GetTextSpan();
            Expression left, right;
            if (leftContext.ChildCount == rightContext.ChildCount)
            {
                var assignments = new List<AssignmentExpression>(leftContext.ChildCount);
                for (int i = 0; i < leftContext.ChildCount; i++)
                {
                    var visitedLeft = (Expression)Visit(leftContext.GetChild(i));
                    Expression visitedRight;
                    var rightContextChild = rightContext.GetChild(i);
                    var rightContextChildType =
                        (rightContextChild as ParserRuleContext)?.Start?.Type ?? 0;

                    if (rightContextChildType == PythonLexer.TRUE
                        || rightContextChildType == PythonLexer.FALSE
                    ) // check cause False and True is not reserved words in Python2
                    {
                        visitedRight = new BooleanLiteral(rightContextChildType == PythonLexer.TRUE,
                            ((ParserRuleContext)rightContextChild).GetTextSpan());
                    }
                    else
                    {
                        visitedRight = (Expression)Visit(rightContextChild);
                    }

                    if (visitedLeft == null && visitedRight == null)
                    {
                        continue;
                    }

                    var assignment = new AssignmentExpression
                    {
                        Left = visitedLeft,
                        Right = visitedRight
                    };
                    assignment.TextSpan = assignment.Left.TextSpan.Union(assignment.Right.TextSpan);
                    assignments.Add(assignment);
                }

                return new VariableDeclarationExpression(null, assignments, textSpan);
            }

            if (leftContext.ChildCount < rightContext.ChildCount)
            {
                left = (Expression)Visit(leftContext);
                right = new TupleCreateExpression
                {
                    TextSpan = rightContext.GetTextSpan(),
                    Initializers = rightContext.children
                        .Where(x => (x as ITerminalNode)?.Symbol.Type == PythonLexer.COMMA)
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

        private VariableDeclarationExpression ProcessAnnassign(PythonParser.Expr_stmtContext parent, PythonParser.Assign_partContext context)
        {
            var typeContext = context.test();
            var result = new VariableDeclarationExpression
            {
                Type = new TypeToken(typeContext.GetText(), typeContext.GetTextSpan()),
                TextSpan = parent.GetTextSpan()
            };
            var leftContexts = parent.testlist_star_expr().children
                .Where(x => (x as ITerminalNode)?.Symbol.Type == PythonLexer.COMMA);
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

            if (context.yield_expr().Length == 1)
            {
                var right = Visit(context.yield_expr(0)).ToExpressionIfRequired();
                result.Variables.AddRange(assignments.Select(x =>
                   {
                       x.Right = right;
                       return x;
                   }));
            }
            else
            {
                var rightContexts = context.testlist()?.children
                    .Where(x => (x as ITerminalNode)?.Symbol.Type == PythonLexer.COMMA);
                if (rightContexts?.Count() == assignments.Count)
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

        private Expression ProcessAugmented_assign(PythonParser.Expr_stmtContext parent, PythonParser.Assign_partContext context)
        {
            ParserRuleContext rightContext;
            if (context.yield_expr().Length == 1)
            {
                rightContext = context.yield_expr(0);
            }
            else
            {
                rightContext = context.testlist();
            }
            return CreateBinaryOperatorExpression(parent.testlist_star_expr(), context.op.Text.Remove(context.op.Text.Length - 1), context.op.GetTextSpan(), rightContext);
        }

        public Ust VisitTestlist_star_expr(PythonParser.Testlist_star_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAssign_part(PythonParser.Assign_partContext context)
        {
            throw new ShouldNotBeVisitedException(nameof(context));
        }

        public Ust VisitPrint_stmt(PythonParser.Print_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDel_stmt(PythonParser.Del_stmtContext context)
        {
            return new UnaryOperatorExpression
            {
                Operator = new UnaryOperatorLiteral(UnaryOperator.Delete),
                Expression = Visit(context.exprlist()).ToExpressionIfRequired(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitPass_stmt(PythonParser.Pass_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBreak_stmt(PythonParser.Break_stmtContext context)
        {
            return new BreakStatement(context.GetTextSpan());
        }

        public Ust VisitContinue_stmt(PythonParser.Continue_stmtContext context)
        {
            return new ContinueStatement(context.GetTextSpan());
        }

        public Ust VisitReturn_stmt(PythonParser.Return_stmtContext context)
        {
            return new ReturnStatement(Visit(context.testlist()).ToExpressionIfRequired(), context.GetTextSpan());
        }

        public Ust VisitRaise_stmt(PythonParser.Raise_stmtContext context)
        {
            return new ThrowStatement(Visit(context.test().FirstOrDefault()).ToExpressionIfRequired(), context.GetTextSpan());
        }

        public Ust VisitYield_stmt(PythonParser.Yield_stmtContext context)
        {
            return new ExpressionStatement((Expression)Visit(context.yield_expr()), context.GetTextSpan());
        }

        public Ust VisitImport_stmt(PythonParser.Import_stmtContext context)
        {
            string name = context.dotted_as_names().GetText();
            TextSpan textSpan = context.dotted_as_names().GetTextSpan();
            return new UsingDeclaration(new StringLiteral(name, textSpan, 0), context.GetTextSpan());
        }

        public Ust VisitFrom_stmt(PythonParser.From_stmtContext context)
        {
            string name = context.children.LastOrDefault()?.GetText() ?? "";
            TextSpan textSpan = context.GetTextSpan();
            return new UsingDeclaration(new StringLiteral(name, textSpan, 0), context.GetTextSpan());
        }

        public Ust VisitImport_as_name(PythonParser.Import_as_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDotted_as_name(PythonParser.Dotted_as_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImport_as_names(PythonParser.Import_as_namesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDotted_as_names(PythonParser.Dotted_as_namesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDotted_name(PythonParser.Dotted_nameContext context)
        {
            if (context.dotted_name() != null)
            {
                return new MemberReferenceExpression
                {
                    Target = (Expression)Visit(context.dotted_name()),
                    Name = (Expression)Visit(context.name()),
                    TextSpan = context.GetTextSpan()
                };
            }

            return new IdToken(context.name().GetText(), context.name().GetTextSpan());
        }

        public Ust VisitGlobal_stmt(PythonParser.Global_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExec_stmt(PythonParser.Exec_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCompound_stmt(PythonParser.Compound_stmtContext context)
        {
            return Visit(context);
        }

        public Ust VisitIf_stmt(PythonParser.If_stmtContext context)
        {
            var condition = (Expression)Visit(context.test());
            Statement elseStatement = null;
            var result = new IfElseStatement
            {
                Condition = condition,
                TextSpan = context.GetTextSpan()
            };

            if (context.suite() != null)
            {
                result.TrueStatement = (Statement)Visit(context.suite());
                List<IfElseStatement> ifElseStatements = context.elif_clause()
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

        public Ust VisitElif_clause([NotNull] PythonParser.Elif_clauseContext context)
        {
            return new IfElseStatement
            {
                Condition = (Expression)Visit(context.test()),
                TrueStatement = (BlockStatement)Visit(context.suite()),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitWhile_stmt(PythonParser.While_stmtContext context)
        {
            return new WhileStatement
            {
                Condition = Visit(context.test()).ToExpressionIfRequired(),
                Embedded = Visit(context.suite()).ToStatementIfRequired(),
                TextSpan = context.GetTextSpan()
            };
        }

        //TODO: Add specific for statement with few VarNames and InExpressions and else statement
        public Ust VisitClass_or_func_def_stmt(PythonParser.Class_or_func_def_stmtContext context)
        {
            var result = Visit((IParseTree)context.classdef() ?? context.funcdef());

            if (context.decorator().Length > 0 && result is IHasAttributes attributable)
            {
                var attributes = new List<Attribute>();

                foreach (PythonParser.DecoratorContext dec in context.decorator())
                {
                    attributes.Add((Attribute)Visit(dec));
                }

                attributable.Attributes = attributes;
            }

            return result;
        }

        public Ust VisitFor_stmt(PythonParser.For_stmtContext context)
        {
            return new ForeachStatement
            {
                VarName = new IdToken(context.exprlist().GetText(), context.exprlist().GetTextSpan()),
                InExpression = Visit(context.testlist()).ToExpressionIfRequired(),
                EmbeddedStatement = Visit(context.suite()).ToStatementIfRequired(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitAssert_stmt(PythonParser.Assert_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNonlocal_stmt(PythonParser.Nonlocal_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTry_stmt(PythonParser.Try_stmtContext context)
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

        public Ust VisitExcept_clause(PythonParser.Except_clauseContext context)
        {
            var result = new CatchClause
            {
                VarName = (IdToken)Visit(context.name()),
                Body = (BlockStatement)Visit(context.suite())
            };
            if (context.test() != null)
            {
                result.Type = new TypeToken(context.test().GetText(), context.test().GetTextSpan());
            }
            return result;
        }

        public Ust VisitElse_clause([NotNull] PythonParser.Else_clauseContext context)
        {
            return Visit(context.suite());
        }

        public Ust VisitFinaly_clause([NotNull] PythonParser.Finaly_clauseContext context)
        {
            return Visit(context.suite());
        }

        public Ust VisitWith_stmt(PythonParser.With_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWith_item(PythonParser.With_itemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSuite(PythonParser.SuiteContext context)
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

        public Ust VisitTestlist_safe(PythonParser.Testlist_safeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTest(PythonParser.TestContext context)
        {
            if (context.LAMBDA() != null)
            {
                return CreateLambdaMethod(context.test(), context.varargslist(), context.GetTextSpan());
            }

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

            return Visit(context.logical_test(0));
        }

        public Ust VisitLogical_test([NotNull] PythonParser.Logical_testContext context)
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
                return CreateBinaryOperatorExpression(context.logical_test(0), context.op, context.logical_test(1));
            }

            return VisitChildren(context);
        }

        public Ust VisitComparison(PythonParser.ComparisonContext context)
        {
            if (context.expr() == null)
            {
                if (context.optional == null)
                {
                    return CreateBinaryOperatorExpression(context.comparison(0), context.GetChild<ITerminalNode>(0),
                        context.comparison(1));
                }

                return new UnaryOperatorExpression
                {
                    Operator = new UnaryOperatorLiteral(UnaryOperator.Not, context.NOT().GetTextSpan()),
                    Expression = CreateBinaryOperatorExpression(context.comparison(0),
                        context.IN() ?? context.IS(), context.comparison(1))
                };
            }

            return Visit(context.expr());
        }

        public Ust VisitStar_expr(PythonParser.Star_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpr(PythonParser.ExprContext context)
        {
            if (context.op != null)
            {
                return context.ChildCount == 3
                    ? CreateBinaryOperatorExpression(context.expr(0), context.op, context.expr(1))
                    : CreateUnaryOperatorExpression(context.expr(0), context.op);
            }

            if (context.atom() != null)
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
                                                   || trailer.GetChild(1) is PythonParser.ArglistContext
                                    ? CreateInvocationExpression(target, argsUst, textSpan)
                                    : CreateIndexerExpression(target, argsUst, textSpan);
                                continue;
                            }

                            resultExpression = trailer.ChildCount == 2
                                               || trailer.GetChild(1) is PythonParser.ArglistContext
                                ? CreateInvocationExpression(resultExpression, argsUst, textSpan)
                                : CreateIndexerExpression(resultExpression, argsUst, textSpan);
                        }
                    }
                    return resultExpression;
                }


                return target;
            }

            return VisitChildren(context);
        }

        public Ust VisitAtom(PythonParser.AtomContext context)
        {
            if (context.ChildCount == 3)
            {
                var visited = Visit(context.GetChild(1));

                return new ArrayCreationExpression
                {
                    Initializers = visited is MultichildExpression multichild
                        ? UstUtils.ExtractMultiChild(multichild)
                        : new List<Expression> { visited.ToExpressionIfRequired() },
                    TextSpan = context.GetTextSpan()
                };
            }
            else if (context.ChildCount == 2)
            {
                return new ArrayCreationExpression
                {
                    TextSpan = context.GetTextSpan()
                };
            }
            return VisitChildren(context);
        }

        public Ust VisitName(PythonParser.NameContext context)
        {
            return new IdToken(context.GetText(), context.GetTextSpan());
        }

        public Ust VisitNumber(PythonParser.NumberContext context)
        {
            if (context.integer() != null)
            {
                return VisitInteger(context.integer());
            }

            if (context.FLOAT_NUMBER() != null)
            {
                IToken symbol = context.FLOAT_NUMBER().Symbol;
                ConvertHelper.TryParseDoubleInvariant(symbol.Text, out double value);
                return new FloatLiteral(value, symbol.GetTextSpan());
            }

            return Visit(context.GetChild(0));
        }

        public Ust VisitInteger(PythonParser.IntegerContext context)
        {
            int fromBase = context.DECIMAL_INTEGER() != null
                ? 10
                : context.HEX_INTEGER() != null
                    ? 16
                    : context.OCT_INTEGER() != null
                        ? 8
                        : 2;

            ReadOnlySpan<char> span = ExtractSpan(context.GetChild<ITerminalNode>(0).Symbol, out TextSpan textSpan);
            convertHelper.TryConvertNumeric(span, textSpan, fromBase, out Literal numeric);

            return numeric;
        }

        public Ust VisitTestlist_comp(PythonParser.Testlist_compContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTrailer(PythonParser.TrailerContext context)
        {
            return context.ChildCount == 3
                ? Visit(context.GetChild(1))
                : new ArgsUst { TextSpan = context.GetTextSpan() };
        }

        public Ust VisitSubscriptlist(PythonParser.SubscriptlistContext context)
        {
            return new ArgsUst
            {
                Collection = context.subscript().Select(x => (Expression)Visit(x)).ToList(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitSubscript(PythonParser.SubscriptContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSliceop(PythonParser.SliceopContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExprlist(PythonParser.ExprlistContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTestlist(PythonParser.TestlistContext context)
        {
            return VisitChildren(context);
        }

        //TODO: handle dictionary initializers
        public Ust VisitDictorsetmaker(PythonParser.DictorsetmakerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitClassdef(PythonParser.ClassdefContext context)
        {
            var result = new TypeDeclaration
            {
                Name = new IdToken(context.name().GetText(), context.name().GetTextSpan()),
                Type = new TypeTypeLiteral { TypeType = TypeType.Class },
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
                    var typeMembers = typeBodyContext.stmt().Select(Visit);
                    typeMembers = typeMembers.Select(ConvertToTypeMember).Where(x => x != null);
                    result.TypeMembers.AddRange(typeMembers);
                }
            }
            return result;
        }

        private EntityDeclaration ConvertToTypeMember(Ust node)
        {
            if (node == null)
            {
                return null;
            }
            if (node is ExpressionStatement exprStmt
                && exprStmt.Expression is VariableDeclarationExpression variableDeclaration)
            {
                return new FieldDeclaration(variableDeclaration.Type, variableDeclaration.Variables, variableDeclaration.TextSpan);
            }
            return node is EntityDeclaration entityDeclaration
                ? entityDeclaration
                : new StatementDeclaration(node.ToStatementIfRequired(), node.TextSpan);
        }

        public Ust VisitArglist(PythonParser.ArglistContext context)
        {
            return new ArgsUst
            {
                Collection = context.argument().Select(x => Visit(x).ToExpressionIfRequired()).ToList(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitArgument(PythonParser.ArgumentContext context)
        {
            if (context.ChildCount == 3
                && (context.GetChild(1) as ITerminalNode)?.Symbol.Type == PythonLexer.ASSIGN)
            {
                return new AssignmentExpression
                {
                    Left = Visit(context.GetChild(0)).ToExpressionIfRequired(),
                    Right = Visit(context.GetChild(2)).ToExpressionIfRequired(),
                    TextSpan = context.GetTextSpan()
                };
            }
            return VisitChildren(context);
        }

        public Ust VisitList_iter(PythonParser.List_iterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComp_iter(PythonParser.Comp_iterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComp_for(PythonParser.Comp_forContext context)
        {
            return new ForeachStatement
            {
                VarName = new IdToken(context.exprlist().GetText(), context.exprlist().GetTextSpan()),
                InExpression = Visit(context.logical_test()).ToExpressionIfRequired(),
                EmbeddedStatement = Visit(context.comp_iter()).ToStatementIfRequired(),
                TextSpan = context.GetTextSpan()
            };
        }

        public Ust VisitYield_expr(PythonParser.Yield_exprContext context)
        {
            var yieldArg = context.yield_arg();
            return yieldArg == null
                ? new YieldExpression(null, context.GetTextSpan())
                : new YieldExpression(Visit(yieldArg).ToExpressionIfRequired(), context.GetTextSpan());
        }

        public Ust VisitYield_arg(PythonParser.Yield_argContext context)
        {
            if (context.test() != null)
            {
                return Visit(context.test());
            }
            return Visit(context.testlist());
        }
    }
}