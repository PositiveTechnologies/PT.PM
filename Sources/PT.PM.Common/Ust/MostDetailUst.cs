using System.Collections.Generic;
using PT.PM.Common.Nodes;

namespace PT.PM.Common.Ust
{
    public class MostDetailUst : Ust
    {
        public override UstType Type => UstType.Detail;

        public MostDetailUst()
            : base()
        {
        }

        public MostDetailUst(FileNode fileNode, LanguageFlags sourceLanguages)
            : base(fileNode, sourceLanguages)
        {
        }
    }
}
