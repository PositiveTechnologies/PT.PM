using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.Common.Nodes;

namespace PT.PM.AntlrUtils
{
    /// <summary>
    /// Generates UST node at each `ExitEveryRule` method
    /// If needed, use concrete Listener with overriden methods for certain contexts
    /// </summary>
    public class ListenerConverter : IParseTreeListener
    {
        // We should store children count for every ascendant node and process such number in ProcessChildren
        // After processing, such nodes should be removed from `resultNodes`
        private readonly Dictionary<IParseTree, int> childCount = new Dictionary<IParseTree, int>();

        // Stack of created children
        private readonly List<Ust> resultNodes = new List<Ust>();

        /// <summary>
        /// Calls at the end of conversion process.
        /// </summary>
        /// <returns>Root node</returns>
        public Ust Complete()
        {
            return ProcessChildren(null, resultNodes.Count);
        }

        /// <summary>
        /// Saves child count for every context
        /// </summary>
        /// <param name="ctx"></param>
        public void EnterEveryRule(ParserRuleContext ctx)
        {
            AddChildCount(ctx.parent);
        }

        public void ExitEveryRule(ParserRuleContext ctx)
        {
            if (childCount.TryGetValue(ctx, out int count))
            {
                ProcessChildren(ctx, count);
            }

            // TODO: Log error. Should not be gone here, visitor stack is broken
        }

        public void VisitTerminal(ITerminalNode node)
        {
            AddChildCount(node.Parent);
            resultNodes.Add(null); // TODO: add result UST
        }

        public void VisitErrorNode(IErrorNode node)
        {
            AddChildCount(node.Parent);
            resultNodes.Add(null); // TODO: add result UST
        }

        private Ust ProcessChildren(IParseTree parseTree, int count)
        {
            // Extract and process previous nodes in common, left-to-right order
            for (int i = resultNodes.Count - count; i < resultNodes.Count; i++)
            {
                // TODO: process nodes (result children)
            }

            // Children always are being removed from `resultNodes`
            resultNodes.RemoveRange(resultNodes.Count - count, count);

            Ust result = null; // TODO: create result UST

            if (parseTree != null)
            {
                // It's always necessary to add result node
                resultNodes.Add(result);

                // Removes context from dictionary to release object and make it ready for GC
                childCount.Remove(parseTree);
            }

            return result;
        }

        private void AddChildCount(IRuleNode parent)
        {
            if (parent != null)
            {
                if (childCount.TryGetValue(parent, out int count))
                {
                    childCount[parent] = count + 1;
                }
                else
                {
                    childCount.Add(parent, 1);
                }
            }
        }
    }
}