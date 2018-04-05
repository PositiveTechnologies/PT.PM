using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PT.PM
{
    public class StageRenderer : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string DumpDir { get; set; } = "";

        public HashSet<Stage> Stages { get; set; } = new HashSet<Stage>();

        public WorkflowResult WorkflowResult { get; set; }

        public GraphvizOutputFormat RenderFormat { get; set; }

        public GraphvizDirection RenderDirection { get; set; }

        public bool IncludeHiddenTokens { get; set; } = false;

        public StageRenderer(WorkflowResult workflowResult)
        {
            WorkflowResult = workflowResult;
        }

        public void Render()
        {

            foreach (RootUst ust in WorkflowResult.Usts)
            {
                try
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
                    graph.Render(Path.Combine(DumpDir, fileName));
                }
                catch (Exception ex) when (!(ex is ThreadAbortException))
                {
                    Logger.LogError(ex);
                }
            }
        }
    }
}
