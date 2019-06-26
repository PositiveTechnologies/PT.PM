using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using System.Collections.Generic;
using System.Text;

namespace PT.PM
{
    public class StageDotRenderer
    {
        private PrettyPrinter graphNodePrinter = new PrettyPrinter
        {
            MaxMessageLength = 30,
            ReduceWhitespaces = true,
            CutWords = false,
            Escape = true
        };

        private PrettyPrinter graphTooltipPrinter = new PrettyPrinter
        {
            MaxMessageLength = 200,
            ReduceWhitespaces = true,
            CutWords = false,
            Escape = true
        };

        private StringBuilder vertexesString;
        private StringBuilder edgesString;
        private int currentIndex;

        // TODO: Support for parse tree.
        public HashSet<Stage> RenderStages { get; set; } = new HashSet<Stage> { Stage.Ust };

        public GraphvizDirection RenderDirection { get; set; } = GraphvizDirection.TopBottom;

        public bool IncludeHiddenTokens { get; set; } = false;

        public string Render(Ust ust)
        {
            edgesString = new StringBuilder();
            vertexesString = new StringBuilder();
            currentIndex = 0;

            RenderNode(ust);

            var graphString = new StringBuilder();
            graphString.AppendLine("digraph ust {");
            if (RenderDirection != GraphvizDirection.TopBottom)
            {
                string rankdir;
                if (RenderDirection == GraphvizDirection.BottomTop)
                {
                    rankdir = "BT";
                }
                else if (RenderDirection == GraphvizDirection.LeftRight)
                {
                    rankdir = "LR";
                }
                else if (RenderDirection == GraphvizDirection.RightLeft)
                {
                    rankdir = "RL";
                }
                else
                {
                    rankdir = "TB";
                }
                graphString.AppendLine($"rankdir={rankdir};");
            }
            graphString.Append(vertexesString);
            graphString.Append(edgesString);
            graphString.AppendLine("}");

            return graphString.ToString();
        }

        private void RenderNode(Ust ust)
        {
            int index = currentIndex;
            string typeName = graphTooltipPrinter.Print(ust.GetType().Name);

            string labelName, tooltip;
            if (ust is ArgsUst ||
                ust is BlockStatement ||
                ust is ExpressionStatement ||
                ust is ConditionalExpression)
            {
                labelName = typeName;
                tooltip = graphTooltipPrinter.Print(ust.ToString());
            }
            else
            {
                labelName = graphNodePrinter.Print(ust.ToString());
                tooltip = typeName;
            }
            vertexesString.Append($@"{index} [label=""{labelName}""");
            vertexesString.Append($@", tooltip=""{tooltip}""");

            /*if (ust is Token)
            {
                vertexesString.Append(", fillcolor=khaki, style=filled");
            }
            else if (ust is Expression)
            {
                vertexesString.Append(", fillcolor=rosybrown1, style=filled");
            }
            else if (ust is Statement)
            {
                vertexesString.Append(", fillcolor=skyblue1, style=filled");
            }
            else if (ust is ArgsUst)
            {
                vertexesString.Append(", fillcolor=palegreen, style=filled");
            }*/
            vertexesString.AppendLine("];");

            foreach (Ust child in ust.Children)
            {
                if (child != null)
                {
                    currentIndex++;
                    edgesString.AppendEdge(index, currentIndex);
                    RenderNode(child);
                }
            }
        }
    }
}
