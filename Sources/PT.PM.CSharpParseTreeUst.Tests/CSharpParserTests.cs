using System.IO;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Tests;
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;

namespace PT.PM.CSharpParseTreeUst.Tests
{
    [TestFixture]
    public class CSharpParserTests
    {
        [TestCase("AllInOne.cs")]
        public void Parse_CSharpWithRoslyn(string fileName)
        {
            TestHelper.CheckFile(fileName, Language.CSharp, Stage.Parse);
        }

        [Test]
        public void Parse_SyntaxErrorFileCSharp_CatchErrors()
        {
            var logger = new LoggerMessageCounter();
            TestHelper.CheckFile("ParseError.cs", Language.CSharp, Stage.Parse, logger, true);

            Assert.AreEqual(7, logger.ErrorCount);
        }

        [TestCase("WebGoat.NET-1c6cab")]
        [TestCase("roslyn-1.1.1")]
        public void Parse_NETProject_WithoutErrors(string projectKey)
        {
            TestHelper.CheckProject(TestProjects.CSharpProjects
                .Single(p => p.Key == projectKey), Language.CSharp, Stage.Parse);
        }
    }
}
