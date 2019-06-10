using MessagePack;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class LabelStatement : Statement
    {
        [Key(0)] public override UstType UstType => UstType.LabelStatement;

        [Key(UstFieldOffset)]
        public IdToken Label { get; set; }

        [Key(UstFieldOffset + 1)]
        public Statement Body { get; set; }

        public LabelStatement(IdToken label, Statement body, TextSpan textSpan)
            : base(textSpan)
        {
            Label = label;
            Body = body;
        }

        public LabelStatement()
        {
        }

        public override Ust[] GetChildren() => new Ust[] { Label, Body };

        public override string ToString()
        {
            return $"{Label}:\n{Body}";
        }
    }
}
