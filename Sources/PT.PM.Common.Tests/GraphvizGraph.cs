using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Tests
{
    public class GraphvizGraph
    {
        public string GraphvizPath { get; set; } = TestHelper.GraphvizPath;

        public string DotGraph { get; set; }

        public GraphvizGraph(string dotGraph)
        {
            DotGraph = dotGraph;
        }

        public void Dump(string filePath)
        {
            var tempGraphViz = Path.GetTempFileName();
            File.WriteAllText(tempGraphViz, DotGraph);
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = GraphvizPath,
                Arguments = "\"" + tempGraphViz + "\" -Tpng -o \"" + filePath + "\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            process.WaitForExit();
            File.Delete(tempGraphViz);
            var errors = process.StandardError.ReadToEnd();
            if (process.ExitCode != 0 || errors != "")
            {
                throw new Exception($"Error while graph rendering. Errors: {errors}");
            }
        }
    }
}
