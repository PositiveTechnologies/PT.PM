namespace PT.PM.TestUtils
{
    public static class TestProjects
    {
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
