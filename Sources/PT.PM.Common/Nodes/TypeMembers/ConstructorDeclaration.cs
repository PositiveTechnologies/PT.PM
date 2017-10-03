using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using System.Linq;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class ConstructorDeclaration : EntityDeclaration
    {
        public List<ParameterDeclaration> Args { get; set; }

        public BlockStatement Body { get; set; }

        public ConstructorDeclaration(IdToken typeName, IEnumerable<ParameterDeclaration> args, BlockStatement body,
            TextSpan textSpan)
            : base(typeName, textSpan)
        {
            Args = args as List<ParameterDeclaration> ?? args.ToList();
            Body = body;
        }

        public ConstructorDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>(base.GetChildren());
            result.AddRange(Args);
            result.Add(Body);
            return result.ToArray();
        }
    }
}
