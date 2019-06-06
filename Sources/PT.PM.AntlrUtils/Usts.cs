using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using PT.PM.Common.Nodes;

namespace PT.PM.AntlrUtils
{
    public class Usts : List<Ust>
    {
#if DEBUG
        private ParserRuleContext parserRuleContext;
#endif

        public Usts(ParserRuleContext parserRuleContext)
        {
#if DEBUG
            this.parserRuleContext = parserRuleContext;
#endif
        }

        public override string ToString()
        {
            var result = new StringBuilder();

#if DEBUG
            if (parserRuleContext != null)
            {
                result.Append(parserRuleContext.GetType().Name);
                result.Append(":    ");
            }
#endif

            for (int j = 0; j < Count; j++)
            {
                result.Append(this[j]);
                if (j != Count - 1)
                {
                    result.Append(' ');
                }
            }

            return result.ToString();
        }

#if DEBUG
        public new void Clear()
        {
            base.Clear();
            parserRuleContext = null;
        }
#endif
    }
}
