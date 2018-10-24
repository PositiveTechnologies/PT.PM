using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using System;
using System.Linq;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class MethodDeclaration : EntityDeclaration
    {
        public TypeToken ReturnType { get; set; }

        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        public BlockStatement Body { get; set; }

        public string Signature => UstUtils.GenerateSignature(Name.TextValue, Parameters);

        public MethodDeclaration(IdToken name, IEnumerable<Parameter> parameters, BlockStatement body, TextSpan textSpan)
            : base(name, textSpan)
        {
            Parameters = parameters as List<Parameter>
                ?? parameters?.ToList()
                ?? new List<Parameter>();
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
            return $"{ReturnType.ToStringWithTrailSpace()}{Name}({(string.Join(", ", Parameters))})\n{{\n{Body.ToStringWithTrailNL()}}}";
        }
    }
}
