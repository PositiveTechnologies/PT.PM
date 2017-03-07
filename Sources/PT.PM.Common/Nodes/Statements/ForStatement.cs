using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System;
using System.Linq;

namespace PT.PM.Common.Nodes.Statements
{
    public class ForStatement : Statement
    {
        public override NodeType NodeType => NodeType.ForStatement;

        public IEnumerable<Statement> Initializers { get; set; } = ArrayUtils<Statement>.EmptyArray;

        public Expression Condition { get; set; }

        public IEnumerable<Expression> Iterators { get; set; } = ArrayUtils<Expression>.EmptyArray;

        public Statement Statement { get; set; }

        public ForStatement(IEnumerable<Statement> initializers, Expression condition,
            IEnumerable<Expression> iterators, Statement statement, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Initializers = initializers;
            Condition = condition;
            Iterators = iterators;
            Statement = statement;
        }

        public ForStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.AddRange(Initializers);
            result.Add(Condition);
            result.AddRange(Iterators);
            result.Add(Statement);
            return result.ToArray();
        }

        public override string ToString()
        {
            var nl = Environment.NewLine;
            return $"for ({string.Join(" ", Initializers)} {Condition}; {string.Join("; ", Iterators)}) {{{nl}    {Statement}{nl}}}";
        }
    }
}
