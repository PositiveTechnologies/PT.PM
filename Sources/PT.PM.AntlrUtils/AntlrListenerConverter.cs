using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.Common;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.AntlrUtils
{
    /// <summary>
    /// Generates UST node at each `ExitEveryRule` method
    /// If needed, use concrete Listener with overriden methods for certain contexts
    /// </summary>
    public abstract class AntlrListenerConverter : IParseTreeListener, ILoggable
    {
        private readonly Stack<List<Ust>> childNodes = new Stack<List<Ust>>();
        private readonly AntlrConvertHelper convertHelper;
        protected readonly RootUst root;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        protected AntlrListenerConverter(TextFile sourceFile, AntlrParserBase antlrParser)
        {
            root = new RootUst(sourceFile, antlrParser.Language);
            convertHelper = new AntlrConvertHelper(root, antlrParser);
            childNodes.Push(new List<Ust>());
        }

        /// <summary>
        /// Calls at the end of conversion process.
        /// </summary>
        public RootUst Complete()
        {
            ProcessChildren();
            root.Node = childNodes.Pop()[0];
            root.FillAscendants(); // TODO: should be filled during conversion
            Clear();
            return root;
        }

        public void Clear()
        {
            childNodes.Clear();
            childNodes.Push(new List<Ust>());
        }

        public void EnterEveryRule(ParserRuleContext ctx)
        {
            childNodes.Push(new List<Ust>());
        }

        public virtual void ExitEveryRule(ParserRuleContext ctx)
        {
            RemoveAndAdd(ProcessChildren());
        }

        public void VisitTerminal(ITerminalNode node)
        {
            childNodes.Peek().Add(convertHelper.Convert(node));
        }

        public void VisitErrorNode(IErrorNode node)
        {
            childNodes.Peek().Add(convertHelper.Convert(node));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int TryGetChildOfType<T>(out T child)
            where T : Ust
        {
            List<Ust> children = childNodes.Peek();

            for (int i = 0; i < childNodes.Count; i++)
            {
                if (children[i] is T ust)
                {
                    child = ust;
                    return i;
                }
            }

            child = default;
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected List<Ust> GetChildren() => childNodes.Peek();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckKeyword(string keywordValue, int index)
            => GetChild(index) is Keyword keyword && keyword.Substring.EqualsIgnoreCase(keywordValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Ust GetChild(int index) => childNodes.Peek()[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Ust GetChildFromEnd(int index)
        {
            List<Ust> nodes = childNodes.Peek();
            return nodes[nodes.Count - 1 - index];
        }

        protected Ust ProcessChildren()
        {
            Ust result;

            List<Ust> nodes = childNodes.Peek();
            if (nodes.Count == 0)
            {
                result = null;
            }
            else if (nodes.Count == 1)
            {
                result = nodes[0];
            }
            else
            {
                var resultCollection = new List<Ust>(nodes.Count);

                // Extract and process previous nodes in common, left-to-right order
                foreach (Ust resultNode in nodes)
                {
                    if (resultNode != null)
                    {
                        resultCollection.Add(resultNode);
                    }
                }

                resultCollection.TrimExcess();

                result = new Collection(resultCollection);
            }

            return result;
        }

        protected void RemoveAndAdd(Ust result)
        {
            List<Ust> nodes = childNodes.Pop();
            if (nodes.Count == 0)
            {
                return;
            }

            foreach (Ust node in nodes)
            {
                if (node != null)
                {
                    node.Parent = result;
                    node.Root = root;
                }
            }

            childNodes.Peek().Add(result);
        }
    }
}
