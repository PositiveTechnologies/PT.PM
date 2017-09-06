using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Collections;
using PT.PM.AntlrUtils;
using PT.PM.Patterns.Nodes;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using Antlr4.Runtime.Tree;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Dsl
{
    public class DslUstConverter : IDslParserVisitor<UstNode>
    {
        public LanguageFlags SourceLanguage { get; set; }

        public bool PatternExpressionInsideStatement { get; set; } = true;

        public string Data { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        const string DslHelperPrefix = "pt.pm_";
        private Dictionary<string, PatternVarDef> patternVarDefs;
        private int unnamedVarNumber;

        public DslNode Convert(DslParser.PatternContext pattern)
        {
            try
            {
                unnamedVarNumber = 0;
                patternVarDefs = new Dictionary<string, PatternVarDef>();
                var result = (DslNode)VisitPattern(pattern);
                result.PatternVarDefs = patternVarDefs.Select(keyValue => keyValue.Value).ToList();
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException("Pattern", ex) { IsPattern = true });
                throw;
            }
        }

        public UstNode VisitPattern(DslParser.PatternContext context)
        {
            DslNode result;
            result = new DslNode(context.dslCode().Select(code => VisitDslCode(code)).ToList());
            return result;
        }

        public UstNode VisitDslCode(DslParser.DslCodeContext context)
        {
            UstNode result;
            if (context.statement().Length > 0)
            {
                Statement[] statements = context.statement().Select(statement =>
                (Statement)VisitStatement(statement))
                .Where(statement => statement.NodeType != NodeType.PatternMultipleStatements).ToArray();
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

        public UstNode VisitStatement(DslParser.StatementContext context)
        {
            return context.Accept(this);
        }

        public UstNode VisitExpressionStatement(DslParser.ExpressionStatementContext context)
        {
            Statement result;
            var expression = (Expression)VisitExpression(context.expression());
            if (!PatternExpressionInsideStatement)
            {
                result = new ExpressionStatement(expression, context.GetTextSpan(), null);
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

        public UstNode VisitPatternStatement(DslParser.PatternStatementContext context)
        {
            return new PatternStatement() { TextSpan = context.GetTextSpan() };
        }

        public UstNode VisitPatternMultipleStatement(DslParser.PatternMultipleStatementContext context)
        {
            return new PatternMultipleStatements() { TextSpan = context.GetTextSpan() };
        }

        public UstNode VisitPatternTryCatchStatement(DslParser.PatternTryCatchStatementContext context)
        {
            var exceptionTypes = ProcessLiteralsOrPatternIds(context.literalOrPatternId());
            bool isCatchBodyEmpty = context.Ellipsis() == null;
            var result = new PatternTryCatchStatement(exceptionTypes, isCatchBodyEmpty, context.GetTextSpan(), null);
            return result;
        }

        public UstNode VisitExpression(DslParser.ExpressionContext context)
        {
            return context.Accept(this);
        }

        public UstNode VisitPatternOrExpression(DslParser.PatternOrExpressionContext context)
        {
            Expression[] values = context.expression().Select(expr =>
            {
                return (Expression)VisitExpression(expr);
            }).ToArray();
            var result = new PatternVarDef(GetNewVarDefName(), values, context.GetTextSpan());
            return result;
        }

        public UstNode VisitPatternAndExpression(DslParser.PatternAndExpressionContext context)
        {
            var expressions = context.expression().Select(expr =>
            {
                return (Expression)VisitExpression(expr);
            }).ToList();
            return new PatternAnd(expressions, context.GetTextSpan(), null);
        }

        public UstNode VisitPatternNotExpression(DslParser.PatternNotExpressionContext context)
        {
            var expression = (Expression)VisitExpression(context.expression());
            return new PatternNot(expression, context.GetTextSpan(), null);
        }

        public UstNode VisitClassDeclaration(DslParser.ClassDeclarationContext context)
        {
            List<Token> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            List<Token> baseTypes = ProcessLiteralsOrPatternIds(context._baseTypes);

            Token name = null;
            var nameContext = context.name;
            if (nameContext != null)
            {
                name = (Token)VisitLiteralOrPatternId(context.name);
            }

            PatternExpressionInsideNode body = null;
            var arbitraryDepthExpression = context.arbitraryDepthExpression();
            if (arbitraryDepthExpression != null)
            {
                body = (PatternExpressionInsideNode)VisitArbitraryDepthExpression(context.arbitraryDepthExpression());
            }

            return new PatternClassDeclaration(modifiers, name, baseTypes, body, context.GetTextSpan(), null);
        }

        public UstNode VisitMethodDeclaration(DslParser.MethodDeclarationContext context)
        {
            UstNode result;
            List<Token> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            var name = (Token)VisitLiteralOrPatternId(context.methodName);
            var arbitraryDepthExpression = context.arbitraryDepthExpression();
            if (arbitraryDepthExpression != null)
            {
                var body = (PatternExpressionInsideNode)VisitArbitraryDepthExpression(context.arbitraryDepthExpression());
                result = new PatternMethodDeclaration(modifiers, name, body, context.GetTextSpan(), null);
            }
            else if (context.Ellipsis() != null)
            {
                // Any body
                result = new PatternMethodDeclaration(modifiers, name, true, context.GetTextSpan(), null);
            }
            else
            {
                result = new PatternMethodDeclaration(modifiers, name, false, context.GetTextSpan(), null);
            }
            return result;
        }

        public UstNode VisitInvocationExpression(DslParser.InvocationExpressionContext context)
        {
            ArgsNode args = context.args() == null ? new ArgsNode() : (ArgsNode)VisitArgs(context.args());
            var expr = (Expression)VisitExpression(context.expression());
            var result = new InvocationExpression(expr, args, context.GetTextSpan(), null);
            return result;
        }

        public UstNode VisitMemberReferenceExpression(DslParser.MemberReferenceExpressionContext context)
        {
            var target = (Expression)VisitExpression(context.expression());
            var type = (Expression)VisitLiteralOrPatternId(context.literalOrPatternId());
            var result = new MemberReferenceExpression(target, type, context.GetTextSpan(), null);
            return result;
        }

        public UstNode VisitBinaryOperatorExpression(DslParser.BinaryOperatorExpressionContext context)
        {
            var left = (Expression)VisitExpression(context.expression(0));
            var literal = new BinaryOperatorLiteral(context.op.Text, context.op.GetTextSpan(), null);
            var right = (Expression)VisitExpression(context.expression(1));
            var textSpan = context.GetTextSpan();

            var result = new BinaryOperatorExpression(left, literal, right, textSpan, null);
            return result;
        }

        public UstNode VisitIndexerExpression(DslParser.IndexerExpressionContext context)
        {
            var target = (Expression)VisitExpression(context.expression(0));
            var args = new ArgsNode(new[] { (Expression)VisitExpression(context.expression(1)) },
                context.expression(1).GetTextSpan(), null);

            var result = new IndexerExpression(target, args, context.GetTextSpan(), null);
            return result;
        }

        public UstNode VisitMemberReferenceOrLiteralExpression(DslParser.MemberReferenceOrLiteralExpressionContext context)
        {
            var textSpan = context.GetTextSpan();
            var values = new Expression[]
            {
                new MemberReferenceExpression((Expression)VisitExpression(context.expression()), (Expression)VisitLiteralOrPatternId(context.literalOrPatternId()), textSpan, null),
                (Expression)VisitLiteralOrPatternId(context.literalOrPatternId())
            };
            var result = new PatternVarDef(GetNewVarDefName(), values, context.GetTextSpan());
            return result;
        }

        public UstNode VisitAssignmentExpression(DslParser.AssignmentExpressionContext context)
        {
            Expression result;
            var left = (Expression)VisitExpression(context.expression(0));
            var right = (Expression)VisitExpression(context.expression(1));
            result = new AssignmentExpression(left, right, context.GetTextSpan(), null);
            return result;
        }

        public UstNode VisitVariableDeclarationExpression([NotNull] DslParser.VariableDeclarationExpressionContext context)
        {
            var literal = (Token)VisitLiteralOrPatternId(context.literalOrPatternId());
            var typeToken = new TypeToken(literal.TextValue, literal.TextSpan, null);
            var left = (Expression)VisitExpression(context.expression(0));
            var right = (Expression)VisitExpression(context.expression(1));
            var variables = new AssignmentExpression[] {
                new AssignmentExpression(left, right, left.TextSpan.Union(right.TextSpan), null) };
            var result = new VariableDeclarationExpression(typeToken, variables, context.GetTextSpan(), null);
            return result;
        }

        public UstNode VisitComparisonExpression([NotNull] DslParser.ComparisonExpressionContext context)
        {
            var left = (Expression)VisitExpression(context.expression(0));
            var right = (Expression)VisitExpression(context.expression(1));
            var opLiteral = new BinaryOperatorLiteral(BinaryOperatorLiteral.TextBinaryOperator[context.op.Text],
                context.op.GetTextSpan(), null);
            var result = new BinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan(), null);
            return result;
        }

        public UstNode VisitObjectCreationExpression(DslParser.ObjectCreationExpressionContext context)
        {
            var literal = (Token)VisitLiteralOrPatternId(context.literalOrPatternId());
            var typeToken = new TypeToken(literal.TextValue, literal.TextSpan, null);
            ArgsNode args = context.args() == null ? new ArgsNode() : (ArgsNode)VisitArgs(context.args());
            var result = new ObjectCreateExpression(typeToken, args, context.GetTextSpan(), null);
            return result;
        }

        public UstNode VisitFunctionExpression([NotNull] DslParser.FunctionExpressionContext context)
        {
            var body = new PatternStatements(
                new Statement[] { new ExpressionStatement((Expression)VisitExpression(context.expression())) });
            return new AnonymousMethodExpression(new ParameterDeclaration[0], body, context.GetTextSpan(), null);
        }

        public UstNode VisitPatternLiteralExpression(DslParser.PatternLiteralExpressionContext context)
        {
            return (Expression)VisitPatternLiterals(context.patternLiterals());
        }

        public UstNode VisitLiteralExpression(DslParser.LiteralExpressionContext context)
        {
            return (Token)VisitLiteral(context.literal());
        }

        public UstNode VisitPatternExpression(DslParser.PatternExpressionContext context)
        {
            return new PatternExpression() { TextSpan = context.GetTextSpan() };
        }

        public UstNode VisitParenthesisExpression([NotNull] DslParser.ParenthesisExpressionContext context)
        {
            return VisitExpression(context.expression());
        }

        public UstNode VisitPatternArbitraryDepthExpression(
            [NotNull] DslParser.PatternArbitraryDepthExpressionContext context)
        {
            return VisitArbitraryDepthExpression(context.arbitraryDepthExpression());
        }

        public UstNode VisitArbitraryDepthExpression([NotNull] DslParser.ArbitraryDepthExpressionContext context)
        {
            return new PatternExpressionInsideNode(
                (Expression)VisitExpression(context.expression()), context.GetTextSpan(), null);
        }

        public UstNode VisitBaseReferenceExpression(DslParser.BaseReferenceExpressionContext context)
        {
            return new BaseReferenceExpression(context.GetTextSpan(), null);
        }

        public UstNode VisitArgs([NotNull] DslParser.ArgsContext context)
        {
            List<Expression> expressions = context.arg().Select(arg =>
                (Expression)VisitArg(arg)).ToList();

            var span = context.GetTextSpan();
            ArgsNode result;
            if (expressions.All(expr => expr.NodeType != NodeType.PatternMultipleExpressions))
            {
                result = new ArgsNode(expressions, span, null);
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

        public UstNode VisitArg([NotNull] DslParser.ArgContext context)
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

        public UstNode VisitLiteral(DslParser.LiteralContext context)
        {
            Token result;
            var textSpan = context.GetTextSpan();
            if (context.Id() != null)
            {
                result = ProcessId(context.Id());
            }
            else if (context.String() != null)
            {
                result = new StringLiteral(RemoveQuotes(context.GetText()), textSpan, null);
            }
            else if (context.Oct() != null)
            {
                result = new IntLiteral(
                    System.Convert.ToInt64(context.Oct().GetText(), 8), textSpan, null);
            }
            else if (context.Int() != null)
            {
                result = new IntLiteral(long.Parse(context.Int().GetText()), textSpan, null);
            }
            else if (context.Hex() != null)
            {
                result = new IntLiteral(
                    System.Convert.ToInt64(context.Hex().GetText(), 16), textSpan, null);
            }
            else if (context.Bool() != null)
            {
                result = new BooleanLiteral(bool.Parse(context.Bool().GetText()), textSpan, null);
            }
            else if (context.Null() != null)
            {
                result = new NullLiteral(textSpan, null);
            }
            else
            {
                throw new NotImplementedException();
            }
            return result;
        }

        public UstNode VisitLiteralOrPatternId(DslParser.LiteralOrPatternIdContext context)
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

        public UstNode VisitPatternLiterals([NotNull] DslParser.PatternLiteralsContext context)
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

        public UstNode VisitPatternNotLiteral([NotNull] DslParser.PatternNotLiteralContext context)
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

        public UstNode VisitPatternLiteral(DslParser.PatternLiteralContext context)
        {
            return (Token)context.Accept(this);
        }

        public UstNode VisitPatternInt([NotNull] DslParser.PatternIntContext context)
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
            return new IntLiteral(resultValue, context.GetTextSpan(), null);
        }

        public UstNode VisitPatternStringLiteral(DslParser.PatternStringLiteralContext context)
        {
            string value = RemoveQuotes(context.GetText());
            return new PatternStringLiteral(value == "" ? ".*" : value, context.GetTextSpan());
        }

        public UstNode VisitPatternIdToken(DslParser.PatternIdTokenContext context)
        {
            return (IdToken)VisitPatternId(context.patternId());
        }

        public UstNode VisitPatternIntLiteral(DslParser.PatternIntLiteralContext context)
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

        public UstNode VisitPatternIntExpression([NotNull] DslParser.PatternIntExpressionContext context)
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
                result = new IntLiteral(resultValue, context.GetTextSpan(), null);
            }
            else
            {
                result = (IntLiteral)VisitPatternInt(context.patternInt());
            }
            return result;
        }

        public UstNode VisitPatternBoolLiteral(DslParser.PatternBoolLiteralContext context)
        {
            var boolText = context.PatternBool().GetText();
            var result = new PatternBooleanLiteral(boolText == "bool" ? (bool?)null : bool.Parse(boolText));
            result.TextSpan = context.GetTextSpan();
            return result;
        }

        public UstNode VisitPatternNullLiteral(DslParser.PatternNullLiteralContext context)
        {
            return new NullLiteral(context.GetTextSpan(), null);
        }

        public UstNode VisitPatternId([NotNull] DslParser.PatternIdContext context)
        {
            string patternId = context.GetText();
            if (SourceLanguage.IsCaseInsensitive() && !patternId.StartsWith("(?i)"))
            {
                patternId = "(?i)" + patternId;
            }
            IdToken result = new PatternIdToken(patternId, context.GetTextSpan());
            return result;
        }

        public UstNode Visit(IParseTree tree)
        {
            throw new ShouldNotBeVisitedException("DSL node");
        }

        public UstNode VisitChildren(IRuleNode node)
        {
            throw new ShouldNotBeVisitedException("DSL children");
        }

        public UstNode VisitTerminal(ITerminalNode node)
        {
            throw new ShouldNotBeVisitedException("DSL Terminal");
        }

        public UstNode VisitErrorNode(IErrorNode node)
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
            IdToken result;
            if (SourceLanguage.IsCaseInsensitive())
            {
                result = new PatternIdToken("(?i)^" + id + "$", idTerminal.GetTextSpan());
            }
            else
            {
                result = new IdToken(id, idTerminal.GetTextSpan(), null);
            }
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
            }).ToList(); ;
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
                Token[] values = contexts.Select(literal =>
                {
                    return (IdToken)VisitPatternId(literal);
                }).ToArray();
                result = new PatternVarDef(GetNewVarDefName(), values, firstPatternId.GetTextSpan());
            }
            return result;
        }
    }
}
