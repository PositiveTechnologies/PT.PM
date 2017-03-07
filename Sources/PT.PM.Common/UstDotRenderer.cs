using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using System.Text;
namespace PT.PM.Common
{
    public class UstDotRenderer: INodeRenderer<UstNode>
    {
        private StringBuilder vertexesString;
        private StringBuilder edgesString;
        private int currentIndex;

        public string Render(UstNode node)
        {
            edgesString = new StringBuilder();
            vertexesString = new StringBuilder();
            currentIndex = 0;

            RenderNode(node);

            var graphString = new StringBuilder();
            graphString.AppendLine("digraph cfg {");
            graphString.Append(vertexesString.ToString());
            graphString.Append(edgesString.ToString());
            graphString.AppendLine("}");

            return graphString.ToString();
        }

        private void RenderNode(UstNode node)
        {
            int index = currentIndex;
            vertexesString.Append(index + " [label=\"" + DotFormatHelper.GetNodeName(node) + "\"");
            
            if (node is Token)
            {
                vertexesString.Append(", fillcolor=khaki, style=filled");
            }
            else if (node is Expression)
            {
                vertexesString.Append(", fillcolor=rosybrown1, style=filled");
            }
            else if (node is Statement)
            {
                vertexesString.Append(", fillcolor=skyblue1, style=filled");
            }
            else if (node is ArgsNode)
            {
                vertexesString.Append(", fillcolor=palegreen, style=filled");
            }
            vertexesString.AppendLine("];");

            foreach (var child in node.Children)
            {
                if (child != null && child.NodeType != NodeType.FileNode)
                {
                    currentIndex++;
                    edgesString.AppendEdge(index, currentIndex);
                    RenderNode(child);
                }
            }
        }
    }
}
