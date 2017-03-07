using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Tests
{
    public class TestProject
    {
        public string Key { get; set; }

        public Language Language { get; set; }

        public IEnumerable<string> Urls { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<string> IgnoredFiles { get; set; } = Enumerable.Empty<string>();

        public TestProject(string key, string url)
            : this(key)
        {
            Urls = new List<string>() { url };
        }

        public TestProject(string key)
        {
            Key = key;
        }
    }
}
