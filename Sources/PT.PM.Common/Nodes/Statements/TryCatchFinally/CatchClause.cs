using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Statements.TryCatchFinally
{
    public class CatchClause : Ust
    {
        public TypeToken Type { get; set; }

        public IdToken VarName { get; set; }

        public BlockStatement Body { get; set; }

        public CatchClause(TypeToken type, IdToken varName, BlockStatement body, TextSpan textSpan)
            : base(textSpan)
        {
            Type = type;
            VarName = varName;
            Body = body;
        }

        public CatchClause()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] {Type, VarName, Body};
        }

        public override string ToString()
        {
            return $"catch ({Type.ToStringWithTrailSpace()}{(VarName.ToStringNullable())})\n{{{Body}}}";
        }
    }
}
