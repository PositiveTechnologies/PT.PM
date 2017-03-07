using System.Collections.Generic;

namespace PT.PM.Common.CodeRepository
{
    public interface ISourceCodeRepository : ILoggable
    {
        string Path { get; set; }

        IEnumerable<string> GetFileNames();

        SourceCodeFile ReadFile(string fileName);

        string GetFullPath(string relativePath);
    }
}
