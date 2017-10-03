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
    public class PatternNormalizer : PatternVisitor<PatternBase>, ILoggable
    {
        private static PropertyCloner<PatternBase> propertyEnumerator = new PropertyCloner<PatternBase>
        {
            IgnoredProperties = new HashSet<string>() { nameof(Ust.Parent), nameof(Ust.Root) }
        };

        public PatternRoot Normalize(PatternRoot pattern)
        {
            var newPattern = new PatternRoot
            {
                Logger = pattern.Logger,
                Key = pattern.Key,
                FilenameWildcard = pattern.FilenameWildcard,
                SourceCodeFile = pattern.SourceCodeFile,
                Languages = new HashSet<Language>(pattern.Languages),
                DataFormat = pattern.DataFormat,
                DebugInfo = pattern.DebugInfo,
                Node = Visit(pattern.Node),
            };
            var ascendantsFiller = new PatternAscendantsFiller(newPattern);
            ascendantsFiller.FillAscendants();
            return newPattern;
        }

        public override PatternBase Visit(PatternArgs patternExpressions)
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

        public override PatternBase Visit(PatternOr patternOr)
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

        public override PatternBase Visit(PatternIdRegexToken patternIdRegexToken)
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

        public override PatternBase Visit(PatternIntRangeLiteral patternIntLiteral)
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

        public override PatternBase Visit(PatternAnd patternAnd)
        {
            if (patternAnd.Patterns.Count == 1)
            {
                return Visit(patternAnd.Patterns[0]);
            }

            IEnumerable<PatternBase> exprs = patternAnd.Patterns
                .Select(e => Visit(e))
                .OrderBy(e => e);
            return new PatternAnd(exprs, patternAnd.TextSpan);
        }

        public override PatternBase Visit(PatternNot patternNot)
        {
            if (patternNot.Pattern is PatternNot innerPatternNot)
            {
                return Visit(innerPatternNot.Pattern);
            }

            return VisitChildren(patternNot);
        }

        protected override PatternBase VisitChildren(PatternBase patternBase)
        {
            try
            {
                return propertyEnumerator.VisitProperties(patternBase, Visit);
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
