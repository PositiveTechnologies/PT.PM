using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Dsl
{
    public class DslUstConverter : IDslParserVisitor<PatternUst>
    {
        private const string DslHelperPrefix = "pt.pm_";
        private Dictionary<string, PatternVar> patternVars;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool PatternExpressionInsideStatement { get; set; } = true;

        public string Data { get; set; }

        public PatternRoot Convert(DslParser.PatternContext pattern)
        {
            try
            {
                patternVars = new Dictionary<string, PatternVar>();
                var result = new PatternRoot();
                result.Node = VisitPattern(pattern);
                result.Languages = new HashSet<Language>(LanguageUtils.PatternLanguages.Values);
                var ascendantsFiller = new PatternAscendantsFiller(result);
                ascendantsFiller.FillAscendants();
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException("Pattern", ex) { IsPattern = true });
                throw;
            }
        }

        public PatternUst VisitPattern(DslParser.PatternContext context)
        {
            return VisitDslCode(context.dslCode(0));
        }

        public PatternUst VisitDslCode(DslParser.DslCodeContext context)
        {
            PatternUst result;
            if (context.statement().Length > 0)
            {
                IEnumerable<PatternUst> statements = context.statement().Select(
                    statement => VisitStatement(statement))
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

        public PatternUst VisitStatement(DslParser.StatementContext context)
        {
            return context.Accept(this);
        }

        public PatternUst VisitExpressionStatement(DslParser.ExpressionStatementContext context)
        {
            PatternUst result = VisitExpression(context.expression());
            if (!PatternExpressionInsideStatement)
            {
                if (context.PatternNot() != null)
                {
                    result = new PatternNot(result);
                }
            }
            else
            {
                result = new PatternArbitraryDepth(result, context.GetTextSpan());
                if (context.PatternNot() != null)
                {
                    result = new PatternNot(result);
                }
            }
            return result;
        }

        public PatternUst VisitPatternStatement(DslParser.PatternStatementContext context)
        {
            return new PatternAnyExpression(context.GetTextSpan());
        }

        public PatternUst VisitPatternMultipleStatement(DslParser.PatternMultipleStatementContext context)
        {
            return null;
        }

        public PatternUst VisitPatternTryCatchStatement(DslParser.PatternTryCatchStatementContext context)
        {
            IEnumerable<PatternUst> exceptionTypes = ProcessLiteralsOrPatternIds(context.literalOrPatternId());
            bool isCatchBodyEmpty = context.Ellipsis() == null;
            var result = new PatternTryCatchStatement(exceptionTypes, isCatchBodyEmpty, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitExpression(DslParser.ExpressionContext context)
        {
            return context.Accept(this);
        }

        public PatternUst VisitPatternOrExpression(DslParser.PatternOrExpressionContext context)
        {
            IEnumerable<PatternUst> values = context.expression().Select(expr => VisitExpression(expr));
            var result = new PatternOr(values, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitPatternAndExpression(DslParser.PatternAndExpressionContext context)
        {
            IEnumerable<PatternUst> values = context.expression().Select(expr => VisitExpression(expr));
            return new PatternAnd(values, context.GetTextSpan());
        }

        public PatternUst VisitPatternNotExpression(DslParser.PatternNotExpressionContext context)
        {
            var expression = (PatternUst)VisitExpression(context.expression());
            return new PatternNot(expression, context.GetTextSpan());
        }

        public PatternUst VisitClassDeclaration(DslParser.ClassDeclarationContext context)
        {
            IEnumerable<PatternUst> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            IEnumerable<PatternUst> baseTypes = ProcessLiteralsOrPatternIds(context._baseTypes);

            PatternUst name = null;
            if (context.name != null)
            {
                name = VisitLiteralOrPatternId(context.name);
            }

            PatternArbitraryDepth body = null;
            var arbitraryDepthExpression = context.arbitraryDepthExpression();
            if (arbitraryDepthExpression != null)
            {
                body = (PatternArbitraryDepth)VisitArbitraryDepthExpression(arbitraryDepthExpression);
            }

            return new PatternClassDeclaration(modifiers, name, baseTypes, body, context.GetTextSpan());
        }

        public PatternUst VisitMethodDeclaration(DslParser.MethodDeclarationContext context)
        {
            PatternUst result;
            IEnumerable<PatternUst> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            PatternUst name = VisitLiteralOrPatternId(context.methodName);
            var bodyExpression = context.expression();
            if (bodyExpression != null)
            {
                PatternUst body = VisitExpression(bodyExpression);
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

        public PatternUst VisitVarOrFieldDeclarationExpression(DslParser.VarOrFieldDeclarationExpressionContext context)
        {
            bool localVariable = context.Field() == null;
            PatternUst typeLiteralOrPatternId = VisitLiteralOrPatternId(context.type);
            PatternUst type = typeLiteralOrPatternId is PatternIdRegexToken ?
                typeLiteralOrPatternId :
                new PatternIdToken(typeLiteralOrPatternId.ToString(), typeLiteralOrPatternId.TextSpan);
            PatternUst name = VisitVariableName(context.variableName());
            IEnumerable<PatternUst> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            var assignment = new PatternAssignmentExpression(name, null, name.TextSpan);
            var result = new PatternVarOrFieldDeclaration(localVariable, modifiers, type, assignment, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitInvocationExpression(DslParser.InvocationExpressionContext context)
        {
            PatternArgs args = context.args() == null
                ? new PatternArgs()
                : (PatternArgs)VisitArgs(context.args());
            PatternUst expr = VisitExpression(context.expression());
            var result = new PatternInvocationExpression(expr, args, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitMemberReferenceExpression(DslParser.MemberReferenceExpressionContext context)
        {
            PatternUst target = VisitExpression(context.expression());
            PatternUst type = VisitLiteralOrPatternId(context.literalOrPatternId());
            var result = new PatternMemberReferenceExpression(target, type, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitBinaryOperatorExpression(DslParser.BinaryOperatorExpressionContext context)
        {
            PatternUst left = VisitExpression(context.expression(0));
            var literal = new PatternBinaryOperatorLiteral(context.op.Text, context.op.GetTextSpan());
            PatternUst right = VisitExpression(context.expression(1));
            var textSpan = context.GetTextSpan();

            var result = new PatternBinaryOperatorExpression(left, literal, right, textSpan);
            return result;
        }

        public PatternUst VisitIndexerExpression(DslParser.IndexerExpressionContext context)
        {
            PatternUst target = VisitExpression(context.expression(0));
            var args = new PatternArgs(VisitExpression(context.expression(1)));

            var result = new PatternIndexerExpression(target, args, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitMemberReferenceOrLiteralExpression(DslParser.MemberReferenceOrLiteralExpressionContext context)
        {
            var textSpan = context.GetTextSpan();
            var values = new PatternUst[]
            {
                new PatternMemberReferenceExpression((PatternUst)VisitExpression(context.expression()),
                    VisitLiteralOrPatternId(context.literalOrPatternId()), textSpan),
                    VisitLiteralOrPatternId(context.literalOrPatternId())
            };
            var result = new PatternOr(values, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitAssignmentExpression(DslParser.AssignmentExpressionContext context)
        {
            PatternUst result;
            PatternUst left = VisitExpression(context.expression(0));
            PatternUst right = VisitExpression(context.expression(1));
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

        public PatternUst VisitComparisonExpression([NotNull] DslParser.ComparisonExpressionContext context)
        {
            PatternUst left = VisitExpression(context.expression(0));
            PatternUst right = VisitExpression(context.expression(1));
            var opLiteral = new PatternBinaryOperatorLiteral(context.op.Text, context.op.GetTextSpan());
            var result = new PatternBinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitObjectCreationExpression(DslParser.ObjectCreationExpressionContext context)
        {
            PatternUst literal = VisitLiteralOrPatternId(context.literalOrPatternId());
            var typeToken = new PatternIdToken(literal.ToString(), literal.TextSpan);
            PatternArgs args = context.args() == null
                ? new PatternArgs()
                : (PatternArgs)VisitArgs(context.args());
            var result = new PatternObjectCreateExpression(typeToken, args, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitFunctionExpression([NotNull] DslParser.FunctionExpressionContext context)
        {
            return VisitExpression(context.expression()); // TODO: remove FunctionExpression from DSL.
        }

        public PatternUst VisitPatternLiteralExpression(DslParser.PatternLiteralExpressionContext context)
        {
            return VisitPatternLiterals(context.patternLiterals());
        }

        public PatternUst VisitLiteralExpression(DslParser.LiteralExpressionContext context)
        {
            return VisitLiteral(context.literal());
        }

        public PatternUst VisitPatternExpression(DslParser.PatternExpressionContext context)
        {
            return new PatternAnyExpression(context.GetTextSpan());
        }

        public PatternUst VisitParenthesisExpression([NotNull] DslParser.ParenthesisExpressionContext context)
        {
            return VisitExpression(context.expression());
        }

        public PatternUst VisitPatternArbitraryDepthExpression([NotNull] DslParser.PatternArbitraryDepthExpressionContext context)
        {
            return VisitArbitraryDepthExpression(context.arbitraryDepthExpression());
        }

        public PatternUst VisitArbitraryDepthExpression([NotNull] DslParser.ArbitraryDepthExpressionContext context)
        {
            return new PatternArbitraryDepth(VisitExpression(context.expression()), context.GetTextSpan());
        }

        public PatternUst VisitPatternReturnExpression(DslParser.PatternReturnExpressionContext context)
        {
            PatternUst expression = VisitExpression(context.expression());
            var result = new PatternReturn(expression, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitPatternThrowExpression(DslParser.PatternThrowExpressionContext context)
        {
            PatternUst expression = VisitExpression(context.expression());
            var result = new PatternThrow(expression, context.GetTextSpan());
            return result;
        }

        public PatternUst VisitBaseReferenceExpression(DslParser.BaseReferenceExpressionContext context)
        {
            return new PatternBaseReferenceToken(context.GetTextSpan());
        }

        public PatternUst VisitVariableName(DslParser.VariableNameContext context)
        {
            PatternUst result;
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

        public PatternUst VisitArgs([NotNull] DslParser.ArgsContext context)
        {
            IEnumerable<PatternUst> expressions =
                context.arg().Select(arg => (PatternUst)VisitArg(arg));

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

        public PatternUst VisitArg([NotNull] DslParser.ArgContext context)
        {
            if (context.expression() != null)
            {
                return VisitExpression(context.expression());
            }
            else
            {
                return new PatternMultipleExpressions() { TextSpan = context.GetTextSpan() };
            }
        }

        public PatternUst VisitLiteral(DslParser.LiteralContext context)
        {
            PatternUst result;
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

        public PatternUst VisitLiteralOrPatternId(DslParser.LiteralOrPatternIdContext context)
        {
            PatternUst result;
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

        public PatternUst VisitPatternLiterals([NotNull] DslParser.PatternLiteralsContext context)
        {
            IEnumerable<PatternUst> values = context.patternNotLiteral()
                .Select(literal => VisitPatternNotLiteral(literal));
            var patternOr = values.Count() > 1
                ? new PatternOr(values)
                : values.Count() == 1
                ? values.First()
                : new PatternIdRegexToken();

            PatternUst result;
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
                patternVars[id] = patternVar;
                patternVar.Value = patternOr;
                result = patternVar;
            }
            else
            {
                result = patternOr;
            }

            return result;
        }

        public PatternUst VisitPatternNotLiteral([NotNull] DslParser.PatternNotLiteralContext context)
        {
            PatternUst patternLiteral = VisitPatternLiteral(context.patternLiteral());
            PatternUst result;
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

        public PatternUst VisitPatternLiteral(DslParser.PatternLiteralContext context)
        {
            return context.Accept(this);
        }

        public PatternUst VisitPatternInt([NotNull] DslParser.PatternIntContext context)
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

        public PatternUst VisitPatternStringLiteral(DslParser.PatternStringLiteralContext context)
        {
            string value = RemoveQuotes(context.GetText());
            return new PatternStringRegexLiteral(value == "" ? ".*" : value, context.GetTextSpan());
        }

        public PatternUst VisitPatternIdToken(DslParser.PatternIdTokenContext context)
        {
            return (PatternIdRegexToken)VisitPatternId(context.patternId());
        }

        public PatternUst VisitPatternIntLiteral(DslParser.PatternIntLiteralContext context)
        {
            PatternUst result;
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

        public PatternUst VisitPatternIntExpression([NotNull] DslParser.PatternIntExpressionContext context)
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

        public PatternUst VisitPatternBoolLiteral(DslParser.PatternBoolLiteralContext context)
        {
            var boolText = context.PatternBool().GetText();
            var result = new PatternBooleanLiteral(boolText == "bool" ? (bool?)null : bool.Parse(boolText));
            result.TextSpan = context.GetTextSpan();
            return result;
        }

        public PatternUst VisitPatternNullLiteral(DslParser.PatternNullLiteralContext context)
        {
            return new PatternNullLiteral(context.GetTextSpan());
        }

        public PatternUst VisitPatternId([NotNull] DslParser.PatternIdContext context)
        {
            string patternId = context.GetText();
            var result = new PatternIdRegexToken(patternId, context.GetTextSpan());
            return result;
        }

        public PatternUst Visit(IParseTree tree)
        {
            throw new ShouldNotBeVisitedException("DSL node");
        }

        public PatternUst VisitChildren(IRuleNode node)
        {
            throw new ShouldNotBeVisitedException("DSL children");
        }

        public PatternUst VisitTerminal(ITerminalNode node)
        {
            throw new ShouldNotBeVisitedException("DSL Terminal");
        }

        public PatternUst VisitErrorNode(IErrorNode node)
        {
            throw new ShouldNotBeVisitedException("DSL Error");
        }

        private string RemoveQuotes(string str)
        {
            return str.Substring(1, str.Length - 2).Replace("\\\"", "\"");
        }

        private PatternUst ProcessId(ITerminalNode idTerminal)
        {
            var result = new PatternIdToken(idTerminal.GetText(), idTerminal.GetTextSpan());
            return result;
        }

        private IEnumerable<PatternUst> ProcessLiteralsOrPatternIds(
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

        private PatternUst ProcessPatternIds(IList<DslParser.PatternIdContext> contexts)
        {
            PatternUst result;
            var firstPatternId = contexts.First();
            if (contexts.Count == 1)
            {
                result = VisitPatternId(firstPatternId);
            }
            else
            {
                IEnumerable<PatternUst> values =
                    contexts.Select(literal => VisitPatternId(literal));
                result = new PatternOr(values, firstPatternId.GetTextSpan());
            }
            return result;
        }
    }
}