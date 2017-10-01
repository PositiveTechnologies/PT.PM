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
    public class DslUstConverter : IDslParserVisitor<PatternBase>
    {
        private const string DslHelperPrefix = "pt.pm_";
        private Dictionary<string, PatternVar> patternVars;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public HashSet<Language> Languages { get; set; }

        public bool PatternExpressionInsideStatement { get; set; } = true;

        public string Data { get; set; }

        public PatternRoot Convert(DslParser.PatternContext pattern)
        {
            try
            {
                patternVars = new Dictionary<string, PatternVar>();
                var result = new PatternRoot();
                result.Node = VisitPattern(pattern);
                result.Languages = new HashSet<Language>(LanguageExt.AllPatternLanguages);
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

        public PatternBase VisitPattern(DslParser.PatternContext context)
        {
            return VisitDslCode(context.dslCode(0));
        }

        public PatternBase VisitDslCode(DslParser.DslCodeContext context)
        {
            PatternBase result;
            if (context.statement().Length > 0)
            {
                IEnumerable<PatternBase> statements = context.statement().Select(
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

        public PatternBase VisitStatement(DslParser.StatementContext context)
        {
            return context.Accept(this);
        }

        public PatternBase VisitExpressionStatement(DslParser.ExpressionStatementContext context)
        {
            PatternBase result = VisitExpression(context.expression());
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

        public PatternBase VisitPatternStatement(DslParser.PatternStatementContext context)
        {
            return new PatternAnyExpression(context.GetTextSpan());
        }

        public PatternBase VisitPatternMultipleStatement(DslParser.PatternMultipleStatementContext context)
        {
            return null;
        }

        public PatternBase VisitPatternTryCatchStatement(DslParser.PatternTryCatchStatementContext context)
        {
            IEnumerable<PatternBase> exceptionTypes = ProcessLiteralsOrPatternIds(context.literalOrPatternId());
            bool isCatchBodyEmpty = context.Ellipsis() == null;
            var result = new PatternTryCatchStatement(exceptionTypes, isCatchBodyEmpty, context.GetTextSpan());
            return result;
        }

        public PatternBase VisitExpression(DslParser.ExpressionContext context)
        {
            return context.Accept(this);
        }

        public PatternBase VisitPatternOrExpression(DslParser.PatternOrExpressionContext context)
        {
            IEnumerable<PatternBase> values = context.expression().Select(expr => VisitExpression(expr));
            var result = new PatternOr(values, context.GetTextSpan());
            return result;
        }

        public PatternBase VisitPatternAndExpression(DslParser.PatternAndExpressionContext context)
        {
            IEnumerable<PatternBase> values = context.expression().Select(expr => VisitExpression(expr));
            return new PatternAnd(values, context.GetTextSpan());
        }

        public PatternBase VisitPatternNotExpression(DslParser.PatternNotExpressionContext context)
        {
            var expression = (PatternBase)VisitExpression(context.expression());
            return new PatternNot(expression, context.GetTextSpan());
        }

        public PatternBase VisitClassDeclaration(DslParser.ClassDeclarationContext context)
        {
            IEnumerable<PatternBase> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            IEnumerable<PatternBase> baseTypes = ProcessLiteralsOrPatternIds(context._baseTypes);

            PatternBase name = null;
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

        public PatternBase VisitMethodDeclaration(DslParser.MethodDeclarationContext context)
        {
            PatternBase result;
            IEnumerable<PatternBase> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            PatternBase name = VisitLiteralOrPatternId(context.methodName);
            var arbitraryDepthExpression = context.arbitraryDepthExpression();
            if (arbitraryDepthExpression != null)
            {
                var body = (PatternArbitraryDepth)VisitArbitraryDepthExpression(arbitraryDepthExpression);
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

        public PatternBase VisitVarOrFieldDeclarationExpression(DslParser.VarOrFieldDeclarationExpressionContext context)
        {
            bool localVariable = context.Field() == null;
            PatternBase typeLiteralOrPatternId = VisitLiteralOrPatternId(context.type);
            PatternBase type = typeLiteralOrPatternId is PatternIdRegexToken ?
                typeLiteralOrPatternId :
                new PatternIdToken(typeLiteralOrPatternId.ToString(), typeLiteralOrPatternId.TextSpan);
            PatternBase name = VisitVariableName(context.variableName());
            IEnumerable<PatternBase> modifiers = ProcessLiteralsOrPatternIds(context._modifiers);
            var assignment = new PatternAssignmentExpression(name, null, name.TextSpan);
            var result = new PatternVarOrFieldDeclaration(localVariable, modifiers, type, assignment, context.GetTextSpan());
            return result;
        }

        public PatternBase VisitInvocationExpression(DslParser.InvocationExpressionContext context)
        {
            PatternArgs args = context.args() == null
                ? new PatternArgs()
                : (PatternArgs)VisitArgs(context.args());
            PatternBase expr = VisitExpression(context.expression());
            var result = new PatternInvocationExpression(expr, args, context.GetTextSpan());
            return result;
        }

        public PatternBase VisitMemberReferenceExpression(DslParser.MemberReferenceExpressionContext context)
        {
            PatternBase target = VisitExpression(context.expression());
            PatternBase type = VisitLiteralOrPatternId(context.literalOrPatternId());
            var result = new PatternMemberReferenceExpression(target, type, context.GetTextSpan());
            return result;
        }

        public PatternBase VisitBinaryOperatorExpression(DslParser.BinaryOperatorExpressionContext context)
        {
            PatternBase left = VisitExpression(context.expression(0));
            var literal = new PatternBinaryOperatorLiteral(context.op.Text, context.op.GetTextSpan());
            PatternBase right = VisitExpression(context.expression(1));
            var textSpan = context.GetTextSpan();

            var result = new PatternBinaryOperatorExpression(left, literal, right, textSpan);
            return result;
        }

        public PatternBase VisitIndexerExpression(DslParser.IndexerExpressionContext context)
        {
            PatternBase target = VisitExpression(context.expression(0));
            var args = new PatternArgs(VisitExpression(context.expression(1)));

            var result = new PatternIndexerExpression(target, args, context.GetTextSpan());
            return result;
        }

        public PatternBase VisitMemberReferenceOrLiteralExpression(DslParser.MemberReferenceOrLiteralExpressionContext context)
        {
            var textSpan = context.GetTextSpan();
            var values = new PatternBase[]
            {
                new PatternMemberReferenceExpression((PatternBase)VisitExpression(context.expression()),
                    VisitLiteralOrPatternId(context.literalOrPatternId()), textSpan),
                    VisitLiteralOrPatternId(context.literalOrPatternId())
            };
            var result = new PatternOr(values, context.GetTextSpan());
            return result;
        }

        public PatternBase VisitAssignmentExpression(DslParser.AssignmentExpressionContext context)
        {
            PatternBase result;
            PatternBase left = VisitExpression(context.expression(0));
            PatternBase right = VisitExpression(context.expression(1));
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

        public PatternBase VisitComparisonExpression([NotNull] DslParser.ComparisonExpressionContext context)
        {
            PatternBase left = VisitExpression(context.expression(0));
            PatternBase right = VisitExpression(context.expression(1));
            var opLiteral = new PatternBinaryOperatorLiteral(context.op.Text, context.op.GetTextSpan());
            var result = new PatternBinaryOperatorExpression(left, opLiteral, right, context.GetTextSpan());
            return result;
        }

        public PatternBase VisitObjectCreationExpression(DslParser.ObjectCreationExpressionContext context)
        {
            PatternBase literal = VisitLiteralOrPatternId(context.literalOrPatternId());
            var typeToken = new PatternIdToken(literal.ToString(), literal.TextSpan);
            PatternArgs args = context.args() == null
                ? new PatternArgs()
                : (PatternArgs)VisitArgs(context.args());
            var result = new PatternObjectCreateExpression(typeToken, args, context.GetTextSpan());
            return result;
        }

        public PatternBase VisitFunctionExpression([NotNull] DslParser.FunctionExpressionContext context)
        {
            return VisitExpression(context.expression()); // TODO: remove FunctionExpression from DSL.
        }

        public PatternBase VisitPatternLiteralExpression(DslParser.PatternLiteralExpressionContext context)
        {
            return VisitPatternLiterals(context.patternLiterals());
        }

        public PatternBase VisitLiteralExpression(DslParser.LiteralExpressionContext context)
        {
            return VisitLiteral(context.literal());
        }

        public PatternBase VisitPatternExpression(DslParser.PatternExpressionContext context)
        {
            return new PatternAnyExpression(context.GetTextSpan());
        }

        public PatternBase VisitParenthesisExpression([NotNull] DslParser.ParenthesisExpressionContext context)
        {
            return VisitExpression(context.expression());
        }

        public PatternBase VisitPatternArbitraryDepthExpression([NotNull] DslParser.PatternArbitraryDepthExpressionContext context)
        {
            return VisitArbitraryDepthExpression(context.arbitraryDepthExpression());
        }

        public PatternBase VisitArbitraryDepthExpression([NotNull] DslParser.ArbitraryDepthExpressionContext context)
        {
            return new PatternArbitraryDepth(VisitExpression(context.expression()), context.GetTextSpan());
        }

        public PatternBase VisitBaseReferenceExpression(DslParser.BaseReferenceExpressionContext context)
        {
            return new PatternBaseReferenceExpression(context.GetTextSpan());
        }

        public PatternBase VisitVariableName(DslParser.VariableNameContext context)
        {
            PatternBase result;
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

        public PatternBase VisitArgs([NotNull] DslParser.ArgsContext context)
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

        public PatternBase VisitArg([NotNull] DslParser.ArgContext context)
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

        public PatternBase VisitLiteral(DslParser.LiteralContext context)
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

        public PatternBase VisitLiteralOrPatternId(DslParser.LiteralOrPatternIdContext context)
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

        public PatternBase VisitPatternLiterals([NotNull] DslParser.PatternLiteralsContext context)
        {
            IEnumerable<PatternBase> values = context.patternNotLiteral()
                .Select(literal => VisitPatternNotLiteral(literal));
            var patternOr = values.Count() > 1
                ? new PatternOr(values)
                : values.Count() == 1
                ? values.First()
                : new PatternIdRegexToken();

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

        public PatternBase VisitPatternNotLiteral([NotNull] DslParser.PatternNotLiteralContext context)
        {
            PatternBase patternLiteral = VisitPatternLiteral(context.patternLiteral());
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

        public PatternBase VisitPatternLiteral(DslParser.PatternLiteralContext context)
        {
            return context.Accept(this);
        }

        public PatternBase VisitPatternInt([NotNull] DslParser.PatternIntContext context)
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

        public PatternBase VisitPatternStringLiteral(DslParser.PatternStringLiteralContext context)
        {
            string value = RemoveQuotes(context.GetText());
            return new PatternStringRegexLiteral(value == "" ? ".*" : value, context.GetTextSpan());
        }

        public PatternBase VisitPatternIdToken(DslParser.PatternIdTokenContext context)
        {
            return (PatternIdRegexToken)VisitPatternId(context.patternId());
        }

        public PatternBase VisitPatternIntLiteral(DslParser.PatternIntLiteralContext context)
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

        public PatternBase VisitPatternIntExpression([NotNull] DslParser.PatternIntExpressionContext context)
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

        public PatternBase VisitPatternBoolLiteral(DslParser.PatternBoolLiteralContext context)
        {
            var boolText = context.PatternBool().GetText();
            var result = new PatternBooleanLiteral(boolText == "bool" ? (bool?)null : bool.Parse(boolText));
            result.TextSpan = context.GetTextSpan();
            return result;
        }

        public PatternBase VisitPatternNullLiteral(DslParser.PatternNullLiteralContext context)
        {
            return new PatternNullLiteral(context.GetTextSpan());
        }

        public PatternBase VisitPatternId([NotNull] DslParser.PatternIdContext context)
        {
            string patternId = context.GetText();
            var result = new PatternIdRegexToken(patternId, context.GetTextSpan());
            return result;
        }

        public PatternBase Visit(IParseTree tree)
        {
            throw new ShouldNotBeVisitedException("DSL node");
        }

        public PatternBase VisitChildren(IRuleNode node)
        {
            throw new ShouldNotBeVisitedException("DSL children");
        }

        public PatternBase VisitTerminal(ITerminalNode node)
        {
            throw new ShouldNotBeVisitedException("DSL Terminal");
        }

        public PatternBase VisitErrorNode(IErrorNode node)
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
                result = VisitPatternId(firstPatternId);
            }
            else
            {
                IEnumerable<PatternBase> values =
                    contexts.Select(literal => VisitPatternId(literal));
                result = new PatternOr(values, firstPatternId.GetTextSpan());
            }
            return result;
        }
    }
}