using PT.PM.Common;

namespace PT.PM
{
    public interface IArchiveExtractor : ILoggable
    {
        void Extract(string zipPath, string extractPath);
    }
}
