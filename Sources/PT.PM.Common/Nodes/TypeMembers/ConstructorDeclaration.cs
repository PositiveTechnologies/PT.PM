using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using System.Linq;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class ConstructorDeclaration : EntityDeclaration
    {
        public override NodeType NodeType => NodeType.ConstructorDeclaration;

        public List<ParameterDeclaration> Args { get; set; }

        public BlockStatement Body { get; set; }

        public ConstructorDeclaration(IdToken typeName, IEnumerable<ParameterDeclaration> args, BlockStatement body,
            TextSpan textSpan, RootNode fileNode)
            : base(typeName, textSpan, fileNode)
        {
            Args = args as List<ParameterDeclaration> ?? args.ToList();
            Body = body;
        }

        public ConstructorDeclaration()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>(base.GetChildren());
            result.AddRange(Args);
            result.Add(Body);
            return result.ToArray();
        }
    }
}
