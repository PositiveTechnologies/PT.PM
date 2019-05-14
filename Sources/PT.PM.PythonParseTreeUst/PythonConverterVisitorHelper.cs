using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
using System.Linq;
using PythonParseTree;

namespace PT.PM.PythonParseTreeUst
{
    public partial class PythonAntlrConverter
    {
        public override Ust VisitTerminal(ITerminalNode node)
        {
            var text = node.GetText();

            if (text == "False" || text == "True")
            {
                return new BooleanLiteral(text == "True", node.GetTextSpan());
            }

            if (text == "None")
            {
                return new NullLiteral(node.GetTextSpan());
            }

            return base.VisitTerminal(node);
        }

        private Expression CreateInvocationExpression(Expression target, ArgsUst args, TextSpan textSpan)
           => new InvocationExpression(target, args, textSpan);

        private Expression CreateIndexerExpression(Expression target, ArgsUst args, TextSpan textSpan)
            => new IndexerExpression(target, args, textSpan);

        private Collection CreateParametersCollection(ParserRuleContext context)
        {
            var result = new Collection();
            foreach (var child in context.children.Where(x => x.GetText() != ","))
            {
                var visited = Visit(child);
                if (visited is Collection visitedCollection)
                {
                    result.Collection.AddRange(visitedCollection.Collection);
                }
                else
                {
                    result.Collection.Add(visited);
                }
            }
            return result;
        }

        private Ust CreateLambdaMethod(ParserRuleContext bodyContext, PythonParser.VarargslistContext argsListContext, TextSpan textSpan)
        {
            var result = new AnonymousMethodExpression
            {
                Body = Visit(bodyContext),
                TextSpan = textSpan
            };
            if (argsListContext != null)
            {
                result.Parameters = ((Collection)Visit(argsListContext))
                .Collection.Cast<ParameterDeclaration>().ToList();
            }
            return result;
        }
    }
}
