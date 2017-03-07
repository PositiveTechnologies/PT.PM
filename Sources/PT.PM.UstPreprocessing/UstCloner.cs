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

        public Ust Preprocess(Ust ast)
        {
            Ust result;
            result = ast.Type == UstType.Common ? (Ust)new MostCommonUst() : (Ust)new MostDetailUst();
            result.FileName = ast.FileName;
            result.SourceLanguages = ast.SourceLanguages;
            result.Root = (FileNode)Visit(ast.Root);
            result.Comments = ast.Comments.Select(comment => (CommentLiteral)Visit(comment)).ToArray();
            return result;
        }

        protected override UstNode VisitChildren(UstNode astNode)
        {
            try
            {
                return base.VisitChildren(astNode);
            }
            catch
            {
                return null;
            }
        }
    }
}
