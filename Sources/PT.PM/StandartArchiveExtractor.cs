using PT.PM.Common;
using System.IO.Compression;

namespace PT.PM
{
    public class StandartArchiveExtractor : IArchiveExtractor
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public void Extract(string zipPath, string extractPath)
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }
    }
}
