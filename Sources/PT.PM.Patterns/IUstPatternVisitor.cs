using PT.PM.Patterns.Nodes;

namespace PT.PM.Patterns
{
    public interface IUstPatternVisitor<out T>
    {
        T Visit(PatternRootNode patternVars);
        T Visit(PatternBooleanLiteral patternBooleanLiteral);
        T Visit(PatternComment patternComment);
        T Visit(PatternExpression patternExpression);
        T Visit(PatternExpressionInsideExpression patternExpressionInsideExpression);
        T Visit(PatternExpressionInsideStatement patternExpressionInsideStatement);
        T Visit(PatternExpressions patternExpressions);
        T Visit(PatternIdToken patternIdToken);
        T Visit(PatternIntLiteral patternIntLiteral);
        T Visit(PatternMultipleExpressions patternMultiExpressions);
        T Visit(PatternMultipleStatements patternMultiStatements);
        T Visit(PatternStatement patternStatement);
        T Visit(PatternStatements patternStatements);
        T Visit(PatternStringLiteral patternStringLiteral);
        T Visit(PatternTryCatchStatement patternTryCatchStatement);
        T Visit(PatternVarDef patternVarDef);
        T Visit(PatternVarRef patternVarRef);
    }
}