using System.Collections.Generic;
using PT.PM.Common.Nodes;

namespace PT.PM.Common.Ust
{
    public class MostCommonUst : Ust
    {
        public override UstType Type => UstType.Common;

        public MostCommonUst()
            : base()
        {
        }

        public MostCommonUst(FileNode fileNode, LanguageFlags sourceLanguages)
            : base(fileNode, sourceLanguages)
        {
        }
    }
}
