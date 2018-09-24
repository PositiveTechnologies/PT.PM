using Esprima;
using Esprima.Ast;
using System.Collections.Generic;

namespace PT.PM.JavaScriptParseTreeUst
{
    class NodeLocationComparer : IComparer<INode>
    {
        public static NodeLocationComparer Instance = new NodeLocationComparer();

        public int Compare(INode x, INode y)
        {
            ref Position xStart = ref x.Location.Start;
            ref Position yStart = ref y.Location.Start;

            int lineComparison = xStart.Line.CompareTo(yStart.Line);

            if (lineComparison != 0)
            {
                return lineComparison;
            }

            return xStart.Column.CompareTo(yStart.Column);
        }
    }
}
