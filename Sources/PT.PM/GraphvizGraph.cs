using PT.PM.Common;
using System;
using System.IO;

namespace PT.PM
{
    public class GraphvizGraph : ILoggable
    {
        private static bool? isDotInstalled = null;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public static bool IsDotInstalled
        {
            get
            {
                if (!isDotInstalled.HasValue)
                {
                    try
                    {
                        new Processor("dot", "-V", 1000).Start();
                        isDotInstalled = true;
                    }
                    catch
                    {
                        isDotInstalled = false;
                    }
                }

                return isDotInstalled.Value;
            }
        }

        public string DotGraph { get; set; }

        public bool SaveDot { get; set; } = false;

        public GraphvizOutputFormat OutputFormat { get; set; }

        public GraphvizGraph(string dotGraph)
        {
            DotGraph = dotGraph;
        }

        public void Render(string filePath)
        {
            if (!IsDotInstalled)
            {
                Logger.LogError(new FileNotFoundException("dot tool (Graphviz) is not installed"));
                return;
            }

            string ext = Path.GetExtension(filePath);
            if (ext.Length > 0)
            {
                ext = ext.Substring(1);
            }
            GraphvizOutputFormat outputFormat = OutputFormat;
            string imagePath = filePath;
            bool appendExt = false;
            if (OutputFormat == GraphvizOutputFormat.None)
            {
                if (!Enum.TryParse(ext, true, out outputFormat))
                {
                    outputFormat = GraphvizOutputFormat.Png;
                    imagePath = $"{imagePath}.{outputFormat.ToString().ToLowerInvariant()}";
                    appendExt = true;
                }
            }
            else
            {
                string outputFormatString = OutputFormat.ToString().ToLowerInvariant();
                if (!filePath.EndsWith(outputFormatString))
                {
                    imagePath = $"{imagePath}.{outputFormatString}";
                    appendExt = true;
                }
            }

            string dotFilePath = appendExt ? filePath + ".dot" : Path.ChangeExtension(filePath, "dot");
            File.WriteAllText(dotFilePath, DotGraph);

            var processor = new Processor("dot")
            {
                Arguments = $"\"{dotFilePath}\" -T{outputFormat.ToString().ToLowerInvariant()} -o \"{imagePath}\""
            };
            processor.ErrorDataReceived += (sender, e) =>
            {
                Logger.LogError(new Exception($"Error while graph rendering: {e}"));
            };
            ProcessExecutionResult executionResult = processor.Start();

            if (!SaveDot)
            {
                File.Delete(dotFilePath);
            }

            if (executionResult.ExitCode != 0)
            {
                Logger.LogError(new Exception($"Graph rendering completed with error code {executionResult.ExitCode}"));
            }
        }
    }
}
