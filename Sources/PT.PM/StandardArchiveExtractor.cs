using PT.PM.Common;
using System.IO.Compression;

namespace PT.PM
{
    public class StandardArchiveExtractor : IArchiveExtractor
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public void Extract(string zipPath, string extractPath)
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }
    }
}
