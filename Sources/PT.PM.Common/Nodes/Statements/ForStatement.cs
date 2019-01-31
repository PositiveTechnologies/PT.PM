using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class ForStatement : Statement
    {
        [Key(UstFieldOffset)]
        public List<Statement> Initializers { get; set; } = new List<Statement>();

        [Key(UstFieldOffset + 1)]
        public Expression Condition { get; set; }

        [Key(UstFieldOffset + 2)]
        public List<Expression> Iterators { get; set; } = new List<Expression>();

        [Key(UstFieldOffset + 3)]
        public Statement Statement { get; set; }

        public ForStatement(IEnumerable<Statement> initializers, Expression condition,
            IEnumerable<Expression> iterators, Statement statement, TextSpan textSpan)
            : base(textSpan)
        {
            Initializers = initializers as List<Statement> ?? initializers.ToList();
            Condition = condition;
            Iterators = iterators as List<Expression> ?? iterators.ToList();
            Statement = statement;
        }

        public ForStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Initializers);
            result.Add(Condition);
            result.AddRange(Iterators);
            result.Add(Statement);
            return result.ToArray();
        }

        public override string ToString()
        {
            string initializersStr = Initializers?.Count > 0 ? string.Join(" ", Initializers) : "";
            string iteratorsStr = Iterators?.Count > 0 ? " " + string.Join("; ", Iterators) : "";
            return $"for ({TextUtils.CollectWords(initializersStr, Condition)};{iteratorsStr})\n{{\n{Statement.ToStringWithTrailNL()}}}";
        }
    }
}
