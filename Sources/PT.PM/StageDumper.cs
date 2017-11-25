using PT.PM.Common.Nodes;
using System.Collections.Generic;
using System.IO;

namespace PT.PM
{
    public class StageDumper
    {
        public string DumpDir { get; set; } = "";

        public HashSet<Stage> Stages { get; set; } = new HashSet<Stage>();

        public WorkflowResult WorkflowResult { get; set; }

        public GraphvizOutputFormat RenderFormat { get; set; }

        public GraphvizDirection RenderDirection { get; set; }

        public bool IncludeHiddenTokens { get; set; } = false;

        public StageDumper(WorkflowResult workflowResult)
        {
            WorkflowResult = workflowResult;
        }

        public void Dump()
        {
            foreach (RootUst ust in WorkflowResult.Usts)
            {
                var renderer = new StageDotRenderer
                {
                    RenderStages = Stages,
                    IncludeHiddenTokens = IncludeHiddenTokens,
                    RenderDirection = RenderDirection
                };

                string dotGraph = renderer.Render(ust);

                string fileName = 
                    (!string.IsNullOrEmpty(Path.GetFileName(ust.SourceCodeFile.Name)) ? ust.SourceCodeFile.Name + "." : "")
                    + "ust";
                var graph = new GraphvizGraph(dotGraph)
                {
                    OutputFormat = RenderFormat
                };
                graph.Dump(Path.Combine(DumpDir, fileName));
            }
        }
    }
}
