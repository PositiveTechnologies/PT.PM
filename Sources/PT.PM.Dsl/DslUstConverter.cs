using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Collections;
using PT.PM.AntlrUtils;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using Antlr4.Runtime.Tree;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Matching.Patterns;

namespace PT.PM.Dsl
{
    public class DslUstConverter : IDslParserVisitor<Ust>
    {
        public HashSet<Language> Languages { get; set; }

        public bool PatternExpressionInsideStatement { get; set; } = true;

        public string Data { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        const string DslHelperPrefix = "pt.pm_";
        private Dictionary<string, PatternVarDef> patternVarDefs;
        private int unnamedVarNumber;

        public PatternRootUst Convert(DslParser.PatternContext pattern)
        {
            try
            {
                unnamedVarNumber = 0;
                patternVarDefs = new Dictionary<string, PatternVarDef>();
                var result = (PatternRootUst)VisitPattern(pattern);
                result.Languages = new HashSet<Language>(LanguageExt.AllPatternLanguages);
                result.Vars = patternVarDefs.Select(keyValue => keyValue.Value).ToList();
                result.FillAscendants();
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException("Pattern", ex) { IsPattern = true });
                throw;
            }
        }

        public Ust VisitPattern(DslParser.PatternContext context)
        {
            PatternRootUst result;
            result = new PatternRootUst
            {
                Nodes = context.dslCode().Select(code => VisitDslCode(code)).ToArray()
            };
            return result;
        }

        public Ust VisitDslCode(DslParser.DslCodeContext context)
        {
            Ust result;
            if (context.statement().Length > 0)
            {
                Statement[] statements = context.statement().Select(statement =>
                (Statement)VisitStatement(statement))
                .Where(statement => statement.Kind != UstKind.PatternMultipleStatements).ToArray();
                var resultStatements = new List<Statement>();
                foreach (var statement in statements)
                {
                    resultStatements.Add(statement);
                    resultStatements.Add(new PatternMultipleStatements());
                }
                if (resultStatements.Count == 0)
                {
                    resultStatements.Add(new PatternMultipleStatements());
                }
                else
                {
                    resultStatements.RemoveAt(resultStatements.Count - 1);
                }
                result = new PatternStatements
                {
                    Statements = resultStatements,
                    TextSpan = context.GetTextSpan()
                };
            }
            else if (context.expression() != null)
            {
                result = VisitExpression(context.expression());
            }
            else
            {
                PatternComment[] patternComments = context.PatternString().Select(literal =>
                    new PatternComment(RemoveQuotes(literal.GetText()), literal.GetTextSpan())).ToArray();
                if (patternComments.Length == 1)
                {
                    result = patternComments[0];
                }
                else
                {
                    result = new PatternVarDef(GetNewVarDefName(), patternComments, context.GetTextSpan());
                }
            }
            return result;
        }

        public Ust VisitStatement(DslParser.StatementContext context)
        {
            return context.Accept(this);
        }

        public Ust VisitExpressionStatement(DslParser.ExpressionStatementContext context)
        {
            Statement result;
            var expression = (Expression)VisitExpression(context.expression());
            if (!PatternExpressionInsideStatement)
            {
                result = new ExpressionStatement(expression, context.GetTextSpan());
                if (context.PatternNot() != null)
                {
                    result = new PatternStatement(result, true);
                }
            }
            else
            {
                result = new PatternExpressionInsideStatement(expression, context.PatternNot() != null);
            }
            return result;
        }

        public Ust VisitPatternStatement(DslParser.PatternStatementContext context)
        {
            return new PatternStatement() { TextSpan = context.GetTextSpan() };
        }

        public Ust VisitPatternMultipleStatement(DslParser.PatternMultipleStatementContext context)
        {
            return new PatternMultipleStatements() { TextSpan = context.GetTextSpan() };
        }

        public Ust VisitPatternTryCatchStatement(DslParser.PatternTryCatchStatementContext context)
        {
            var exceptionTypes = ProcessLiteralsOrPatternIds(context.literalOrPatternId());
            bool isCatchBodyEmpty = context.Ellipsis() == null;
            var result = new PatternTryCatchStatement(exceptionTypes, isCatchBodyEmpty, context.GetTextSpan());
            return result;
        }

        public Ust VisitExpression(DslParser.ExpressionContext context)
        {
            return context.Accept(this);
        }

        public Ust VisitPatternOrExpression(DslParser.PatternOrExpressionContext context)
        {
            Expression[] values = context.expression().Select(expr =>
            {
                return (Expression)VisitExpression(expr);
            }).ToArray();
            var result = new PatternVarDef(GetNewVarDefName(), values, context.GetTextSpan());
            return result;
        }

        public Ust VisitPatternAndExpression(DslParser.PatternAndExpressionContext context)
        {
            var expressions = context.expression().Select(expr =>
            {
                return (Expression)VisitExpression(expr);
            }).ToList();
            return new PatternAnd(expressions, context.GetTextSpan());
        }

        public Ust VisitPatternNotExpression(DslParser.PatternNotExpressionContext context)
        {
            var expression = (Expression)VisitExpression(context.expression());
            return new PatternNot(expression, context.GetTextSpan());
        }

        public Ust VisitClassDeclaration(DslParser.ClassDeclarationContext context)
        {
            List<Token> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            List<Token> baseTypes = ProcessLiteralsOrPatternIds(context._baseTypes);

            Token name = null;
            if (context.name != null)
            {
                name = (Token)VisitLiteralOrPatternId(context.name);
            }

            PatternExpressionInsideNode body = null;
            var arbitraryDepthExpression = context.arbitraryDepthExpression();
            if (arbitraryDepthExpression != null)
            {
                body = (PatternExpressionInsideNode)VisitArbitraryDepthExpression(arbitraryDepthExpression);
            }

            return new PatternClassDeclaration(modifiers, name, baseTypes, body, context.GetTextSpan());
        }

        public Ust VisitMethodDeclaration(DslParser.MethodDeclarationContext context)
        {
            Ust result;
            List<Token> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            var name = (Token)VisitLiteralOrPatternId(context.methodName);
            var arbitraryDepthExpression = context.arbitraryDepthExpression();
            if (arbitraryDepthExpression != null)
            {
                var body = (PatternExpressionInsideNode)VisitArbitraryDepthExpression(arbitraryDepthExpression);
                result = new PatternMethodDeclaration(modifiers, name, body, context.GetTextSpan());
            }
            else if (context.Ellipsis() != null)
            {
                // Any body
                result = new PatternMethodDeclaration(modifiers, name, true, context.GetTextSpan());
            }
            else
            {
                result = new PatternMethodDeclaration(modifiers, name, false, context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitVarOrFieldDeclarationExpression(DslParser.VarOrFieldDeclarationExpressionContext context)
        {
            bool localVariable = context.Field() == null;
            var typeLiteralOrPatternId = (Token)VisitLiteralOrPatternId(context.type);
            var type = typeLiteralOrPatternId is PatternIdToken ?
                typeLiteralOrPatternId :
                new TypeToken(typeLiteralOrPatternId.TextValue, typeLiteralOrPatternId.TextSpan);
            var name = (Expression)VisitVariableName(context.variableName());
            List<Token> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            var result = new PatternVarOrFieldDeclaration(localVariable, modifiers, type, name, context.GetTextSpan());
            return result;
        }

        public Ust VisitInvocationExpression(DslParser.InvocationExpressionContext context)
        {
            ArgsUst args = context.args() == null ? new ArgsUst() : (ArgsUst)VisitArgs(context.args());
            var expr = (Expression)VisitExpression(context.expression());
            var result = new InvocationExpression(expr, args, context.GetTextSpan());
            return result;
        }

        public Ust VisitMemberReferenceExpression(DslParser.MemberReferenceExpressionContext context)
        {
            var target = (Expression)VisitExpression(context.expression());
            var type = (Expression)VisitLiteralOrPatternId(context.literalOrPatternId());
            var result = new MemberReferenceExpression(target, type, context.GetTextSpan());
            return result;
        }

        public Ust VisitBinaryOperatorExpression(DslParser.BinaryOperatorExpressionContext context)
        {
            var left = (Expression)VisitExpression(context.expression(0));
            var literal = new BinaryOperatorLiteral(context.op.Text, context.op.GetTextSpan());
            var right = (Expression)VisitExpression(context.expression(1));
            var textSpan = context.GetTextSpan();

            var result = new BinaryOperatorExpression(left, literal, right, textSpan);
            return result;
        }

        public Ust VisitIndexerExpression(DslParser.IndexerExpressionContext context)
        {
            var target = (Expression)VisitExpression(context.expression(0));
            var args = new ArgsUst(new[] { (Expression)VisitExpression(context.expression(1)) },
                context.expression(1).GetTextSpan());

            var result = new IndexerExpression(target, args, context.GetTextSpan());
            return result;
        }

        public Ust VisitMemberReferenceOrLiteralExpression(DslParser.MemberReferenceOrLiteralExpressionContext context)
        {
            var textSpan = context.GetTextSpan();
            var values = new Expression[]
            {
                new MemberReferenceExpression((Expression)VisitExpression(context.expression()), (Expression)VisitLiteralOrPatternId(context.literalOrPatternId()), textSpan),
                (Expression)VisitLiteralOrPatternId(context.literalOrPatternId())
            };
            var result = new PatternVarDef(GetNewVarDefName(), values, context.GetTextSpan());
            return result;
        }

        public Ust VisitAssignmentExpression(DslParser.AssignmentExpressionContext context)
        {
            Expression result;
            var left = (Expression)VisitExpression(context.expression(0));
            var right = (Expression)VisitExpression(context.expression(1));
            if (left is PatternVarOrFieldDeclaration declaration)
            {
                declaration.Right = right;
                result = declaration;
            }
            else
            {
                result = new AssignmentExpression(left, right, context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitComparisonExpression([NotNull] DslParser.ComparisonExpressionContext context)
        {
            var left = (Expression)VisitExpression(context.expression(0));
            var right = (Expression)VisitExpression(context.expression(1));
            var opLiteral = new BinaryOperatorLiteral(BinaryOperatorLiteral.TextBinaryOperator[context.op.Text],
                context.op.GetTextSpan());
            var result = new BinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan());
            return result;
        }

        public Ust VisitObjectCreationExpression(DslParser.ObjectCreationExpressionContext context)
        {
            var literal = (Token)VisitLiteralOrPatternId(context.literalOrPatternId());
            var typeToken = new TypeToken(literal.TextValue, literal.TextSpan);
            ArgsUst args = context.args() == null ? new ArgsUst() : (ArgsUst)VisitArgs(context.args());
            var result = new ObjectCreateExpression(typeToken, args, context.GetTextSpan());
            return result;
        }

        public Ust VisitFunctionExpression([NotNull] DslParser.FunctionExpressionContext context)
        {
            var body = new PatternStatements(
                new Statement[] { new ExpressionStatement((Expression)VisitExpression(context.expression())) });
            return new AnonymousMethodExpression(new ParameterDeclaration[0], body, context.GetTextSpan());
        }

        public Ust VisitPatternLiteralExpression(DslParser.PatternLiteralExpressionContext context)
        {
            return (Expression)VisitPatternLiterals(context.patternLiterals());
        }

        public Ust VisitLiteralExpression(DslParser.LiteralExpressionContext context)
        {
            return (Token)VisitLiteral(context.literal());
        }

        public Ust VisitPatternExpression(DslParser.PatternExpressionContext context)
        {
            return new PatternExpression() { TextSpan = context.GetTextSpan() };
        }

        public Ust VisitParenthesisExpression([NotNull] DslParser.ParenthesisExpressionContext context)
        {
            return VisitExpression(context.expression());
        }

        public Ust VisitPatternArbitraryDepthExpression(
            [NotNull] DslParser.PatternArbitraryDepthExpressionContext context)
        {
            return VisitArbitraryDepthExpression(context.arbitraryDepthExpression());
        }

        public Ust VisitArbitraryDepthExpression([NotNull] DslParser.ArbitraryDepthExpressionContext context)
        {
            return new PatternExpressionInsideNode(
                (Expression)VisitExpression(context.expression()), context.GetTextSpan());
        }

        public Ust VisitBaseReferenceExpression(DslParser.BaseReferenceExpressionContext context)
        {
            return new BaseReferenceExpression(context.GetTextSpan());
        }

        public Ust VisitVariableName(DslParser.VariableNameContext context)
        {
            Ust result;
            if(context.literalOrPatternId() != null)
            {
                result = VisitLiteralOrPatternId(context.literalOrPatternId());
            }
            else
            {
                result = VisitPatternLiterals(context.patternLiterals());
            }
            return result;
        }

        public Ust VisitArgs([NotNull] DslParser.ArgsContext context)
        {
            List<Expression> expressions = context.arg().Select(arg =>
                (Expression)VisitArg(arg)).ToList();

            var span = context.GetTextSpan();
            ArgsUst result;
            if (expressions.All(expr => expr.Kind != UstKind.PatternMultipleExpressions))
            {
                result = new ArgsUst(expressions, span);
            }
            else
            {
                result = new PatternExpressions()
                {
                    Collection = expressions,
                    TextSpan = span
                };
            }
            return result;
        }

        public Ust VisitArg([NotNull] DslParser.ArgContext context)
        {
            if (context.expression() != null)
            {
                return (Expression)VisitExpression(context.expression());
            }
            else
            {
                return new PatternMultipleExpressions() { TextSpan = context.GetTextSpan() };
            }
        }

        public Ust VisitLiteral(DslParser.LiteralContext context)
        {
            Token result;
            var textSpan = context.GetTextSpan();
            if (context.Id() != null)
            {
                result = ProcessId(context.Id());
            }
            else if (context.String() != null)
            {
                result = new StringLiteral(RemoveQuotes(context.GetText()), textSpan);
            }
            else if (context.Oct() != null)
            {
                result = new IntLiteral(
                    System.Convert.ToInt64(context.Oct().GetText(), 8), textSpan);
            }
            else if (context.Int() != null)
            {
                result = new IntLiteral(long.Parse(context.Int().GetText()), textSpan);
            }
            else if (context.Hex() != null)
            {
                result = new IntLiteral(
                    System.Convert.ToInt64(context.Hex().GetText(), 16), textSpan);
            }
            else if (context.Bool() != null)
            {
                result = new BooleanLiteral(bool.Parse(context.Bool().GetText()), textSpan);
            }
            else if (context.Null() != null)
            {
                result = new NullLiteral(textSpan);
            }
            else
            {
                throw new NotImplementedException();
            }
            return result;
        }

        public Ust VisitLiteralOrPatternId(DslParser.LiteralOrPatternIdContext context)
        {
            Token result;
            if (context.Id() != null)
            {
                result = ProcessId(context.Id());
            }
            else
            {
                result = ProcessPatternIds(context.patternId());
            }
            return result;
        }

        public Ust VisitPatternLiterals([NotNull] DslParser.PatternLiteralsContext context)
        {
            Expression result;
            PatternVarDef patternVarDef;
            if (context.patternNotLiteral().Length == 1)
            {
                result = (Expression)VisitPatternNotLiteral(context.patternNotLiteral().First());
                if (context.PatternVar() != null)
                {
                    string id = context.PatternVar().GetText().Substring(1);
                    if (!patternVarDefs.TryGetValue(id, out patternVarDef))
                    {
                        patternVarDef = new PatternVarDef(id, new Expression[] { result }, context.GetTextSpan());
                        patternVarDefs[id] = patternVarDef;
                    }
                    else
                    {
                        if (context.patternNotLiteral().Length != 0)
                        {
                            var lcTextSpan = new LineColumnTextSpan(patternVarDef.TextSpan, Data);
                            throw new ConversionException(
                                $"DSL Error: PatternVar {id} with matching Id already defined earlier at {lcTextSpan}")
                            {
                                TextSpan = context.PatternVar().GetTextSpan()
                            };
                        }
                    }
                    result = new PatternVarRef(patternVarDef, context.GetTextSpan());
                }
            }
            else
            {
                List<Expression> values = context.patternNotLiteral()
                    .Select(literal => (Expression)VisitPatternNotLiteral(literal)).ToList();
                if (values.Count == 0)
                {
                    values.Add(new PatternIdToken("", context.GetTextSpan()));
                }

                if (context.PatternVar() == null)
                {
                    result = new PatternVarDef(GetNewVarDefName(), values, context.GetTextSpan());
                }
                else
                {
                    string id = context.PatternVar().GetText().Substring(1);
                    if (!patternVarDefs.TryGetValue(id, out patternVarDef))
                    {
                        patternVarDef = new PatternVarDef(id, values, context.GetTextSpan());
                        patternVarDefs[id] = patternVarDef;
                    }
                    else
                    {
                        if (context.patternNotLiteral().Length != 0)
                        {
                            var lcTextSpan = new LineColumnTextSpan(patternVarDef.TextSpan, Data);
                            throw new ConversionException(
                                $"DSL Error: PatternVar {id} with matching Id already defined earlier at {lcTextSpan}")
                            {
                                TextSpan = context.PatternVar().GetTextSpan()
                            };
                        }
                    }
                    result = new PatternVarRef(patternVarDef, context.GetTextSpan());
                }
            }
            return result;
        }

        public Ust VisitPatternNotLiteral([NotNull] DslParser.PatternNotLiteralContext context)
        {
            Token patternLiteral = (Token)VisitPatternLiteral(context.patternLiteral());
            Expression result;
            if (context.PatternNot() != null)
            {
                result = new PatternExpression(patternLiteral, true) { TextSpan = context.GetTextSpan() };
            }
            else
            {
                result = patternLiteral;
            }
            return result;
        }

        public Ust VisitPatternLiteral(DslParser.PatternLiteralContext context)
        {
            return (Token)context.Accept(this);
        }

        public Ust VisitPatternInt([NotNull] DslParser.PatternIntContext context)
        {
            long resultValue;
            if (context.PatternOct() != null)
            {
                resultValue = System.Convert.ToInt64(context.PatternOct().GetText(), 8);
            }
            else if (context.PatternInt() != null)
            {
                resultValue = long.Parse(context.PatternInt().GetText());
            }
            else
            {
                resultValue = System.Convert.ToInt64(context.PatternHex().GetText(), 16);
            }
            return new IntLiteral(resultValue, context.GetTextSpan());
        }

        public Ust VisitPatternStringLiteral(DslParser.PatternStringLiteralContext context)
        {
            string value = RemoveQuotes(context.GetText());
            return new PatternStringLiteral(value == "" ? ".*" : value, context.GetTextSpan());
        }

        public Ust VisitPatternIdToken(DslParser.PatternIdTokenContext context)
        {
            return (IdToken)VisitPatternId(context.patternId());
        }

        public Ust VisitPatternIntLiteral(DslParser.PatternIntLiteralContext context)
        {
            IntLiteral result;
            if (context.i != null)
            {
                result = (IntLiteral)VisitPatternIntExpression(context.i);
            }
            else
            {
                result = new PatternIntLiteral(
                    context.i1 != null ? ((IntLiteral)VisitPatternIntExpression(context.i1)).Value : long.MinValue,
                    context.i2 != null ? ((IntLiteral)VisitPatternIntExpression(context.i2)).Value : long.MaxValue);
            }
            result.TextSpan = context.GetTextSpan();
            return result;
        }

        public Ust VisitPatternIntExpression([NotNull] DslParser.PatternIntExpressionContext context)
        {
            IntLiteral result;
            if (context.op != null)
            {
                long leftValue = ((IntLiteral)VisitPatternIntExpression(context.left)).Value;
                long rightValue = ((IntLiteral)VisitPatternIntExpression(context.right)).Value;
                long resultValue = 0;
                switch (context.op.Text)
                {
                    case "*":
                        resultValue = leftValue * rightValue;
                        break;
                    case "/":
                        resultValue = leftValue / rightValue;
                        break;
                    case "+":
                        resultValue = leftValue + rightValue;
                        break;
                    case "-":
                        resultValue = leftValue - rightValue;
                        break;
                    default:
                        throw new NotImplementedException($"Operation {context.op.Text} is not implemented");
                }
                result = new IntLiteral(resultValue, context.GetTextSpan());
            }
            else
            {
                result = (IntLiteral)VisitPatternInt(context.patternInt());
            }
            return result;
        }

        public Ust VisitPatternBoolLiteral(DslParser.PatternBoolLiteralContext context)
        {
            var boolText = context.PatternBool().GetText();
            var result = new PatternBooleanLiteral(boolText == "bool" ? (bool?)null : bool.Parse(boolText));
            result.TextSpan = context.GetTextSpan();
            return result;
        }

        public Ust VisitPatternNullLiteral(DslParser.PatternNullLiteralContext context)
        {
            return new NullLiteral(context.GetTextSpan());
        }

        public Ust VisitPatternId([NotNull] DslParser.PatternIdContext context)
        {
            string patternId = context.GetText();
            IdToken result = new PatternIdToken(patternId, context.GetTextSpan());
            return result;
        }

        public Ust Visit(IParseTree tree)
        {
            throw new ShouldNotBeVisitedException("DSL node");
        }

        public Ust VisitChildren(IRuleNode node)
        {
            throw new ShouldNotBeVisitedException("DSL children");
        }

        public Ust VisitTerminal(ITerminalNode node)
        {
            throw new ShouldNotBeVisitedException("DSL Terminal");
        }

        public Ust VisitErrorNode(IErrorNode node)
        {
            throw new ShouldNotBeVisitedException("DSL Error");
        }

        private string RemoveQuotes(string str)
        {
            return str.Substring(1, str.Length - 2).Replace("\\\"", "\"");
        }

        private string GetNewVarDefName()
        {
            return $"{DslHelperPrefix}var_{unnamedVarNumber++}";
        }

        private IdToken ProcessId(ITerminalNode idTerminal)
        {
            string id = idTerminal.GetText();
            IdToken result = new IdToken(id, idTerminal.GetTextSpan());
            return result;
        }

        private List<Token> ProcessLiteralsOrPatternIds(
            IList<DslParser.LiteralOrPatternIdContext> literalsOrPatternIds)
        {
            // return literalsOrPatternIds.Select(VisitLiteralOrPatternId).OfType<Token>().ToList();
            return literalsOrPatternIds.Select(context =>
            {
                if (context.Id() != null)
                {
                    return new PatternIdToken(context.Id().GetText(), context.GetTextSpan());
                }
                else
                {
                    return ProcessPatternIds(context.patternId());
                }
            }).ToList();
        }

        private Token ProcessPatternIds(IList<DslParser.PatternIdContext> contexts)
        {
            Token result;
            var firstPatternId = contexts.First();
            if (contexts.Count == 1)
            {
                result = (Token)VisitPatternId(firstPatternId);
            }
            else
            {
                Token[] values = contexts.Select(literal => (IdToken)VisitPatternId(literal)).ToArray();
                result = new PatternVarDef(GetNewVarDefName(), values, firstPatternId.GetTextSpan());
            }
            return result;
        }
    }
}