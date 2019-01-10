using PT.PM.Common.Utils;
using System.IO;
using PT.PM.Common.Files;

namespace PT.PM.Common
{
    public abstract class ParseTreeDumper
    {
        public static string TokensSuffix { get; set; } = "tokens.txt";

        public virtual string ParseTreeSuffix => "parseTree.json";

        public static string UstSuffix { get; set; } = "ust.json";

        public string DumpDir { get; set; } = "";

        public int IndentSize { get; set; } = 2;

        public bool IncludeTextSpans { get; set; }

        public bool IsLineColumn { get; set; }

        public int MaxTokenValueLength { get; set; } = 16;

        public TokenValueDisplayMode TokenValueDisplayMode { get; set; } = TokenValueDisplayMode.Show;

        public bool EachTokenOnNewLine { get; set; } = true;

        public bool OnlyCommonTokens { get; set; } = false;

        public abstract void DumpTokens(ParseTree parseTree);

        public abstract void DumpTree(ParseTree parseTree);

        protected void Dump(string data, CodeFile sourceCodeFile, bool tokens)
        {
            DirectoryExt.CreateDirectory(DumpDir);
            string name = string.IsNullOrEmpty(sourceCodeFile.Name) ? "" : sourceCodeFile.Name + ".";
            FileExt.WriteAllText(Path.Combine(DumpDir, $"{name}{(tokens ? TokensSuffix : ParseTreeSuffix)}"), data);
        }
    }
}
