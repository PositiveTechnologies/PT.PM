using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching
{
    public class PatternNormalizer : IUstPatternVisitor<PatternBase>, ILoggable
    {
        private static PropertyEnumerator<PatternBase> propertyEnumerator = new PropertyEnumerator<PatternBase>
        {
            IgnoredProperties = new HashSet<string>() { nameof(Ust.Parent), nameof(Ust.Root) }
        };

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public PatternRootUst Normalize(PatternRootUst patternUst)
        {
            var newPattern = new PatternRootUst(patternUst.SourceCodeFile);

            newPattern.SourceCodeFile = patternUst.SourceCodeFile;
            newPattern.Nodes = patternUst.Nodes.Select(node => Visit((PatternBase)node)).ToArray();
            newPattern.Languages = new HashSet<Language>(patternUst.Languages);

            newPattern.FillAscendants();
            return newPattern;
        }

        public PatternBase Visit(PatternBase patternBase)
        {
            if (patternBase == null)
            {
                return null;
            }
            return Visit((dynamic)patternBase);
        }

        public PatternBase Visit(PatternArgs patternExpressions)
        {
            // #* #* ... #* -> #*
            List<PatternBase> args = patternExpressions.Args
                .Select(item => (PatternBase)Visit(item)).ToList();
            int index = 0;
            while (index < args.Count)
            {
                if (args[index] is PatternMultipleExpressions &&
                    index + 1 < args.Count &&
                    args[index + 1] is PatternMultipleExpressions)
                {
                    args.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
            var result = new PatternArgs(args);
            return result;
        }

        public PatternBase Visit(PatternStatements patternStatements)
        {
            return VisitChildren(patternStatements);
        }

        public PatternBase Visit(PatternOr patternOr)
        {
            if (patternOr.Patterns.Count == 1)
            {
                return Visit(patternOr.Patterns[0]);
            }

            IEnumerable<PatternBase> exprs = patternOr.Patterns
                .Select(e => Visit(e))
                .OrderBy(e => e);
            return new PatternOr(exprs, patternOr.TextSpan);
        }

        public PatternBase Visit(PatternBooleanLiteral patternBooleanLiteral)
        {
            return VisitChildren(patternBooleanLiteral);
        }

        public PatternBase Visit(PatternCommentRegex patternComment)
        {
            return VisitChildren(patternComment);
        }

        public PatternBase Visit(PatternAnyExpression patternAnyExpression)
        {
            return VisitChildren(patternAnyExpression);
        }

        public PatternBase Visit(PatternArbitraryDepth patternArbitraryDepthExpression)
        {
            return VisitChildren(patternArbitraryDepthExpression);
        }

        public PatternBase Visit(PatternIdToken patternIdToken)
        {
            return VisitChildren(patternIdToken);
        }

        public PatternBase Visit(PatternIdRegexToken patternIdRegexToken)
        {
            string regexString = patternIdRegexToken.IdRegex.ToString();

            if (regexString.StartsWith("(?i)"))
            {
                regexString = regexString.Substring("(?i)".Length);
                Regex newRegex = new Regex(
                    string.IsNullOrEmpty(regexString) ? @"\w+" : regexString,
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return new PatternIdRegexToken(newRegex, patternIdRegexToken.TextSpan);
            }

            if (regexString.StartsWith("^") && regexString.EndsWith("$"))
            {
                string newRegexString = regexString.Substring(1, regexString.Length - 2);
                if (newRegexString.All(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    return new PatternIdToken(
                        newRegexString,
                        patternIdRegexToken.TextSpan);
                }
            }

            return new PatternIdRegexToken(regexString, patternIdRegexToken.TextSpan);
        }

        public PatternBase Visit(PatternIntRangeLiteral patternIntLiteral)
        {
            if (patternIntLiteral.MinValue == patternIntLiteral.MaxValue)
            {
                return new PatternIntLiteral(
                    patternIntLiteral.MinValue,
                    patternIntLiteral.TextSpan);
            }
            else
            {
                return new PatternIntRangeLiteral(
                    patternIntLiteral.MinValue,
                    patternIntLiteral.MaxValue,
                    patternIntLiteral.TextSpan);
            }
        }

        public PatternBase Visit(PatternMultipleExpressions patternMultiExpressions)
        {
            return VisitChildren(patternMultiExpressions);
        }

        public PatternBase Visit(PatternStringRegexLiteral patternStringLiteral)
        {
            return VisitChildren(patternStringLiteral);
        }

        public PatternBase Visit(PatternTryCatchStatement patternTryCatchStatement)
        {
            return VisitChildren(patternTryCatchStatement);
        }

        public PatternBase Visit(PatternAnd patternAnd)
        {
            if (patternAnd.Patterns.Count == 1)
            {
                return Visit(patternAnd.Patterns[0]);
            }

            IEnumerable<PatternBase> exprs = patternAnd.Patterns
                .Select(e => (PatternBase)Visit(e))
                .OrderBy(e => e);
            return new PatternAnd(exprs, patternAnd.TextSpan);
        }

        public PatternBase Visit(PatternAny patternAny)
        {
            return VisitChildren(patternAny);
        }

        public PatternBase Visit(PatternNot patternNot)
        {
            if (patternNot.Pattern is PatternNot innerPatternNot)
            {
                return Visit(innerPatternNot.Pattern);
            }

            return VisitChildren(patternNot);
        }

        public PatternBase Visit(PatternClassDeclaration patternClassDeclaration)
        {
            return VisitChildren(patternClassDeclaration);
        }

        public PatternBase Visit(PatternMethodDeclaration patternMethodDeclaration)
        {
            return VisitChildren(patternMethodDeclaration);
        }

        public PatternBase Visit(PatternVarOrFieldDeclaration patternVarOrFieldDeclaration)
        {
            return VisitChildren(patternVarOrFieldDeclaration);
        }

        public PatternBase Visit(PatternVar patternVar)
        {
            return VisitChildren(patternVar);
        }

        public PatternBase Visit(PatternAssignmentExpression patternAssignmentExpression)
        {
            return VisitChildren(patternAssignmentExpression);
        }

        public PatternBase Visit(PatternBaseReferenceExpression patternBaseReferenceExpression)
        {
            return VisitChildren(patternBaseReferenceExpression);
        }

        public PatternBase Visit(PatternBinaryOperatorExpression patternBinaryOperatorExpression)
        {
            return VisitChildren(patternBinaryOperatorExpression);
        }

        public PatternBase Visit(PatternBinaryOperatorLiteral patternBinaryOperatorLiteral)
        {
            return VisitChildren(patternBinaryOperatorLiteral);
        }

        public PatternBase Visit(PatternIndexerExpression patternIndexerExpression)
        {
            return VisitChildren(patternIndexerExpression);
        }

        public PatternBase Visit(PatternIntLiteral patternIntLiteral)
        {
            return VisitChildren(patternIntLiteral);
        }

        public PatternBase Visit(PatternInvocationExpression patternInvocationExpression)
        {
            return VisitChildren(patternInvocationExpression);
        }

        public PatternBase Visit(PatternMemberReferenceExpression patternMemberReferenceExpression)
        {
            return VisitChildren(patternMemberReferenceExpression);
        }

        public PatternBase Visit(PatternNullLiteral patternNullLiteral)
        {
            return VisitChildren(patternNullLiteral);
        }

        public PatternBase Visit(PatternObjectCreateExpression patternObjectCreateExpression)
        {
            return VisitChildren(patternObjectCreateExpression);
        }

        public PatternBase Visit(PatternParameterDeclaration patternParameterDeclaration)
        {
            return VisitChildren(patternParameterDeclaration);
        }

        public PatternBase Visit(PatternStringLiteral patternStringLiteral)
        {
            return VisitChildren(patternStringLiteral);
        }

        public PatternBase VisitChildren(PatternBase patternBase)
        {
            try
            {
                return propertyEnumerator.Clone(patternBase, Visit);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException(patternBase.Root?.SourceCodeFile?.FullPath ?? "", ex)
                {
                    TextSpan = patternBase.TextSpan,
                    IsPattern = true
                });
                return null;
            }
        }
    }
}
