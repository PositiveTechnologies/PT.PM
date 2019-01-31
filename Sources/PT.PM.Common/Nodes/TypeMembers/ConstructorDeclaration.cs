using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.TypeMembers
{
    [MessagePackObject]
    public class ConstructorDeclaration : MethodDeclaration
    {
        public ConstructorDeclaration(IdToken typeName, IEnumerable<ParameterDeclaration> parameters, BlockStatement body,
            TextSpan textSpan)
            : base(typeName, parameters, body, textSpan)
        {
        }

        public ConstructorDeclaration()
        {
        }
    }
}
