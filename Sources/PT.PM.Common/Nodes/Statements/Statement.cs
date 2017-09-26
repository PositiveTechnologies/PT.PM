using System;

namespace PT.PM.Common.Nodes.Statements
{
    public abstract class Statement : Ust
    {
        public override UstKind Kind => UstKind.Statement;

        protected Statement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Statement()
        {
        }
    }
}
