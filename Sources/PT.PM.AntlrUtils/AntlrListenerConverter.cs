using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.Common;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;

namespace PT.PM.AntlrUtils
{
    /// <summary>
    /// Generates UST node at each `ExitEveryRule` method
    /// If needed, use concrete Listener with overriden methods for certain contexts
    /// </summary>
    public abstract class AntlrListenerConverter : IParseTreeListener, ILoggable
    {
        private static readonly char[] trimChar = {'\''};

        private int childNodesIndex = -1;
        private readonly List<Usts> childNodes = new List<Usts>();
        private readonly AntlrParserConverter antlrParserConverter;
        private readonly AntlrConvertHelper convertHelper;
        protected readonly RootUst root;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool ParseTreeIsExisted { get; set; } = false;

        protected AntlrListenerConverter(TextFile sourceFile, AntlrParserConverter antlrParser)
        {
            root = new RootUst(sourceFile, antlrParser.Language);
            antlrParserConverter = antlrParser;
            convertHelper = new AntlrConvertHelper(root, antlrParser);
            PushNew(null);
        }

        /// <summary>
        /// Calls at the end of conversion process.
        /// </summary>
        public RootUst Complete()
        {
            ConvertChildren();
            root.Node = Peek()[0];
            Pop();
            Clear();
            root.FillAscendants(); // TODO: should be filled during conversion
            return root;
        }

        public void Clear()
        {
            for (int i = 0; i < childNodesIndex; i++)
            {
                childNodes[i].Clear();
            }
            PushNew(null);
        }

        public void EnterEveryRule(ParserRuleContext ctx)
        {
            PushNew(ctx);
        }

        public virtual void ExitEveryRule(ParserRuleContext ctx)
        {
            RemoveAndAdd(ConvertChildren());
        }

        public void VisitTerminal(ITerminalNode node)
        {
            Add(convertHelper.Convert(node));
        }

        public void VisitErrorNode(IErrorNode node)
        {
            Add(convertHelper.Convert(node));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int TryGetChildOfType<T>(out T child)
            where T : Ust
        {
            List<Ust> children = Peek();

            for (int i = 0; i < children.Count; i++)
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
        protected bool CheckChild<T>(int tokenType, int index)
            where T : class
        {
            List<Ust> children = GetChildren();

            if (index < 0 || index >= children.Count)
            {
                return false;
            }

            if (!(children[index] is T ust))
            {
                return false;
            }

            string tokenValue = antlrParserConverter.LexerVocabulary.GetDisplayName(tokenType).Trim(trimChar);

            string substring = ust.ToString();
            return convertHelper.Language.IsCaseInsensitive()
                ? substring.EqualsIgnoreCase(tokenValue)
                : substring.Equals(tokenValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected List<Ust> GetChildren() => Peek();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Ust GetChild(int index) => Peek()[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Ust GetChildFromEnd(int index)
        {
            List<Ust> nodes = Peek();
            return nodes[nodes.Count - 1 - index];
        }

        protected Ust ConvertChildren()
        {
            Ust result;

            List<Ust> nodes = Peek();
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

                result = new Collection(resultCollection);
            }

            return result;
        }

        protected void RemoveAndAdd(Ust result)
        {
            List<Ust> nodes = Peek();

            foreach (Ust node in nodes)
            {
                if (node != null)
                {
                    node.Parent = result;
                    node.Root = root;
                }
            }

            Pop();
            Add(result);
        }

        protected void PushNew(ParserRuleContext parserRuleContext)
        {
            childNodesIndex = childNodesIndex + 1;
            var newList = new Usts(parserRuleContext);
            if (childNodesIndex >= childNodes.Count)
            {
                childNodes.Add(newList);
            }
            else
            {
                childNodes[childNodesIndex] = newList;
            }
        }

        protected List<Ust> Peek() => childNodes[childNodesIndex];

        protected  void Add(Ust result) => childNodes[childNodesIndex].Add(result);

        protected void Pop()
        {
            childNodes[childNodesIndex].Clear();
            childNodesIndex = childNodesIndex - 1;
        }
    }
}
