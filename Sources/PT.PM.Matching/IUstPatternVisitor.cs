using PT.PM.Matching.Patterns;

namespace PT.PM.Matching
{
    public interface IUstPatternVisitor<out T>
    {
        T Visit(PatternAnd patternAnd);
        T Visit(PatternAnonymousMethodExpression patternAnonymousMethodExpression);
        T Visit(PatternAnyExpression patternAnyExpression);
        T Visit(PatternArgs patternArgs);
        T Visit(PatternAssignmentExpression patternAssignmentExpression);
        T Visit(PatternBaseReferenceExpression patternBaseReferenceExpression);
        T Visit(PatternBinaryOperatorExpression patternBinaryOperatorExpression);
        T Visit(PatternBooleanLiteral patternBooleanLiteral);
        T Visit(PatternClassDeclaration patternClassDeclaration);
        T Visit(PatternComment patternComment);
        T Visit(PatternExpressionInside patternExpressionInside);
        T Visit(PatternIdRegexToken patternIdRegexToken);
        T Visit(PatternIdToken patternIdToken);
        T Visit(PatternIndexerExpression patternIndexerExpression);
        T Visit(PatternIntLiteral patternIntLiteral);
        T Visit(PatternIntRangeLiteral patternIntRangeLiteral);
        T Visit(PatternInvocationExpression patternInvocationExpression);
        T Visit(PatternMemberReferenceExpression patternMemberReferenceExpression);
        T Visit(PatternMethodDeclaration patternMethodDeclaration);
        T Visit(PatternMultipleExpressions patternMultiExpressions);
        T Visit(PatternNot patternNot);
        T Visit(PatternNullLiteral patternNullLiteral);
        T Visit(PatternObjectCreateExpression patternObjectCreateExpression);
        T Visit(PatternOr patternOr);
        T Visit(PatternParameterDeclaration patternParameterDeclaration);
        T Visit(PatternRootUst patternRootUst);
        T Visit(PatternStatements patternStatements);
        T Visit(PatternStringLiteral patternStringLiteral);
        T Visit(PatternStringRegexLiteral patternStringLiteral);
        T Visit(PatternTryCatchStatement patternTryCatchStatement);
        T Visit(PatternVar patternVar);
        T Visit(PatternVarOrFieldDeclaration patternVarOrFieldDeclaration);
    }
}