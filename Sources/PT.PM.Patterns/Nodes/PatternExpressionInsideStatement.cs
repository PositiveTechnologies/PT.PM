using PT.PM.Common.Nodes.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.PM.Common.Nodes;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes.Statements;
using Newtonsoft.Json;

namespace PT.PM.Patterns.Nodes
{
    public class PatternExpressionInsideStatement : PatternStatement
    {
        private Expression expression;

        public override NodeType NodeType => NodeType.PatternExpressionInsideStatement;

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

        protected override int Compare(UstNode other)
        {
            return other.DoesAnyDescendantMatchPredicate(astNode => Expression.Equals(astNode)) ? 0 : 1;
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] { Expression };
        }

        public override string ToString()
        {
            if (Expression == null)
            {
                return "#*;";
            }

            return (Not ? "<[~]>" : "") + "#* " + Expression.ToString() + " #*;";
        }
    }
}
