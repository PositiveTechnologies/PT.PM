using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Reflection;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PT.PM.Matching
{
    public class PatternNormalizer : PatternVisitor<PatternUst>
    {
        private static PropertyCloner<PatternUst> propertyEnumerator = new PropertyCloner<PatternUst>
        {
            IgnoredProperties = new HashSet<string> { nameof(PatternUst.Parent), nameof(PatternUst.Root) }
        };

        public PatternRoot Normalize(PatternRoot pattern)
        {
            var newPattern = new PatternRoot
            {
                Logger = pattern.Logger,
                Key = pattern.Key,
                FilenameWildcard = pattern.FilenameWildcard,
                File = pattern.File,
                Languages = new HashSet<Language>(pattern.Languages),
                DataFormat = pattern.DataFormat,
                DebugInfo = pattern.DebugInfo,
                Node = Visit(pattern.Node),
            };
            var ascendantsFiller = new PatternAscendantsFiller(newPattern);
            ascendantsFiller.FillAscendants();
            return newPattern;
        }

        public override PatternUst Visit(PatternArgs patternExpressions)
        {
            // #* #* ... #* -> #*
            var args = new List<PatternUst>(patternExpressions.Args.Count);
            foreach (PatternUst arg in patternExpressions.Args)
            {
                args.Add(arg);
            }

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

            return new PatternArgs(args);
        }

        public override PatternUst Visit(PatternOr patternOr)
        {
            if (patternOr.Patterns.Count == 1)
            {
                return Visit(patternOr.Patterns[0]);
            }

            var exprs = new List<PatternUst>(patternOr.Patterns.Count);
            foreach (PatternUst pattern in patternOr.Patterns)
            {
                exprs.Add(Visit(pattern));
            }
            exprs.Sort();
            
            return new PatternOr(exprs, patternOr.TextSpan);
        }

        public override PatternUst Visit(PatternIdRegexToken patternIdRegexToken)
        {
            string regexString = patternIdRegexToken.Regex.ToString();

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

        public override PatternUst Visit(PatternIntRangeLiteral patternIntLiteral)
        {
            if (patternIntLiteral.MinValue == patternIntLiteral.MaxValue)
            {
                return new PatternIntLiteral(
                    patternIntLiteral.MinValue,
                    patternIntLiteral.TextSpan);
            }

            return new PatternIntRangeLiteral(
                patternIntLiteral.MinValue,
                patternIntLiteral.MaxValue,
                patternIntLiteral.TextSpan);
        }

        public override PatternUst Visit(PatternAnd patternAnd)
        {
            if (patternAnd.Patterns.Count == 1)
            {
                return Visit(patternAnd.Patterns[0]);
            }

            var exprs = new List<PatternUst>(patternAnd.Patterns.Count);
            foreach (PatternUst pattern in patternAnd.Patterns)
            {
                exprs.Add(Visit(pattern));
            }
            exprs.Sort();

            return new PatternAnd(exprs, patternAnd.TextSpan);
        }

        public override PatternUst Visit(PatternNot patternNot)
        {
            if (patternNot.Pattern is PatternNot innerPatternNot)
            {
                return Visit(innerPatternNot.Pattern);
            }

            return VisitChildren(patternNot);
        }

        protected override PatternUst VisitChildren(PatternUst patternBase)
        {
            try
            {
                return propertyEnumerator.VisitProperties(patternBase, Visit);
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ConversionException(patternBase.Root?.File, ex)
                {
                    TextSpan = patternBase.TextSpan
                });
                return null;
            }
        }
    }
}
