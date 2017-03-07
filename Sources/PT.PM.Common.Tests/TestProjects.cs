using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Tests
{
    public static class TestProjects
    {
        public static TestProject[] CSharpProjects = new TestProject[]
        {
            new TestProject("WebGoat.NET-1c6cab")
            {
                Urls = new [] {
                    $"{TestHelper.GithubUrlPrefix}jerryhoff/WebGoat.NET/archive/1c6cab19f9029673cd98ba8624bf9cc91d04bae9.zip"
                }
            },
            new TestProject("corefx-1.0.0-rc2")
            {
                Urls = new [] { $"{TestHelper.GithubUrlPrefix}dotnet/corefx/archive/release/1.0.0-rc2.zip" },
                IgnoredFiles = new [] {
                    @"src\Common\tests\System\Xml\XmlCoreTest\TestData.g.cs"
                }
            },
            new TestProject("roslyn-1.1.1")
            {
                Urls = new [] {
                    $"{TestHelper.GithubUrlPrefix}dotnet/roslyn/archive/version-1.1.1.zip"
                },
                IgnoredFiles = new [] {
                    @"src\Compilers\Test\Resources\Core\Encoding\sjis.cs",
                    @"src\Workspaces\Core\Portable\Shared\Extensions\SourceTextExtensions.cs"
                }
            },
            new TestProject("EntityFramework-7.0.0-rc1")
            {
                Urls = new [] {
                   $"{TestHelper.GithubUrlPrefix}aspnet/EntityFramework/archive/7.0.0-rc1.zip"
                }
            },
            new TestProject("aspnet-mvc-6.0.0-rc1")
            {
                Urls = new []
                {
                    $"{TestHelper.GithubUrlPrefix}aspnet/Mvc/archive/6.0.0-rc1.zip"
                }
            },
            new TestProject("ImageProcessor-2.3.0",
                $"{TestHelper.GithubUrlPrefix}JimBobSquarePants/ImageProcessor/archive/2.3.0.zip"),
            new TestProject("Newtonsoft.Json-8.0.2",
                $"{TestHelper.GithubUrlPrefix}/JamesNK/Newtonsoft.Json/archive/8.0.2.zip")
        };

        public static TestProject[] JavaProjects = new TestProject[]
        {
            new TestProject("WebGoat.Java-05a1f5")
            {
                Urls = new [] {
                    $"{TestHelper.GithubUrlPrefix}WebGoat/WebGoat/archive/05a1f5dd3a6c7def0e6131e4899c85f01a504ea5.zip"
                }
            }
        };

        public static TestProject[] PhpProjects = new TestProject[]
        {
            new TestProject("WebGoatPHP-6f48c9")
            {
                Urls = new []
                {
                    $"{TestHelper.GithubUrlPrefix}shivamdixit/WebGoatPHP/archive/6f48c99b4d952402c805c0e2d561f0f2c7572055.zip"
                },
                IgnoredFiles = new string[]
                {
                    @"_japp\view\default\logs\view.php"
                }
            },
            new TestProject("phpBB-3.1.6")
            {
                Urls = new []
                {
                    $"{TestHelper.GithubUrlPrefix}phpbb/phpbb/archive/release-3.1.6.zip"
                },
                IgnoredFiles = new []
                {
                    @"phpBB\develop\benchmark.php",
                    @"phpBB\develop\search_fill.php",
                    @"phpBB\install\database_update.php"
                }
            },
            new TestProject("ZendFramework-2.4.8")
            {
                Urls = new []
                {
                    $"{TestHelper.GithubUrlPrefix}zendframework/zf2/archive/release-2.4.8.zip"
                }
            }
        };

        public static TestProject[] TSqlProjects = new TestProject[]
        {
            new TestProject("TSQL Samples")
            {
                // TODO: Add samples from https://github.com/antlr/codebuff/tree/master/corpus/sql/training
                Urls = new string[0],
            }
        };

        public static TestProject[] PlSqlProjects = new TestProject[]
        {
            new TestProject("plsql-parser-66cff4")
            {
                Urls = new [] {
                    $"{TestHelper.GithubUrlPrefix}porcelli/plsql-parser/archive/66cff4c8bda5a58698a035bfb2312c326b233a58.zip",
                },
                IgnoredFiles = new []
                {
                    @"tests\function06.sql",
                    @"tests\string01.sql",
                    @"tests\xmltable01.sql"
                }
            }
        };

        public static TestProject[] JavaScriptProjects = new TestProject[]
        {
            new TestProject("bootstrap-v3.3.7")
            {
                Urls = new []
                {
                    $"{TestHelper.GithubUrlPrefix}twbs/bootstrap/archive/v3.3.7.zip"
                }
            },
            new TestProject("JavaScript-Style-Guide-v14.0.0")
            {
                Urls = new []
                {
                    $"{TestHelper.GithubUrlPrefix}airbnb/javascript/archive/eslint-config-airbnb-v14.0.0.zip"
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
