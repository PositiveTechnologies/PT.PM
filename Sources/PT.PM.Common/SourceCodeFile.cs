using System.IO;

namespace PT.PM.Common
{
    public class SourceCodeFile
    {
        public string Name { get; set; }

        public string RelativePath { get; set; } = "";

        public string Code { get; set; }

        public string FullPath => Path.Combine(RelativePath, Name);

        public SourceCodeFile()
        {
        }

        public SourceCodeFile(string name)
        {
            Name = name;
        }

        public SourceCodeFile(SourceCodeFile sourceCodeFile)
        {
            Name = sourceCodeFile.Name;
            RelativePath = sourceCodeFile.RelativePath;
            Code = sourceCodeFile.Code;
        }

        public LineColumnTextSpan GetLineColumnTextSpan(TextSpan textSpan)
        {
            // TODO: replace with fast binary version.
            int beginLine, beginColumn;
            int endLine, endColumn;
            TextHelper.LinearToLineColumn(textSpan.Start, Code, out beginLine, out beginColumn);
            TextHelper.LinearToLineColumn(textSpan.End, Code, out endLine, out endColumn);
            return new LineColumnTextSpan(beginLine, beginColumn, endLine, endColumn);
        }
    }
}
