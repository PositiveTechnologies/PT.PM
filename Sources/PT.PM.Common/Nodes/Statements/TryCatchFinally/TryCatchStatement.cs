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
        public List<CatchClause> CatchClauses { get; set; } = new List<CatchClause>();

        [Key(UstFieldOffset + 2)]
        public BlockStatement ElseBlock { get; set; }

        [Key(UstFieldOffset + 3)]
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
            {
                result.AddRange(CatchClauses);
            }
            if (ElseBlock != null)
            {
                result.Add(ElseBlock);
            }
            if (FinallyBlock != null)
            {
                result.Add(FinallyBlock);
            }
            return result.ToArray();
        }

        public override string ToString()
        {
            var result = $"try {TryBlock}\n";
            if (CatchClauses?.Count > 0)
            {
                result += string.Join("\n", CatchClauses);
            }
            if (ElseBlock != null)
            {
                result += $"else\n\n{ElseBlock.ToStringWithTrailNL()}";
            }
            if (FinallyBlock != null)
            {
                result += $"finally\n\n{FinallyBlock.ToStringWithTrailNL()}";
            }
            return result;
        }
    }
}
