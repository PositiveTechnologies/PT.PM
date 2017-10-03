using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Reflection;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public class PatternVisitor<T> : IPatternVisitor<T>, ILoggable
    {
        private static PropertyVisitor<PatternUst, T> propertyEnumerator = new PropertyVisitor<PatternUst, T>
        {
            IgnoredProperties = new HashSet<string>() { nameof(PatternUst.Parent), nameof(PatternUst.Root) }
        };

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public virtual T Visit(PatternUst patternBase)
        {
            if (patternBase == null)
            {
                return default(T);
            }
            return Visit((dynamic)patternBase);
        }

        public virtual T Visit(PatternAnd patternAnd)
        {
            return VisitChildren(patternAnd);
        }

        public virtual T Visit(PatternAny patternAny)
        {
            return VisitChildren(patternAny);
        }

        public virtual T Visit(PatternAnyExpression patternAnyExpression)
        {
            return VisitChildren(patternAnyExpression);
        }

        public virtual T Visit(PatternArgs patternArgs)
        {
            return VisitChildren(patternArgs);
        }

        public virtual T Visit(PatternAssignmentExpression patternAssignmentExpression)
        {
            return VisitChildren(patternAssignmentExpression);
        }

        public virtual T Visit(PatternThisReferenceToken patternThisReferenceToken)
        {
            return VisitChildren(patternThisReferenceToken);
        }

        public virtual T Visit(PatternBaseReferenceToken patternBaseReferenceExpression)
        {
            return VisitChildren(patternBaseReferenceExpression);
        }

        public virtual T Visit(PatternBinaryOperatorExpression patternBinaryOperatorExpression)
        {
            return VisitChildren(patternBinaryOperatorExpression);
        }

        public virtual T Visit(PatternBinaryOperatorLiteral patternBinaryOperatorLiteral)
        {
            return VisitChildren(patternBinaryOperatorLiteral);
        }

        public virtual T Visit(PatternBooleanLiteral patternBooleanLiteral)
        {
            return VisitChildren(patternBooleanLiteral);
        }

        public virtual T Visit(PatternClassDeclaration patternClassDeclaration)
        {
            return VisitChildren(patternClassDeclaration);
        }

        public virtual T Visit(PatternCommentRegex patternCommentRegex)
        {
            return VisitChildren(patternCommentRegex);
        }

        public virtual T Visit(PatternArbitraryDepth patternArbitraryDepth)
        {
            return VisitChildren(patternArbitraryDepth);
        }

        public virtual T Visit(PatternIdRegexToken patternIdRegexToken)
        {
            return VisitChildren(patternIdRegexToken);
        }

        public virtual T Visit(PatternIdToken patternIdToken)
        {
            return VisitChildren(patternIdToken);
        }

        public virtual T Visit(PatternIndexerExpression patternIndexerExpression)
        {
            return VisitChildren(patternIndexerExpression);
        }

        public virtual T Visit(PatternIntLiteral patternIntLiteral)
        {
            return VisitChildren(patternIntLiteral);
        }

        public virtual T Visit(PatternIntRangeLiteral patternIntRangeLiteral)
        {
            return VisitChildren(patternIntRangeLiteral);
        }

        public virtual T Visit(PatternInvocationExpression patternInvocationExpression)
        {
            return VisitChildren(patternInvocationExpression);
        }

        public virtual T Visit(PatternMemberReferenceExpression patternMemberReferenceExpression)
        {
            return VisitChildren(patternMemberReferenceExpression);
        }

        public virtual T Visit(PatternMethodDeclaration patternMethodDeclaration)
        {
            return VisitChildren(patternMethodDeclaration);
        }

        public virtual T Visit(PatternMultipleExpressions patternMultipleExpressions)
        {
            return VisitChildren(patternMultipleExpressions);
        }

        public virtual T Visit(PatternNot patternNot)
        {
            return VisitChildren(patternNot);
        }

        public virtual T Visit(PatternNullLiteral patternNullLiteral)
        {
            return VisitChildren(patternNullLiteral);
        }

        public virtual T Visit(PatternObjectCreateExpression patternObjectCreateExpression)
        {
            return VisitChildren(patternObjectCreateExpression);
        }

        public virtual T Visit(PatternOr patternOr)
        {
            return VisitChildren(patternOr);
        }

        public virtual T Visit(PatternParameterDeclaration patternParameterDeclaration)
        {
            return VisitChildren(patternParameterDeclaration);
        }

        public virtual T Visit(PatternStatements patternStatements)
        {
            return VisitChildren(patternStatements);
        }

        public virtual T Visit(PatternStringLiteral patternStringLiteral)
        {
            return VisitChildren(patternStringLiteral);
        }

        public virtual T Visit(PatternStringRegexLiteral patternStringRegexLiteral)
        {
            return VisitChildren(patternStringRegexLiteral);
        }

        public virtual T Visit(PatternTryCatchStatement patternTryCatchStatement)
        {
            return VisitChildren(patternTryCatchStatement);
        }

        public virtual T Visit(PatternVar patternVar)
        {
            return VisitChildren(patternVar);
        }

        public virtual T Visit(PatternVarOrFieldDeclaration patternVarOrFieldDeclaration)
        {
            return VisitChildren(patternVarOrFieldDeclaration);
        }

        protected virtual T VisitChildren(PatternUst patternBase)
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
                return default(T);
            }
        }
    }
}
