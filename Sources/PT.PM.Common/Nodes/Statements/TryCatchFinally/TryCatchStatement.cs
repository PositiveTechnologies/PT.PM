using System;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Statements.TryCatchFinally
{
    public class TryCatchStatement : Statement
    {
        public override UstKind Kind => UstKind.TryCatchStatement;

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
            var nl = Environment.NewLine;
            var result = $"try {{{nl}    {TryBlock}{nl}}}{nl}";
            if (CatchClauses != null)
            {
                result += string.Join("", CatchClauses);
            }
            if (FinallyBlock != null)
            {
                result += $"finally {{{nl}    {FinallyBlock}{nl}}}{nl}";
            }
            return result;
        }
    }
}
