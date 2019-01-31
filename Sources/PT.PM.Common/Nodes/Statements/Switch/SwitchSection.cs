using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Statements.Switch
{
    [MessagePackObject]
    public class SwitchSection : Ust
    {
        [Key(UstFieldOffset)]
        public List<Expression> CaseLabels { get; set; } = new List<Expression>();

        [Key(UstFieldOffset + 1)]
        public List<Statement> Statements { get; set; } = new List<Statement>();

        public SwitchSection(IEnumerable<Expression> caseLabels, IEnumerable<Statement> statements, TextSpan textSpan)
            : base(textSpan)
        {
            CaseLabels = caseLabels as List<Expression> ?? caseLabels.ToList();
            Statements = statements as List<Statement> ?? statements.ToList();
        }

        public SwitchSection()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(CaseLabels);
            result.AddRange(Statements);
            return result.ToArray();
        }

        public override string ToString() => ToString(false, "");

        public string ToString(bool newline, string prevIndent = "")
        {
            TextUtils.GetNewlineIndent(newline, prevIndent, out string nl, out string indent);
            string nlIndent = nl + indent;
            string caseLabels = string.Join(nl + prevIndent, CaseLabels.Select(caseLabel => "case " + caseLabel + ": "));
            string statements = string.Join(nlIndent, Statements);
            return $"{prevIndent}{caseLabels}{nlIndent}{statements}";
        }
    }
}
