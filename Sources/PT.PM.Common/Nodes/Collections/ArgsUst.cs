using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Collections
{
    [MessagePackObject]
    public class ArgsUst : CollectionNode<Expression>
    {
        [Key(0)] public override UstType UstType => UstType.ArgsUst;

        public ArgsUst(IEnumerable<Expression> args, TextSpan textSpan)
            : base(args, textSpan)
        {
        }

        public ArgsUst(params Expression[] args)
            : base(args.ToList())
        {
        }

        public ArgsUst(IEnumerable<Expression> args)
            : base(args)
        {
        }

        public ArgsUst()
            : base()
        {
        }

        public override string ToString()
        {
            return string.Join(", ", Collection);
        }
    }
}
