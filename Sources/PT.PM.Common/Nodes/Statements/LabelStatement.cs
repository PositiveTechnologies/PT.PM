using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Statements
{
    public class LabelStatement : Statement
    {
        public IdToken Label { get; set; }

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

        public override Ust[] GetChildren()
        {
            return new Ust[] { Label, Body };
        }

        public override string ToString()
        {
            return $"{Label}:\n{Body}";
        }
    }
}
