using System.Collections.Generic;
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
        private static readonly char[] trimChars = {'\''};

        private int childrenNodesIndex = -1;
        private readonly List<Usts> childrenNodes = new List<Usts>();
        private readonly AntlrParserConverter antlrParserConverter;
        private readonly AntlrConvertHelper convertHelper;
        protected readonly RootUst root;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IsParseTreeExists { get; set; }

        protected AntlrListenerConverter(TextFile sourceFile, AntlrParserConverter antlrParser)
        {
            root = new RootUst(sourceFile, antlrParser.Language);
            antlrParserConverter = antlrParser;
            convertHelper = new AntlrConvertHelper(root, antlrParser);
            PushNew(null);
        }

        /// <summary>
        /// Gets called at the end of conversion process
        /// </summary>
        public RootUst Complete()
        {
            ConvertChildren();
            root.Node = GetChildren()[0];
            PopChildren();
            Clear();
            root.FillParentAndRootForDescendantsAndSelf(); // TODO: should be filled during conversion
            return root;
        }

        public void Clear()
        {
            for (int i = 0; i <= childrenNodesIndex; i++)
            {
                childrenNodes[i].Clear();
            }
            childrenNodesIndex = -1;
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
            AddChild(convertHelper.Convert(node));
        }

        public void VisitErrorNode(IErrorNode node)
        {
            AddChild(convertHelper.Convert(node));
        }

        protected int TryGetChildOfType<T>(out T child)
            where T : Ust
        {
            List<Ust> children = GetChildren();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is T ust)
                {
                    child = ust;
                    return i;
                }
            }

            child = null;
            return -1;
        }

        protected bool CheckChild<T>(int[] tokenTypes, int index)
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

            string substring = ust.ToString();

            foreach (int tokenType in tokenTypes)
            {
                string tokenValue = antlrParserConverter.LexerVocabulary.GetDisplayName(tokenType).Trim(trimChars);
                bool result = convertHelper.Language.IsCaseInsensitive()
                    ? substring.EqualsIgnoreCase(tokenValue)
                    : substring.Equals(tokenValue);

                if (result)
                {
                    return true;
                }
            }

            return false;
        }

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

            string tokenValue = antlrParserConverter.LexerVocabulary.GetDisplayName(tokenType).Trim(trimChars);

            string substring = ust.ToString();
            return convertHelper.Language.IsCaseInsensitive()
                ? substring.EqualsIgnoreCase(tokenValue)
                : substring.Equals(tokenValue);
        }

        protected Ust GetChildFromEnd(int index)
        {
            List<Ust> children = GetChildren();
            return children[children.Count - 1 - index];
        }

        protected Ust ConvertChildren()
        {
            Ust result;

            List<Ust> children = GetChildren();
            if (children.Count == 0)
            {
                result = null;
            }
            else if (children.Count == 1)
            {
                result = children[0];
            }
            else
            {
                var resultCollection = new List<Ust>(children.Count);

                // Extract and process previous nodes in common, left-to-right order
                foreach (Ust resultNode in children)
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

        protected Ust GetLeftChildFromLeftRecursiveRule(ParserRuleContext context)
        {
            Ust leftChild;

            if (!IsParseTreeExists)
            {
                PopChildren();
                var children = GetChildren();
                leftChild = GetChild(children.Count - 1);
                children.RemoveAt(children.Count - 1);
                PushNew(context);
            }
            else
            {
                leftChild = GetChild(0);
            }

            return leftChild;
        }

        protected void RemoveAndAdd(Ust result)
        {
            List<Ust> children = GetChildren();

            foreach (Ust child in children)
            {
                if (child != null)
                {
                    child.Parent = result;
                    child.Root = root;
                }
            }

            PopChildren();
            AddChild(result);
        }

        protected Ust GetChild(int index) => childrenNodes[childrenNodesIndex][index];

        protected List<Ust> GetChildren() => childrenNodes[childrenNodesIndex];

        private void AddChild(Ust result) => childrenNodes[childrenNodesIndex].Add(result);

        private void PopChildren()
        {
            childrenNodes[childrenNodesIndex].Clear();
            childrenNodesIndex = childrenNodesIndex - 1;
        }

        private void PushNew(ParserRuleContext parserRuleContext)
        {
            childrenNodesIndex = childrenNodesIndex + 1;
            var newList = new Usts(parserRuleContext);
            if (childrenNodesIndex >= childrenNodes.Count)
            {
                childrenNodes.Add(newList);
            }
            else
            {
                childrenNodes[childrenNodesIndex] = newList;
            }
        }
    }
}
