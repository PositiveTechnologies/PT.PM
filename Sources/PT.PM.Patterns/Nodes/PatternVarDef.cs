using System.Collections.Generic;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using Newtonsoft.Json;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Patterns.Nodes
{
    public class PatternVarDef : Token
    {
        public override NodeType NodeType => NodeType.PatternVarDef;

        public static PatternVarDef DefaultPatternVarDef => new PatternVarDef("default", new Expression[] { new PatternExpression() });

        public string Id { get; set; }

        public List<Expression> Values { get; set; } = new List<Expression>();

        [JsonIgnore]
        public Token PinValue { get; set; }

        [JsonIgnore]
        public Expression Value { get; private set; }

        public PatternVarDef()
        {
        }

        public PatternVarDef(string id, IEnumerable<Expression> values)
        {
            Id = id;
            Values = values.ToList();
        }

        public PatternVarDef(string id, IEnumerable<Expression> values, TextSpan textSpan)
            : base(textSpan)
        {
            Id = id;
            Values = values as List<Expression> ?? values.ToList();
        }

        public override int CompareTo(UstNode other)
        {
            Value = null;
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternVarDef)
            {
                var otherPatternVarDef = (PatternVarDef)other;
                /*var idCompareResult = Id.CompareTo(otherPatternVarDef.Id);
                if (idCompareResult != 0)
                {
                    return idCompareResult;
                }*/

                var childrenCountCompareResult = Values.Count - otherPatternVarDef.Values.Count;
                if (childrenCountCompareResult != 0)
                {
                    return childrenCountCompareResult;
                }

                for (int i = 0; i < Values.Count; i++)
                {
                    if (Values[i] == null)
                    {
                        if (otherPatternVarDef.Values[i] != null)
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        var childCompareResult = Values[i].CompareTo(otherPatternVarDef.Values[i]);
                        if (childCompareResult != 0)
                        {
                            return childCompareResult;
                        }
                    }
                }

                return 0;
            }

            foreach (var value in Values)
            {
                if (value == null || value.CompareTo(other) == 0)
                {
                    Value = value;
                    return 0;
                }
            }
            
            return 1;
        }

        public override string TextValue => "(" + string.Join(" || ", Values) + ")";

        public override string ToString()
        {
            return "<[@" + Id + " " + TextValue + (PinValue != null ? $" = {PinValue}" : "") + "]>";
        }
    }
}
