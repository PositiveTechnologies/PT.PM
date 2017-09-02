using System;
using Newtonsoft.Json;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class ModifierLiteral : Literal
    {
        public override NodeType NodeType => NodeType.ModifierLiteral;

        [JsonIgnore]
        public Modifier Modifier { get; set; }

        public string ModifierName { get; private set; }

        public override string TextValue => ModifierName;

        public ModifierLiteral(Modifier modifier, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Modifier = modifier;
            if (Modifier == Modifier.Other)
            {
                throw new ArgumentException("modifier should not equal to the Other value");
            }
            ModifierName = Modifier.ToString().ToLowerInvariant();
        }

        public ModifierLiteral(string modifierName, TextSpan textSpan, FileNode fileNode)
             : base(textSpan, fileNode)
        {
            Modifier modifier;
            if (Enum.TryParse(modifierName, true, out modifier))
            {
                Modifier = modifier;
                ModifierName = Modifier.ToString().ToLowerInvariant();
            }
            else
            {
                Modifier = Modifier.Other;
                ModifierName = modifierName;
            }
        }

        public ModifierLiteral()
        {
            Modifier = Modifier.None;
            ModifierName = "";
        }

        public override int CompareTo(UstNode other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var modifierCompareResult = Modifier - ((ModifierLiteral)other).Modifier;
            return modifierCompareResult;
        }
    }
}
