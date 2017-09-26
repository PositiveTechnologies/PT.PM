using PT.PM.Matching.Patterns;

namespace PT.PM.Matching
{
    public interface IUstPatternVisitor<out T>
    {
        T Visit(PatternRootUst patternVars);
        T Visit(PatternBooleanLiteral patternBooleanLiteral);
        T Visit(PatternComment patternComment);
        T Visit(PatternAnyExpression patternAnyExpression);
        T Visit(PatternExpressionInsideNode patternExpressionInsideExpression);
        T Visit(PatternArgs patternExpressions);
        T Visit(PatternIdToken patternIdToken);
        T Visit(PatternIdRegexToken patternIdRegexToken);
        T Visit(PatternIntRangeLiteral patternIntRangeLiteral);
        T Visit(PatternMultipleExpressions patternMultiExpressions);
        T Visit(PatternStatements patternStatements);
        T Visit(PatternStringRegexLiteral patternStringLiteral);
        T Visit(PatternTryCatchStatement patternTryCatchStatement);
        T Visit(PatternVar patternVar);
        T Visit(PatternAnd patternAnd);
        T Visit(PatternNot patternNot);
        T Visit(PatternOr patternOr);
        T Visit(PatternClassDeclaration patternClassDeclaration);
        T Visit(PatternMethodDeclaration patternMethodDeclaration);
        T Visit(PatternVarOrFieldDeclaration patternVarOrFieldDeclaration);
    }
}