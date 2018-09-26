using System;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Statements.TryCatchFinally
{
    public class TryCatchStatement : Statement
    {
        public BlockStatement TryBlock { get; set; }

        public List<CatchClause> CatchClauses { get; set; }

        public BlockStatement FinallyBlock { get; set; }

        public TryCatchStatement(BlockStatement tryBlock, TextSpan textSpan)
            : base(textSpan)
        {
            TryBlock = tryBlock;
        }

        public TryCatchStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.Add(TryBlock);
            if (CatchClauses != null)
                result.AddRange(CatchClauses);
            if (FinallyBlock != null)
                result.Add(FinallyBlock);
            return result.ToArray();
        }

        public override string ToString()
        {
            var result = $"try {{{TryBlock}}}\n";
            if (CatchClauses?.Count > 0)
            {
                result += string.Join("\n", CatchClauses);
            }
            if (FinallyBlock != null)
            {
                result += $"finally\n{{\n{FinallyBlock.ToStringWithTrailNL()}}}";
            }
            return result;
        }
    }
}
