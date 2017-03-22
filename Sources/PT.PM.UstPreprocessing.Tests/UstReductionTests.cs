using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.UstPreprocessing.Tests
{
    [TestFixture]
    public class UstReductionTests
    {
        [Test]
        public void Reduct_BinaryAssignmentOperations_Reducted()
        {
            var sourceCodeRepositories = new Dictionary<Language, string>()
            {
                [Language.CSharp] =
@"class Test
{
    void Main()
    {
        int a = 40;
        a += 2;
    }
}",
                [Language.Java] =
@"class Test
{
    void main()
    {
        int a = 40;
        a += 2;
    }
}",
                [Language.Php] =
@"<?php
    $a = 40;
    $a += 2;",
            };

            foreach (var repository in sourceCodeRepositories)
            {
                var logger = new LoggerMessageCounter();
                var workflow = new Workflow(new MemoryCodeRepository(repository.Value), repository.Key, stage: Stage.Preprocess);
                workflow.UstPreprocessor = new UstPreprocessor();
                workflow.Logger = logger;
                workflow.Stage = Stage.Convert;
                WorkflowResult workflowResult = workflow.Process();

                Assert.IsTrue(workflowResult.Usts.First().Root.ToString().Contains("a = a + 2"));
            }
        }
    }
}
