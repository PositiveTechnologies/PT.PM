using PT.PM.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class PatternDto
    {
        private HashSet<string> languages = new HashSet<string>(
            LanguageUtils.PatternLanguages.Values.Select(value => value.Key));

        public string Name { get; set; } = "";

        public string Key { get; set; } = "";

        public HashSet<string> Languages
        {
            get
            {
                return languages;
            }
            set
            {
                if (value.Contains("Aspx"))
                {
                    throw new ArgumentException($"Unable to create pattern for Aspx");
                }
                languages = value;
            }
        }

        public string DataFormat { get; set; } = "";

        public string Value { get; set; } = "";

        public string CweId { get; set; } = "";

        public string Description { get; set; } = "";

        public string FilenameWildcard { get; set; } = "";

        public PatternDto()
        {
        }

        public override string ToString()
        {
            var titles = LanguageUtils.Languages
                .Where(lang => Languages.Contains(lang.Key))
                .Select(lang => lang.Value.Title);
            return $"{Name} ({(string.Join(", ", titles))})";
        }
    }
}
