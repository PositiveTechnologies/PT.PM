using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Collections
{
    public class ArgsNode : CollectionNode<Expression>
    {
        public override NodeType NodeType => NodeType.ArgsNode;

        public ArgsNode(IEnumerable<Expression> args, TextSpan textSpan, RootNode fileNode)
            : base(args, textSpan, fileNode)
        {
        }

        public ArgsNode(params Expression[] args)
            : base(args.ToList())
        {
        }

        public ArgsNode(List<Expression> args)
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
