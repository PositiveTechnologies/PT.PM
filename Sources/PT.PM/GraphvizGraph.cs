using System;
using System.IO;

namespace PT.PM
{
    public class GraphvizGraph
    {
        private static bool? isDotInstalled = null;

        public static bool IsDotInstalled
        {
            get
            {
                if (!isDotInstalled.HasValue)
                {
                    isDotInstalled = ProcessUtils.IsToolExists("dot", "-V");
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
                throw new FileNotFoundException("dot tool (Graphviz) is not installed");
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

            ProcessExecutionResult executionResult = ProcessUtils.SetupHiddenProcessAndStart("dot",
                $"\"{dotFilePath}\" -T{outputFormat.ToString().ToLowerInvariant()} -o \"{imagePath}\"");

            if (!SaveDot)
            {
                File.Delete(dotFilePath);
            }

            if (executionResult.ExitCode != 0 || executionResult.Errors.Count > 0)
            {
                string errorsString = string.Join(Environment.NewLine, executionResult.Errors);
                throw new Exception($"Error while graph rendering. Errors: {errorsString}");
            }
        }
    }
}
