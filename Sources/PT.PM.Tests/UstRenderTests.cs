using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Tests;
using NUnit.Framework;
using System.IO;

namespace PT.PM.Tests
{
    [TestFixture]
    public class UstRenderTests
    {
        [TestCase("TaintPoc.cs.txt")]
        public void Render_Ust_PngGraph(string fileName)
        {
            var codeRepository = new FileCodeRepository(Path.Combine(TestHelper.TestsDataPath, fileName));

            var language = (Language)LanguageExt.GetLanguageFromFileName(fileName);
            var workflow = new Workflow(codeRepository, language);
            workflow.Stage = Stage.Convert;
            WorkflowResult workflowResult = workflow.Process();

            var astSerializer = new UstDotRenderer();
            var dotString = astSerializer.Render(workflowResult.LastUst.Root);

            TestHelper.RenderGraphvizGraph(dotString, Path.Combine(
                TestHelper.TestsDataPath,
                fileName.Replace(".txt", "") + ".uast.png"));
        }
    }
}
