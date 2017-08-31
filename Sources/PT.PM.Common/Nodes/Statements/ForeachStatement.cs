using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Statements
{
    public class ForeachStatement : Statement
    {
        public override NodeType NodeType => NodeType.ForeachStatement;

        public TypeToken Type { get; set; }

        public IdToken VarName { get; set; }

        public Expression InExpression { get; set; }

        public Statement EmbeddedStatement { get; set; }

        public ForeachStatement(TypeToken type, IdToken varName, Expression inExpression,
            Statement embeddedStatement, TextSpan textSpan)
            : base(textSpan)
        {
            Type = type;
            VarName = varName;
            InExpression = inExpression;
            EmbeddedStatement = embeddedStatement;
        }

        public ForeachStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.Add(Type);
            result.Add(VarName);
            result.Add(InExpression);
            result.Add(EmbeddedStatement);
            return result.ToArray();
        }
    }
}
