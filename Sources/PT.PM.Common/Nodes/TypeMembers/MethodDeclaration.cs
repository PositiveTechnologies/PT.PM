using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using System.Linq;
using MessagePack;
using Newtonsoft.Json;

namespace PT.PM.Common.Nodes.TypeMembers
{
    [MessagePackObject]
    public class MethodDeclaration : EntityDeclaration, IAttributable
    {
        [Key(EntityFieldOffset)]
        public TypeToken ReturnType { get; set; }

        [Key(EntityFieldOffset + 1)]
        public List<ParameterDeclaration> Parameters { get; set; } = new List<ParameterDeclaration>();

        [Key(EntityFieldOffset + 2)]
        public BlockStatement Body { get; set; }

        [Key(EntityFieldOffset + 3)]
        public List<Attribute> Attributes { get; set; } = new List<Attribute>();

        [IgnoreMember, JsonIgnore]
        public string Signature => UstUtils.GenerateSignature(Name.TextValue, Parameters);

        public MethodDeclaration(IdToken name, IEnumerable<ParameterDeclaration> parameters, BlockStatement body, TextSpan textSpan)
            : base(name, textSpan)
        {
            Parameters = parameters as List<ParameterDeclaration>
                ?? parameters?.ToList()
                ?? new List<ParameterDeclaration>();
            Body = body;
        }

        public MethodDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>(base.GetChildren());
            result.Add(ReturnType);
            result.AddRange(Parameters);
            result.Add(Body);
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"{string.Join(" ", Attributes)}{ReturnType.ToStringWithTrailSpace()}{Name}({(string.Join(", ", Parameters))})\n{{\n{Body.ToStringWithTrailNL()}}}";
        }
    }
}
