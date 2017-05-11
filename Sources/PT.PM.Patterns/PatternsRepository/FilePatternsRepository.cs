using System.IO;

namespace PT.PM.Patterns.PatternsRepository
{
    public class FilePatternsRepository : JsonPatternsRepository
    {
        public FilePatternsRepository(string filePath)
            : base(File.ReadAllText(filePath))
        {
            Path = filePath;
        }
    }
}
