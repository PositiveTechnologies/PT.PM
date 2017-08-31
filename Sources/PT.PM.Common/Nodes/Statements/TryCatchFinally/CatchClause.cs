using PT.PM.Common.Nodes.Tokens;
using System;

namespace PT.PM.Common.Nodes.Statements.TryCatchFinally
{
    public class CatchClause : UstNode
    {
        public override NodeType NodeType => NodeType.CatchClause;

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

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Type, VarName, Body};
        }

        public override string ToString()
        {
            var nl = Environment.NewLine;
            return $"catch ({(Type != null ? Type.ToString() : "")} {(VarName != null ? VarName.ToString() : "")}) {{{nl}    {Body}{nl}}}{nl}";
        }
    }
}
