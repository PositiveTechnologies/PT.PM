using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
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

            fileName = fileName.Replace(".txt", "") + ".ust";
            File.WriteAllText(TestHelper.CombineWithOutputDir(fileName + ".dot"), dotString);
            TestHelper.RenderGraphvizGraph(dotString, TestHelper.CombineWithOutputDir(fileName + ".png"));
        }
    }
}
