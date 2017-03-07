using PT.PM.Common.Symbols;
using System;

namespace PT.PM.Common.Nodes.Expressions
{
    public abstract class Expression : UstNode
    {
        public ISymbol Invocation { get; set; }

        public ISymbol ReturnType { get; set; }

        protected Expression(TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        protected Expression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Expression()
        {
        }
    }
}
