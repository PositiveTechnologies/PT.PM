using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Tests;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.JavaUstConversion.Tests
{
    [TestFixture]
    public class JavaParserTests
    {
        [Test]
        public void Parse_SyntaxErrorFileJava_CatchErrors()
        {
            TestHelper.CheckFile("ParseError.java", Language.Java, Stage.Parse, shouldContainsErrors:true);
        }

        [TestCase("WebGoat.Java-05a1f5")]
        public void Parse_JavaProject_WithoutErrors(string projectKey)
        {
            TestHelper.CheckProject(TestProjects.JavaProjects
                .Single(p => p.Key == projectKey), Language.Java, Stage.Parse);
        }
    }
}
