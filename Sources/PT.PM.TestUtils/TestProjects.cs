﻿namespace PT.PM.TestUtils
{
    public static class TestProjects
    {
        public static TestProject[] CSharpProjects = new TestProject[]
        {
            new TestProject("WebGoat.NET-1c6cab")
            {
                Urls = new [] {
                    $"{TestUtility.GithubUrlPrefix}jerryhoff/WebGoat.NET/archive/1c6cab19f9029673cd98ba8624bf9cc91d04bae9.zip"
                }
            },
            new TestProject("corefx-1.0.0-rc2")
            {
                Urls = new [] { $"{TestUtility.GithubUrlPrefix}dotnet/corefx/archive/release/1.0.0-rc2.zip" },
                IgnoredFiles = new [] {
                    @"src\Common\tests\System\Xml\XmlCoreTest\TestData.g.cs"
                }
            },
            new TestProject("EntityFramework-7.0.0-rc1")
            {
                Urls = new [] {
                   $"{TestUtility.GithubUrlPrefix}aspnet/EntityFramework/archive/7.0.0-rc1.zip"
                }
            },
            new TestProject("aspnet-mvc-6.0.0-rc1")
            {
                Urls = new []
                {
                    $"{TestUtility.GithubUrlPrefix}aspnet/Mvc/archive/6.0.0-rc1.zip"
                }
            },
            new TestProject("ImageProcessor-2.3.0",
                $"{TestUtility.GithubUrlPrefix}JimBobSquarePants/ImageProcessor/archive/2.3.0.zip"),
            new TestProject("Newtonsoft.Json-8.0.2",
                $"{TestUtility.GithubUrlPrefix}/JamesNK/Newtonsoft.Json/archive/8.0.2.zip")
        };

        public static TestProject[] TSqlProjects = new TestProject[]
        {
            new TestProject("TSQL Samples")
            {
                // TODO: Add samples from https://github.com/antlr/codebuff/tree/master/corpus/sql/training
                Urls = new string[0],
            }
        };

        public static TestProject[] JavaScriptProjects = new TestProject[]
        {
            new TestProject("bootstrap-v3.3.7")
            {
                Urls = new []
                {
                    $"{TestUtility.GithubUrlPrefix}twbs/bootstrap/archive/v3.3.7.zip"
                }
            },
            new TestProject("JavaScript-Style-Guide-v14.0.0")
            {
                Urls = new []
                {
                    $"{TestUtility.GithubUrlPrefix}airbnb/javascript/archive/eslint-config-airbnb-v14.0.0.zip"
                },
                IgnoredFiles = new []
                {
                    // ES6 syntax
                    @"packages\eslint-config-airbnb\test\test-base.js",
                    @"packages\eslint-config-airbnb\test\test-react-order.js",
                    @"packages\eslint-config-airbnb-base\test\test-base.js"
                }
            },
            // TODO: Maybe add https://github.com/getify/You-Dont-Know-JS project
        };
    }
}
