using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Patterns.Nodes
{
    public class PatternStatement : Statement
    {
        public override NodeType NodeType => NodeType.PatternStatement;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Statement Statement { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Not { get; set; }

        public PatternStatement(Statement statement = null, bool not = false)
        {
            Statement = statement;
            Not = not;
        }

        public PatternStatement()
        {
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            int compareResult;
            if (other.NodeType == NodeType.PatternStatement)
            {
                var otherPatternStatement = (PatternStatement)other;

                if (Statement == null ^ otherPatternStatement.Statement == null)
                {
                    return 1;
                }
                if (Statement == null)
                {
                    if (otherPatternStatement.Statement == null)
                    {
                        return Not == otherPatternStatement.Not ? 0 : 1;
                    }
                    else
                    {
                        return 1;
                    }
                }

                compareResult = Statement.CompareTo(otherPatternStatement.Statement);
                if (compareResult != 0)
                {
                    return compareResult;
                }
                
                return Not == otherPatternStatement.Not ? 0 : 1;
            }

            if (!(other is Statement))  // Compare only with statements.
            {
                return -1;
            }

            if (Statement == null)     // Any statement.
            {
                return 0;
            }

            compareResult = Compare(other);
            if (Not)
            {
                compareResult = compareResult > 0 ? 0 : compareResult;
            }

            return compareResult;
        }

        protected virtual int Compare(UstNode other)
        {
            return Statement.CompareTo(other);
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] { Statement };
        }

        public override string ToString()
        {
            if (Statement == null)
            {
                return "#;";
            }

            return (Not ? "<[~]>" : "") + Statement.ToString() + ";";
        }
    }
}
