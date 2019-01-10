using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.Statements.TryCatchFinally
{
    [MessagePackObject]
    public class TryCatchStatement : Statement
    {
        [Key(UstFieldOffset)]
        public BlockStatement TryBlock { get; set; }

        [Key(UstFieldOffset + 1)]
        public List<CatchClause> CatchClauses { get; set; }

        [Key(UstFieldOffset + 2)]
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
