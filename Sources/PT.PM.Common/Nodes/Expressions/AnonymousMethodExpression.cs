using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Expressions
{
    public class AnonymousMethodExpression : Expression
    {
        public List<ParameterDeclaration> Parameters { get; set; } = new List<ParameterDeclaration>();

        public Ust Body { get; set; }

        public string GetId(Ust parent) => parent is AssignmentExpression assignment 
            ? assignment.Left.ToString()
            : LineColumnTextSpan.ToString();

        public string GetSignature(Ust parent) => UstUtils.GenerateSignature(GetId(parent), Parameters);

        public override Expression[] GetArgs() => new Expression[0];

        public AnonymousMethodExpression(IEnumerable<ParameterDeclaration> parameters, Ust body, TextSpan textSpan)
            : base(textSpan)
        {
            Parameters = parameters as List<ParameterDeclaration> ?? parameters.ToList();
            Body = body;
        }

        public AnonymousMethodExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Parameters);
            result.Add(Body);
            return result.ToArray();
        }

        public override string ToString()
        {
            string paramsStr = Parameters.Count == 1
                ? Parameters[0].ToString()
                : "(" + string.Join(", ", Parameters) + ")";
            return $"{paramsStr} => {Body}";
        }
    }
}
