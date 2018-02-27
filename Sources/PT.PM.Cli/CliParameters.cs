using CommandLine;
using System;
using System.IO;

namespace PT.PM.Cli
{
    public class CliParameters
    {
        [Option('f', "files", HelpText = "Input file or directory to be processed")]
        public string InputFileNameOrDirectory { get; set; } = "";

        [Option('l', "languages", HelpText = "Languages to be processed")]
        public string Languages { get; set; } = "";

        [Option("patterns", HelpText = "Patterns to be processed")]
        public virtual string Patterns { get; set; } = "";

        [Option("pattern-ids", HelpText = "Pattern identifiers to be processed")]
        public string PatternIds { get; set; } = "";

        [Option('t', "threads", HelpText = "Number of processing threads")]
        public int ThreadCount { get; set; } = 1;

        [Option("not-preprocess-ust", HelpText = "Do not include ust simplification stage")]
        public bool NotPreprocessUst { get; set; } = false;

        [Option('s', "stage", HelpText = "End processing stage. By default: Match")]
        public string Stage { get; set; } = PM.Stage.Match.ToString();

        [Option("max-stack-size", HelpText = "Thread stack size in bytes")]
        public int MaxStackSize { get; set; } = 0;

        [Option('m', "memory", HelpText = "Approximate max memory consumption in megabytes")]
        public int Memory { get; set; } = 300;

        [Option("logs-dir", HelpText = "Logs directory")]
        public string LogsDir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

        [Option("temp-dir", HelpText = "Temp directory")]
        public string TempDir { get; set; } = Path.GetTempPath();

        [Option('e', "log-errors", HelpText = "Log errors to console")]
        public bool IsLogErrors { get; set; } = false;

        [Option("log-debugs", HelpText = "Log debug messages")]
        public bool IsLogDebugs { get; set; } = false;

        [Option("no-indents", HelpText = "Do not indent dump trees")]
        public bool NoIndentedDump { get; set; } = false;

        [Option("no-text-spans", HelpText = "Do not include text spans in dump trees")]
        public bool NotIncludeTextSpansInDump { get; set; } = false;

        [Option("line-column", HelpText = "Use line-column format for text spans in dump")]
        public bool LineColumnTextSpans { get; set; } = false;

        [Option("start-stage", HelpText = "Start stage to process (File or Ust)")]
        public string StartStage { get; set; } = "";

        [Option('d', "dump", HelpText = "Stages to be dumped (ParseTree, Ust)")]
        public string DumpStages { get; set; } = "";

        [Option('r', "render", HelpText = "Stages to be rendered")]
        public string RenderStages { get; set; } = "";

        [Option("render-format", HelpText = "Graph render format (Png, Svg, etc.)")]
        public string RenderFormat { get; set; } = GraphvizOutputFormat.Png.ToString();

        [Option("render-direction", HelpText = "Graph render direction (TopBottom, LeftRight, etc.)")]
        public string RenderDirection { get; set; } = GraphvizDirection.TopBottom.ToString();
    }
}
