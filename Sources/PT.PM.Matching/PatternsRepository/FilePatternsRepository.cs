using System.IO;

namespace PT.PM.Matching.PatternsRepository
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
