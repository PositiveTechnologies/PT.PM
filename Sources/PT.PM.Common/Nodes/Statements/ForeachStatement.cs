using System.Collections.Generic;
using MessagePack;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class ForeachStatement : Statement
    {
        [Key(UstFieldOffset)]
        public TypeToken Type { get; set; }

        [Key(UstFieldOffset + 1)]
        public IdToken VarName { get; set; }

        [Key(UstFieldOffset + 2)]
        public Expression InExpression { get; set; }

        [Key(UstFieldOffset + 3)]
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

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.Add(Type);
            result.Add(VarName);
            result.Add(InExpression);
            result.Add(EmbeddedStatement);
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"foreach ({VarName} in {InExpression})\n{EmbeddedStatement}";
        }
    }
}
