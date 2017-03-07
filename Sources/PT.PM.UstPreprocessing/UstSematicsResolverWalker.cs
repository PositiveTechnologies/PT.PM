using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Symbols;

namespace PT.PM.UstPreprocessing
{
    public class UstSematicsResolverWalker : AstListener
    {
        private IDictionary<string, TypeSymbol> typeSymbols = new Dictionary<string, TypeSymbol>();

        public UstSematicsResolverWalker(IDictionary<string, TypeSymbol> typeSymbols)
        {
            this.typeSymbols = typeSymbols;
        }

        public override void Enter(InvocationExpression invocationExpression)
        {
            invocationExpression.Invocation = 
        }
    }
}
