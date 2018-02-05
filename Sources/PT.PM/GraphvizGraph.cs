using PT.PM.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PT.PM
{
    public class GraphvizGraph
    {
        public static string GraphvizPath { get; set; }

        public string DotGraph { get; set; }

        public bool SaveDot { get; set; } = false;

        public GraphvizOutputFormat OutputFormat { get; set; }

        static GraphvizGraph()
        {
            string executingAssemblyPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            GraphvizPath = CommonUtils.IsRunningOnLinux
                ? "dot"
                : Path.Combine(executingAssemblyPath, "Graphviz", "dot.exe");
        }

        public GraphvizGraph(string dotGraph)
        {
            DotGraph = dotGraph;
        }

        public void Render(string filePath)
        {
            if (!File.Exists(GraphvizPath))
            {
                throw new Exception($"dot.exe has not been found at {GraphvizPath}");
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
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = GraphvizPath,
                Arguments = $"\"{dotFilePath}\" -T{outputFormat.ToString().ToLowerInvariant()} -o \"{imagePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            process.WaitForExit();
            if (!SaveDot)
            {
                File.Delete(dotFilePath);
            }
            var errors = process.StandardError.ReadToEnd();
            if (process.ExitCode != 0 || errors != "")
            {
                throw new Exception($"Error while graph rendering. Errors: {errors}");
            }
        }
    }
}
