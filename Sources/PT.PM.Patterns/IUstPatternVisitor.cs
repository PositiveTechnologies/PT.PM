using PT.PM.Dsl;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Patterns
{
    public interface IUstPatternVisitor<out T>
    {
        T Visit(PatternNode patternVars);
        T Visit(PatternBooleanLiteral patternBooleanLiteral);
        T Visit(PatternComment patternComment);
        T Visit(PatternExpression patternExpression);
        T Visit(PatternExpressionInsideNode patternExpressionInsideExpression);
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
        T Visit(PatternAnd patternAnd);
        T Visit(PatternNot patternNot);
        T Visit(PatternClassDeclaration patternClassDeclaration);
        T Visit(PatternMethodDeclaration patternMethodDeclaration);

        T Visit(DslNode patternExpression);
        T Visit(LangCodeNode langCodeNode);
    }
}