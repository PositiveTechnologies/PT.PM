using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using Newtonsoft.Json;

namespace PT.PM.Patterns.Nodes
{
    public class PatternVarRef : IdToken
    {
        private PatternVarDef patternVar = PatternVarDef.DefaultPatternVarDef;

        public override NodeType NodeType => NodeType.PatternVarRef;

        public override string Id
        {
            get
            {
                return "@" + VarId;
            }
            set
            {
                VarId = value;
            }
        }

        public string VarId { get; set; }

        [JsonIgnore]
        public PatternVarDef PatternVar
        {
            get
            {
                return patternVar;
            }
            set
            {
                patternVar = value;
                VarId = patternVar.Id;
            }
        }

        [JsonIgnore]
        public int PatternVarIndex { get; set; }

        [JsonIgnore]
        public UstNode CurrentValue { get; set; } = PatternVarDef.DefaultPatternVarDef.Values[0];

        [JsonIgnore]
        internal bool PinValueAssigned { get; private set; }

        public PatternVarRef(PatternVarDef patternVar, TextSpan textSpan)
            : base()
        {
            PatternVar = patternVar;
            TextSpan = textSpan;
        }

        public PatternVarRef()
        {
        }

        public PatternVarRef(PatternVarDef patternVar)
            : base(null)
        {
            PatternVar = patternVar;
        }

        public override int CompareTo(UstNode other)
        {
            PinValueAssigned = false;
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternVarRef)
            {
                return VarId.CompareTo(((PatternVarRef)other).VarId);
            }

            if (PatternVar.Values[PatternVarIndex] == null)
            {
                return -1;
            }

            int result;
            if (PatternVar.PinValue == null)
            {
                result = CurrentValue.CompareTo(other);
                if (result == 0)
                {
                    PatternVar.PinValue = other as Token;
                    PinValueAssigned = true;
                }
                return result;
            }
            else
            {
                result = PatternVar.PinValue.CompareTo(other);
            }
            return result;
        }

        public override string TextValue
        {
            get { return "<[@" + VarId + (PatternVar?.PinValue != null ? $": {PatternVar.PinValue}" : "") + "]>"; }
        }
    }
}
