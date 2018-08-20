using PT.PM.Common;
using PT.PM.Common.Utils;
using System.IO.Compression;

namespace PT.PM
{
    public class StandardArchiveExtractor : IArchiveExtractor
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public void Extract(string zipPath, string extractPath)
        {
            ZipFile.ExtractToDirectory(zipPath.NormalizeFilePath(), extractPath.NormalizeDirPath());
        }
    }
}
