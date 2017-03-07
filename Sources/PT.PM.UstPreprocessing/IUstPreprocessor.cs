using PT.PM.Common;
using PT.PM.Common.Ust;

namespace PT.PM.UstPreprocessing
{
    public interface IUstPreprocessor : ILoggable
    {
        Ust Preprocess(Ust ast);
    }
}
