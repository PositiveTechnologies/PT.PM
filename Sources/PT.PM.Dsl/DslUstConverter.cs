using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Dsl
{
    public class DslUstConverter : IDslParserVisitor<IPatternUst>
    {
        private const string DslHelperPrefix = "pt.pm_";
        private Dictionary<string, PatternVar> patternVars;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public HashSet<Language> Languages { get; set; }

        public bool PatternExpressionInsideStatement { get; set; } = true;

        public string Data { get; set; }

        public PatternRootUst Convert(DslParser.PatternContext pattern)
        {
            try
            {
                patternVars = new Dictionary<string, PatternVar>();
                var result = (PatternRootUst)VisitPattern(pattern);
                result.Languages = new HashSet<Language>(LanguageExt.AllPatternLanguages);
                result.FillAscendants();
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException("Pattern", ex) { IsPattern = true });
                throw;
            }
        }

        public IPatternUst VisitPattern(DslParser.PatternContext context)
        {
            PatternRootUst result;
            result = new PatternRootUst
            {
                Nodes = context.dslCode().Select(code => (Ust)VisitDslCode(code)).ToArray()
            };
            return result;
        }

        public IPatternUst VisitDslCode(DslParser.DslCodeContext context)
        {
            IPatternUst result;
            if (context.statement().Length > 0)
            {
                IEnumerable<PatternBase> statements = context.statement().Select(
                    statement => (PatternBase)VisitStatement(statement))
                    .Where(statement => statement != null);
                result = new PatternStatements(statements)
                {
                    TextSpan = context.GetTextSpan()
                };
            }
            else if (context.expression() != null)
            {
                result = VisitExpression(context.expression());
            }
            else
            {
                IEnumerable<PatternCommentRegex> patternComments = context.PatternString().Select(literal =>
                    new PatternCommentRegex(RemoveQuotes(literal.GetText()), literal.GetTextSpan()));
                if (patternComments.Count() == 1)
                {
                    result = patternComments.ElementAt(0);
                }
                else
                {
                    result = new PatternOr(patternComments, context.GetTextSpan());
                }
            }
            return result;
        }

        public IPatternUst VisitStatement(DslParser.StatementContext context)
        {
            return context.Accept(this);
        }

        public IPatternUst VisitExpressionStatement(DslParser.ExpressionStatementContext context)
        {
            PatternBase result = (PatternBase)VisitExpression(context.expression());
            if (!PatternExpressionInsideStatement)
            {
                if (context.PatternNot() != null)
                {
                    result = new PatternNot(result);
                }
            }
            else
            {
                result = new PatternArbitraryDepthExpression(result, context.GetTextSpan());
                if (context.PatternNot() != null)
                {
                    result = new PatternNot(result);
                }
            }
            return result;
        }

        public IPatternUst VisitPatternStatement(DslParser.PatternStatementContext context)
        {
            return new PatternAnyExpression(context.GetTextSpan());
        }

        public IPatternUst VisitPatternMultipleStatement(DslParser.PatternMultipleStatementContext context)
        {
            return null;
        }

        public IPatternUst VisitPatternTryCatchStatement(DslParser.PatternTryCatchStatementContext context)
        {
            var exceptionTypes = ProcessLiteralsOrPatternIds(context.literalOrPatternId());
            bool isCatchBodyEmpty = context.Ellipsis() == null;
            var result = new PatternTryCatchStatement(exceptionTypes, isCatchBodyEmpty, context.GetTextSpan());
            return result;
        }

        public IPatternUst VisitExpression(DslParser.ExpressionContext context)
        {
            return context.Accept(this);
        }

        public IPatternUst VisitPatternOrExpression(DslParser.PatternOrExpressionContext context)
        {
            IEnumerable<PatternBase> values = context.expression().Select(expr =>
            {
                return (PatternBase)VisitExpression(expr);
            });
            var result = new PatternOr(values, context.GetTextSpan());
            return result;
        }

        public IPatternUst VisitPatternAndExpression(DslParser.PatternAndExpressionContext context)
        {
            IEnumerable<PatternBase> values = context.expression().Select(expr =>
            {
                return (PatternBase)VisitExpression(expr);
            });
            return new PatternAnd(values, context.GetTextSpan());
        }

        public IPatternUst VisitPatternNotExpression(DslParser.PatternNotExpressionContext context)
        {
            var expression = (PatternBase)VisitExpression(context.expression());
            return new PatternNot(expression, context.GetTextSpan());
        }

        public IPatternUst VisitClassDeclaration(DslParser.ClassDeclarationContext context)
        {
            IEnumerable<PatternBase> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            IEnumerable<PatternBase> baseTypes = ProcessLiteralsOrPatternIds(context._baseTypes);

            PatternBase name = null;
            if (context.name != null)
            {
                name = (PatternBase)VisitLiteralOrPatternId(context.name);
            }

            PatternArbitraryDepthExpression body = null;
            var arbitraryDepthExpression = context.arbitraryDepthExpression();
            if (arbitraryDepthExpression != null)
            {
                body = (PatternArbitraryDepthExpression)VisitArbitraryDepthExpression(arbitraryDepthExpression);
            }

            return new PatternClassDeclaration(modifiers, name, baseTypes, body, context.GetTextSpan());
        }

        public IPatternUst VisitMethodDeclaration(DslParser.MethodDeclarationContext context)
        {
            IPatternUst result;
            IEnumerable<PatternBase> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            var name = (PatternBase)VisitLiteralOrPatternId(context.methodName);
            var arbitraryDepthExpression = context.arbitraryDepthExpression();
            if (arbitraryDepthExpression != null)
            {
                var body = (PatternArbitraryDepthExpression)VisitArbitraryDepthExpression(arbitraryDepthExpression);
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

        public IPatternUst VisitVarOrFieldDeclarationExpression(DslParser.VarOrFieldDeclarationExpressionContext context)
        {
            bool localVariable = context.Field() == null;
            var typeLiteralOrPatternId = (PatternBase)VisitLiteralOrPatternId(context.type);
            var type = typeLiteralOrPatternId is PatternIdRegexToken ?
                typeLiteralOrPatternId :
                new PatternIdToken(typeLiteralOrPatternId.ToString(), typeLiteralOrPatternId.TextSpan);
            var name = (PatternBase)VisitVariableName(context.variableName());
            IEnumerable<PatternBase> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            var assignment = new PatternAssignmentExpression(name, null, name.TextSpan);
            var result = new PatternVarOrFieldDeclaration(localVariable, modifiers, type, assignment, context.GetTextSpan());
            return result;
        }

        public IPatternUst VisitInvocationExpression(DslParser.InvocationExpressionContext context)
        {
            PatternArgs args = context.args() == null
                ? new PatternArgs()
                : (PatternArgs)VisitArgs(context.args());
            var expr = (PatternBase)VisitExpression(context.expression());
            var result = new PatternInvocationExpression(expr, args, context.GetTextSpan());
            return result;
        }

        public IPatternUst VisitMemberReferenceExpression(DslParser.MemberReferenceExpressionContext context)
        {
            var target = (PatternBase)VisitExpression(context.expression());
            var type = (PatternBase)VisitLiteralOrPatternId(context.literalOrPatternId());
            var result = new PatternMemberReferenceExpression(target, type, context.GetTextSpan());
            return result;
        }

        public IPatternUst VisitBinaryOperatorExpression(DslParser.BinaryOperatorExpressionContext context)
        {
            var left = (PatternBase)VisitExpression(context.expression(0));
            var literal = new BinaryOperatorLiteral(context.op.Text, context.op.GetTextSpan());
            var right = (PatternBase)VisitExpression(context.expression(1));
            var textSpan = context.GetTextSpan();

            var result = new PatternBinaryOperatorExpression(left, literal, right, textSpan);
            return result;
        }

        public IPatternUst VisitIndexerExpression(DslParser.IndexerExpressionContext context)
        {
            var target = (PatternBase)VisitExpression(context.expression(0));
            var args = new PatternArgs((PatternBase)VisitExpression(context.expression(1)));

            var result = new PatternIndexerExpression(target, args, context.GetTextSpan());
            return result;
        }

        public IPatternUst VisitMemberReferenceOrLiteralExpression(DslParser.MemberReferenceOrLiteralExpressionContext context)
        {
            var textSpan = context.GetTextSpan();
            var values = new PatternBase[]
            {
                new PatternMemberReferenceExpression((PatternBase)VisitExpression(context.expression()),
                    (PatternBase)VisitLiteralOrPatternId(context.literalOrPatternId()), textSpan),
                (PatternBase)VisitLiteralOrPatternId(context.literalOrPatternId())
            };
            var result = new PatternOr(values, context.GetTextSpan());
            return result;
        }

        public IPatternUst VisitAssignmentExpression(DslParser.AssignmentExpressionContext context)
        {
            PatternBase result;
            var left = (PatternBase)VisitExpression(context.expression(0));
            var right = (PatternBase)VisitExpression(context.expression(1));
            if (left is PatternVarOrFieldDeclaration declaration)
            {
                declaration.Assignment.Right = right;
                result = declaration;
            }
            else
            {
                result = new PatternAssignmentExpression(left, right, context.GetTextSpan());
            }
            return result;
        }

        public IPatternUst VisitComparisonExpression([NotNull] DslParser.ComparisonExpressionContext context)
        {
            var left = (PatternBase)VisitExpression(context.expression(0));
            var right = (PatternBase)VisitExpression(context.expression(1));
            var opLiteral = new BinaryOperatorLiteral(BinaryOperatorLiteral.TextBinaryOperator[context.op.Text],
                context.op.GetTextSpan());
            var result = new PatternBinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan());
            return result;
        }

        public IPatternUst VisitObjectCreationExpression(DslParser.ObjectCreationExpressionContext context)
        {
            var literal = (PatternBase)VisitLiteralOrPatternId(context.literalOrPatternId());
            var typeToken = new PatternIdToken(literal.ToString(), literal.TextSpan);
            PatternArgs args = context.args() == null
                ? new PatternArgs()
                : (PatternArgs)VisitArgs(context.args());
            var result = new PatternObjectCreateExpression(typeToken, args, context.GetTextSpan());
            return result;
        }

        public IPatternUst VisitFunctionExpression([NotNull] DslParser.FunctionExpressionContext context)
        {
            return VisitExpression(context.expression()); // TODO: remove FunctionExpression from DSL.
        }

        public IPatternUst VisitPatternLiteralExpression(DslParser.PatternLiteralExpressionContext context)
        {
            return VisitPatternLiterals(context.patternLiterals());
        }

        public IPatternUst VisitLiteralExpression(DslParser.LiteralExpressionContext context)
        {
            return VisitLiteral(context.literal());
        }

        public IPatternUst VisitPatternExpression(DslParser.PatternExpressionContext context)
        {
            return new PatternAnyExpression(context.GetTextSpan());
        }

        public IPatternUst VisitParenthesisExpression([NotNull] DslParser.ParenthesisExpressionContext context)
        {
            return VisitExpression(context.expression());
        }

        public IPatternUst VisitPatternArbitraryDepthExpression([NotNull] DslParser.PatternArbitraryDepthExpressionContext context)
        {
            return VisitArbitraryDepthExpression(context.arbitraryDepthExpression());
        }

        public IPatternUst VisitArbitraryDepthExpression([NotNull] DslParser.ArbitraryDepthExpressionContext context)
        {
            return new PatternArbitraryDepthExpression(
                (PatternBase)VisitExpression(context.expression()), context.GetTextSpan());
        }

        public IPatternUst VisitBaseReferenceExpression(DslParser.BaseReferenceExpressionContext context)
        {
            return new PatternBaseReferenceExpression(context.GetTextSpan());
        }

        public IPatternUst VisitVariableName(DslParser.VariableNameContext context)
        {
            IPatternUst result;
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

        public IPatternUst VisitArgs([NotNull] DslParser.ArgsContext context)
        {
            IEnumerable<PatternBase> expressions =
                context.arg().Select(arg => (PatternBase)VisitArg(arg));

            var textSpan = context.GetTextSpan();
            PatternArgs result;
            if (expressions.All(expr => !(expr is PatternMultipleExpressions)))
            {
                result = new PatternArgs(expressions) { TextSpan = textSpan };
            }
            else
            {
                result = new PatternArgs(expressions)
                {
                    TextSpan = textSpan
                };
            }
            return result;
        }

        public IPatternUst VisitArg([NotNull] DslParser.ArgContext context)
        {
            if (context.expression() != null)
            {
                return (PatternBase)VisitExpression(context.expression());
            }
            else
            {
                return new PatternMultipleExpressions() { TextSpan = context.GetTextSpan() };
            }
        }

        public IPatternUst VisitLiteral(DslParser.LiteralContext context)
        {
            PatternBase result;
            var textSpan = context.GetTextSpan();
            if (context.Id() != null)
            {
                result = ProcessId(context.Id());
            }
            else if (context.String() != null)
            {
                result = new PatternStringLiteral(RemoveQuotes(context.GetText()), textSpan);
            }
            else if (context.Oct() != null)
            {
                result = new PatternIntLiteral(
                    System.Convert.ToInt64(context.Oct().GetText(), 8), textSpan);
            }
            else if (context.Int() != null)
            {
                result = new PatternIntLiteral(long.Parse(context.Int().GetText()), textSpan);
            }
            else if (context.Hex() != null)
            {
                result = new PatternIntLiteral(
                    System.Convert.ToInt64(context.Hex().GetText(), 16), textSpan);
            }
            else if (context.Bool() != null)
            {
                result = new PatternBooleanLiteral(bool.Parse(context.Bool().GetText()), textSpan);
            }
            else if (context.Null() != null)
            {
                result = new PatternNullLiteral(textSpan);
            }
            else
            {
                throw new NotImplementedException();
            }
            return result;
        }

        public IPatternUst VisitLiteralOrPatternId(DslParser.LiteralOrPatternIdContext context)
        {
            PatternBase result;
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

        public IPatternUst VisitPatternLiterals([NotNull] DslParser.PatternLiteralsContext context)
        {
            IEnumerable<PatternBase> values = context.patternNotLiteral()
                .Select(literal => (PatternBase)VisitPatternNotLiteral(literal));
            var patternOr = new PatternOr(values, values.GetTextSpan());

            PatternBase result;
            if (context.PatternVar() != null)
            {
                string id = context.PatternVar().GetText().Substring(1);
                if (values.Count() > 0 && patternVars.TryGetValue(id, out PatternVar existedPatternVar))
                {
                    var lcTextSpan = new LineColumnTextSpan(existedPatternVar.TextSpan, Data);
                    throw new ConversionException(
                            $"DSL Error: PatternVar {id} with the same Id already defined earlier at {lcTextSpan}")
                    {
                        TextSpan = context.PatternVar().GetTextSpan()
                    };
                }
                var patternVar = new PatternVar(id, context.GetTextSpan());
                patternVar.Value = patternOr;
                result = patternVar;
            }
            else
            {
                result = patternOr;
            }

            return result;
        }

        public IPatternUst VisitPatternNotLiteral([NotNull] DslParser.PatternNotLiteralContext context)
        {
            var patternLiteral = (PatternBase)VisitPatternLiteral(context.patternLiteral());
            PatternBase result;
            if (context.PatternNot() != null)
            {
                result = new PatternNot(patternLiteral, context.GetTextSpan());
            }
            else
            {
                result = patternLiteral;
            }
            return result;
        }

        public IPatternUst VisitPatternLiteral(DslParser.PatternLiteralContext context)
        {
            return context.Accept(this);
        }

        public IPatternUst VisitPatternInt([NotNull] DslParser.PatternIntContext context)
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
            return new PatternIntLiteral(resultValue, context.GetTextSpan());
        }

        public IPatternUst VisitPatternStringLiteral(DslParser.PatternStringLiteralContext context)
        {
            string value = RemoveQuotes(context.GetText());
            return new PatternStringRegexLiteral(value == "" ? ".*" : value, context.GetTextSpan());
        }

        public IPatternUst VisitPatternIdToken(DslParser.PatternIdTokenContext context)
        {
            return (PatternIdRegexToken)VisitPatternId(context.patternId());
        }

        public IPatternUst VisitPatternIntLiteral(DslParser.PatternIntLiteralContext context)
        {
            PatternBase result;
            if (context.i != null)
            {
                result = (PatternIntLiteral)VisitPatternIntExpression(context.i);
            }
            else
            {
                result = new PatternIntRangeLiteral(
                    context.i1 != null ? ((PatternIntLiteral)VisitPatternIntExpression(context.i1)).Value : long.MinValue,
                    context.i2 != null ? ((PatternIntLiteral)VisitPatternIntExpression(context.i2)).Value : long.MaxValue);
            }
            result.TextSpan = context.GetTextSpan();
            return result;
        }

        public IPatternUst VisitPatternIntExpression([NotNull] DslParser.PatternIntExpressionContext context)
        {
            PatternIntLiteral result;
            if (context.op != null)
            {
                long leftValue = ((PatternIntLiteral)VisitPatternIntExpression(context.left)).Value;
                long rightValue = ((PatternIntLiteral)VisitPatternIntExpression(context.right)).Value;
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
                result = new PatternIntLiteral(resultValue, context.GetTextSpan());
            }
            else
            {
                result = (PatternIntLiteral)VisitPatternInt(context.patternInt());
            }
            return result;
        }

        public IPatternUst VisitPatternBoolLiteral(DslParser.PatternBoolLiteralContext context)
        {
            var boolText = context.PatternBool().GetText();
            var result = new PatternBooleanLiteral(boolText == "bool" ? (bool?)null : bool.Parse(boolText));
            result.TextSpan = context.GetTextSpan();
            return result;
        }

        public IPatternUst VisitPatternNullLiteral(DslParser.PatternNullLiteralContext context)
        {
            return new PatternNullLiteral(context.GetTextSpan());
        }

        public IPatternUst VisitPatternId([NotNull] DslParser.PatternIdContext context)
        {
            string patternId = context.GetText();
            var result = new PatternIdRegexToken(patternId, context.GetTextSpan());
            return result;
        }

        public IPatternUst Visit(IParseTree tree)
        {
            throw new ShouldNotBeVisitedException("DSL node");
        }

        public IPatternUst VisitChildren(IRuleNode node)
        {
            throw new ShouldNotBeVisitedException("DSL children");
        }

        public IPatternUst VisitTerminal(ITerminalNode node)
        {
            throw new ShouldNotBeVisitedException("DSL Terminal");
        }

        public IPatternUst VisitErrorNode(IErrorNode node)
        {
            throw new ShouldNotBeVisitedException("DSL Error");
        }

        private string RemoveQuotes(string str)
        {
            return str.Substring(1, str.Length - 2).Replace("\\\"", "\"");
        }

        private PatternBase ProcessId(ITerminalNode idTerminal)
        {
            var result = new PatternIdToken(idTerminal.GetText(), idTerminal.GetTextSpan());
            return result;
        }

        private IEnumerable<PatternBase> ProcessLiteralsOrPatternIds(
            IList<DslParser.LiteralOrPatternIdContext> literalsOrPatternIds)
        {
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
            });
        }

        private PatternBase ProcessPatternIds(IList<DslParser.PatternIdContext> contexts)
        {
            PatternBase result;
            var firstPatternId = contexts.First();
            if (contexts.Count == 1)
            {
                result = (PatternBase)VisitPatternId(firstPatternId);
            }
            else
            {
                IEnumerable<PatternBase> values =
                    contexts.Select(literal => (PatternBase)VisitPatternId(literal));
                result = new PatternOr(values, firstPatternId.GetTextSpan());
            }
            return result;
        }
    }
}