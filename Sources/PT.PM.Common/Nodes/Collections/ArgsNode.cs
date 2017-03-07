using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Collections
{
    public class ArgsNode : CollectionNode<Expression>
    {
        public override NodeType NodeType => NodeType.ArgsNode;

        public ArgsNode(IList<Expression> args, TextSpan textSpan, FileNode fileNode)
            : base(args, textSpan, fileNode)
        {
        }

        public ArgsNode(params Expression[] args)
            : base(args)
        {
        }

        public ArgsNode(IList<Expression> args)
            : base(args)
        {
        }

        public ArgsNode()
            : base()
        {
        }

        public override string ToString()
        {
            return "(" + string.Join(", ", Collection) + ")";
        }
    }
}
