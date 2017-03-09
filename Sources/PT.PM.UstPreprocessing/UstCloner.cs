using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Ust;

namespace PT.PM.UstPreprocessing
{
    public class UstCloner : UstVisitor, IUstPreprocessor
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Ust Preprocess(Ust ust)
        {
            Ust result;
            result = ust.Type == UstType.Common ? (Ust)new MostCommonUst() : (Ust)new MostDetailUst();
            result.FileName = ust.FileName;
            result.SourceLanguages = ust.SourceLanguages;
            result.Root = (FileNode)Visit(ust.Root);
            result.Comments = ust.Comments.Select(comment => (CommentLiteral)Visit(comment)).ToArray();
            return result;
        }

        protected override UstNode VisitChildren(UstNode ustNode)
        {
            try
            {
                return base.VisitChildren(ustNode);
            }
            catch
            {
                return null;
            }
        }
    }
}
