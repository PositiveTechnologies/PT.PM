using PT.PM.Common;
using System.Linq;

namespace PT.PM.TestUtils
{
    public class TestProject
    {
        private string[] ignoredFiles = new string[0];

        public string Key { get; set; }

        public string[] Urls { get; set; } = new string[0];

        public string[] IgnoredFiles
        {
            get
            {
                return ignoredFiles;
            }
            set
            {
                if (value == null)
                {
                    ignoredFiles = new string[0];
                }

                ignoredFiles = value.Select(ignoredFile => ignoredFile.NormDirSeparator()).ToArray();
            }
        }

        public TestProject(string key, string url)
            : this(key)
        {
            Urls = new string[] { url };
        }

        public TestProject(string key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return $"{Key} {Urls.FirstOrDefault() ?? ""}";
        }
    }
}
