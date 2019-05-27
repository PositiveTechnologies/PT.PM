using System;
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
    public class AntlrListenerConverter : IParseTreeListener, ILoggable
    {
        // We should store children count for every ascendant node and process such number in ProcessChildren
        // After processing, such nodes should be removed from `resultNodes`
        protected Stack<int> ChildCount { get; } = new Stack<int>(64);

        // Stack of created children
        protected readonly List<Ust> resultNodes = new List<Ust>();

        protected readonly RootUst root;
        protected readonly AntlrConvertHelper convertHelper;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public AntlrListenerConverter(TextFile sourceFile, AntlrParserBase antlrParser)
        {
            root = new RootUst(sourceFile, antlrParser.Language);
            convertHelper = new AntlrConvertHelper(root, antlrParser);
            ChildCount.Push(0);
        }

        /// <summary>
        /// Calls at the end of conversion process.
        /// </summary>6
        public RootUst Complete()
        {
            ProcessChildren();
            root.Node = resultNodes[resultNodes.Count - 1];
            root.FillAscendants(); // TODO: should be filled during conversion
            Clear();
            return root;
        }

        public void Clear()
        {
            resultNodes.Clear();
            ChildCount.Clear();
            ChildCount.Push(0);
        }

        /// <summary>
        /// Saves child count for every context
        /// </summary>
        public void EnterEveryRule(ParserRuleContext ctx)
        {
            ChildCount.Push(ChildCount.Pop() + 1);
            ChildCount.Push(0);
        }

        public virtual void ExitEveryRule(ParserRuleContext ctx)
        {
            int childCount = ChildCount.Peek();

            if (childCount != 1)
            {
                Ust result = ProcessChildren();
                RemoveAndAdd(result);
            }
        }

        public void VisitTerminal(ITerminalNode node)
        {
            ChildCount.Push(ChildCount.Pop() + 1);

            resultNodes.Add(convertHelper.Convert(node));
        }

        public void VisitErrorNode(IErrorNode node)
        {
            ChildCount.Push(ChildCount.Pop() + 1);

            ReadOnlySpan<char> span = node.Symbol.ExtractSpan(out TextSpan textSpan);
            resultNodes.Add(convertHelper.ConvertToken(span, textSpan));
        }

        protected int GetKeywordIndex(string keywordValue, int childCount)
        {
            for (int i = resultNodes.Count - childCount; i < resultNodes.Count; i++)
            {
                if (resultNodes[i] is Keyword keyword && keyword.Substring.EqualsIgnoreCase(keywordValue))
                {
                    return i - (resultNodes.Count - childCount);
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int TryGetChildOfType<T>(out T child)
            where T : Ust
        {
            int childCount = ChildCount.Peek();
            for (int i = resultNodes.Count - childCount; i < resultNodes.Count; i++)
            {
                if (resultNodes[i] is T ust)
                {
                    child = ust;
                    return i - (resultNodes.Count - childCount);
                }
            }

            child = default;
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckKeyword(string keywordValue, int index)
            => GetChild(index) is Keyword keyword && keyword.Substring.EqualsIgnoreCase(keywordValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckKeywordFromEnd(string keywordValue, int index)
            => GetChildFromEnd(index) is Keyword keyword && keyword.Substring.EqualsIgnoreCase(keywordValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Ust GetChild(int index) => resultNodes[resultNodes.Count - ChildCount.Peek() + index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Ust GetChildFromEnd(int index) => resultNodes[resultNodes.Count - 1 - index];

        protected Ust ProcessChildren()
        {
            Ust result;

            int childCount = ChildCount.Peek();
            if (childCount == 0)
            {
                result = null;
            }
            else if (childCount == 1)
            {
                result = resultNodes[resultNodes.Count - 1];
            }
            else
            {
                var resultCollection = new List<Ust>(childCount);

                // Extract and process previous nodes in common, left-to-right order
                for (int i = resultNodes.Count - childCount; i < resultNodes.Count; i++)
                {
                    Ust resultNode = resultNodes[i];
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
            int removeCount = ChildCount.Pop();
            if (removeCount == 0)
            {
                return;
            }

            for (int i = resultNodes.Count - removeCount; i < resultNodes.Count; i++)
            {
                Ust resultNode = resultNodes[i];
                if (resultNode != null)
                {
                    resultNode.Root = root;
                    resultNode.Parent = result;
                }
            }

            // Remove processed children except the first one
            int countToRemove = removeCount - 1;
            resultNodes.RemoveRange(resultNodes.Count - countToRemove, countToRemove);

            // Replace it with the new value
            resultNodes[resultNodes.Count - 1] = result;
        }
    }
}