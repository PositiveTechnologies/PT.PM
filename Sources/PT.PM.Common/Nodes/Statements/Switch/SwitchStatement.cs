using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Statements.Switch
{
    [MessagePackObject]
    public class SwitchStatement : Statement
    {
        [Key(UstFieldOffset)]
        public Expression Expression { get; set; }

        [Key(UstFieldOffset + 1)]
        public List<SwitchSection> Sections { get; set; }

        public SwitchStatement(Expression expression, IEnumerable<SwitchSection> sections, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = expression;
            Sections = sections as List<SwitchSection> ?? sections.ToList();
        }

        public SwitchStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.Add(Expression);
            result.AddRange(Sections);
            return result.ToArray();
        }

        public override string ToString() => $"switch ({Expression})\n{{{string.Join("\n", Sections)}}}";

        public string ToString(bool newline, string prevIndent = "")
        {
            TextUtils.GetNewlineIndent(newline, prevIndent, out string nl, out string indent);
            return $"{prevIndent}switch({Expression}){prevIndent}{{{string.Join(nl + indent, Sections)}}}";
        }
    }
}
