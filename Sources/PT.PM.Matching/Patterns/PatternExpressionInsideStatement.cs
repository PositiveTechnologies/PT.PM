using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using Newtonsoft.Json;

namespace PT.PM.Matching.Patterns
{
    public class PatternExpressionInsideStatement : PatternStatement
    {
        private Expression expression;

        public override UstKind Kind => UstKind.PatternExpressionInsideStatement;

        [JsonIgnore]
        public Expression Expression
        {
            get
            {
                if (expression == null)
                {
                    expression = ((ExpressionStatement)Statement).Expression;
                }
                return expression;
            }
            set
            {
                expression = value;
                Statement = new ExpressionStatement(expression);
            }
        }

        public PatternExpressionInsideStatement()
        {
        }

        public PatternExpressionInsideStatement(Expression expression = null, bool not = false)
            : base(new ExpressionStatement(expression), not)
        {
            Expression = expression;
        }

        protected override int Compare(Ust other)
        {
            return other.DoesAnyDescendantMatchPredicate(ustNode => Expression.Equals(ustNode)) ? 0 : 1;
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Expression };
        }

        public override string ToString()
        {
            if (Expression == null)
            {
                return "#*;";
            }

            return (Not ? "<~>" : "") + "#* " + Expression.ToString() + " #*;";
        }
    }
}
