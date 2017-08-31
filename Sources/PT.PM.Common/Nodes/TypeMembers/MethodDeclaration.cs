using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using System;
using System.Linq;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class MethodDeclaration : EntityDeclaration
    {
        public override NodeType NodeType => NodeType.MethodDeclaration;

        public TypeToken ReturnType { get; set; }

        public List<ParameterDeclaration> Parameters { get; set; }

        public BlockStatement Body { get; set; }

        public string Signature
        {
            get
            {
                var paramsString = string.Join(",", Parameters.Select(p => p.Type?.TypeText ?? ""));
                return $"{Name.TextValue}({paramsString})";
            }
        }

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

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>(base.GetChildren());
            result.Add(ReturnType);
            result.AddRange(Parameters);
            result.Add(Body);
            return result.ToArray();
        }

        public override string ToString()
        {
            var nl = Environment.NewLine;
            return $"{ReturnType} {Name}({(string.Join(", ", Parameters))}) {{{nl}    {Body}{nl}}}";
        }
    }
}
