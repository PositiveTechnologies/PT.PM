﻿using PT.PM.Matching.Patterns;

namespace PT.PM.Matching
{
    public interface IPatternVisitor<out T>
    {
        T Visit(PatternUst patternUst);
        T Visit(PatternRoot patternRoot);
        T Visit(PatternAnd patternAnd);
        T Visit(PatternAny patternAny);
        T Visit(PatternArgs patternArgs);
        T Visit(PatternArrayCreationExpression patternArrayCreationExpression);
        T Visit(PatternAssignmentExpression patternAssignmentExpression);
        T Visit(PatternThisReferenceToken patternThisReferenceToken);
        T Visit(PatternBaseReferenceToken patternBaseReferenceToken);
        T Visit(PatternBinaryOperatorExpression patternBinaryOperatorExpression);
        T Visit(PatternBinaryOperatorLiteral patternBinaryOperatorLiteral);
        T Visit(PatternBooleanLiteral patternBooleanLiteral);
        T Visit(PatternClassDeclaration patternClassDeclaration);
        T Visit(PatternCommentRegex patternCommentRegex);
        T Visit(PatternArbitraryDepth patternArbitraryDepth);
        T Visit(PatternReturnStatement patternReturn);
        T Visit(PatternThrowStatement patternThrow);
        T Visit(PatternIdRegexToken patternIdRegexToken);
        T Visit(PatternIdToken patternIdToken);
        T Visit(PatternIndexerExpression patternIndexerExpression);
        T Visit(PatternIntLiteral patternIntLiteral);
        T Visit(PatternIntRangeLiteral patternIntRangeLiteral);
        T Visit(PatternBigIntLiteral patternBigIntLiteral);
        T Visit(PatternInvocationExpression patternInvocationExpression);
        T Visit(PatternMemberReferenceExpression patternMemberReferenceExpression);
        T Visit(PatternMethodDeclaration patternMethodDeclaration);
        T Visit(PatternMultipleExpressions patternMultipleExpressions);
        T Visit(PatternNot patternNot);
        T Visit(PatternNullLiteral patternNullLiteral);
        T Visit(PatternObjectCreateExpression patternObjectCreateExpression);
        T Visit(PatternTupleCreateExpression patternTupleCreateExpression);
        T Visit(PatternOr patternOr);
        T Visit(PatternParameterDeclaration patternParameterDeclaration);
        T Visit(PatternStatements patternStatements);
        T Visit(PatternStringLiteral patternStringLiteral);
        T Visit(PatternStringRegexLiteral patternStringRegexLiteral);
        T Visit(PatternTryCatchStatement patternTryCatchStatement);
        T Visit(PatternUnaryOperatorExpression patternUnaryOperatorExpression);
        T Visit(PatternUnaryOperatorLiteral patternUnaryOperatorLiteral);
        T Visit(PatternVar patternVar);
        T Visit(PatternVarOrFieldDeclaration patternVarOrFieldDeclaration);
    }
}