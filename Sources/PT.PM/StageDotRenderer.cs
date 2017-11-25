using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using System.Text;
using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM
{
    public class StageDotRenderer
    {
        private StringBuilder vertexesString;
        private StringBuilder edgesString;
        private int currentIndex;

        // TODO: Support for parse tree.
        public HashSet<Stage> RenderStages { get; set; } = new HashSet<Stage>
        {
            Stage.Ust,
        };

        public GraphvizDirection RenderDirection { get; set; } = GraphvizDirection.TopBottom;

        public bool IncludeHiddenTokens { get; set; } = false;

        public string Render(Ust ust)
        {
            edgesString = new StringBuilder();
            vertexesString = new StringBuilder();
            currentIndex = 0;

            RenderNode(ust);

            var graphString = new StringBuilder();
            graphString.AppendLine("digraph cfg {");
            graphString.Append(vertexesString.ToString());
            graphString.Append(edgesString.ToString());
            graphString.AppendLine("}");

            return graphString.ToString();
        }

        private void RenderNode(Ust ust)
        {
            int index = currentIndex;
            string typeName = DotFormatUtils.TrimAndEscape(ust.GetType().Name);

            string labelName, tooltip;
            if (ust is ArgsUst ||
                ust is BlockStatement ||
                ust is ExpressionStatement ||
                ust is ConditionalExpression)
            {
                labelName = typeName;
                tooltip = DotFormatUtils.EscapeString(ust.ToString());
            }
            else
            {
                labelName = DotFormatUtils.TrimAndEscape(ust.ToString());
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
