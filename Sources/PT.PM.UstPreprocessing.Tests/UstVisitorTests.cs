﻿using PT.PM.UstPreprocessing;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Tests;
using PT.PM.Patterns.Nodes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.UstPreprocessing.Tests
{
    [TestFixture]
    public class UstVisitorTests
    {
        // TODO: Complete this test.
        [Test]
        public void VisitUst_WebGoatPhp_VisitedEqualToOrig()
        {
            Assert.Inconclusive("Not completed");
            var preprocessor = new UstCloner();
            TestHelper.CheckProject(
                TestProjects.PhpProjects.Single(p => p.Key == "WebGoatPHP-6f48c9"), Language.Php, Stage.Preprocess, preprocessor);
        }

        [Test]
        public void Check_IAstVisitor_AllVisitMethodsExists()
        {
            MethodInfo[] iAstVisitorMethods = typeof(IUstVisitor<>).GetMethods();
            IEnumerable<Type> allAstNodeTypes = ListenerVisitorUtils.GetAssemblyAstNodeTypes(typeof(UstNode), typeof(PatternVarDef));
            foreach (Type type in allAstNodeTypes)
            {
                Assert.IsTrue(iAstVisitorMethods
                    .FirstOrDefault(methodInfo =>
                    {
                        var parameters = methodInfo.GetParameters();
                        return parameters.Length > 0 && parameters[0].ParameterType == type;
                    }) != null,
                    $"Visitor for Type {type} is not exists");
            }
        }
    }
}
