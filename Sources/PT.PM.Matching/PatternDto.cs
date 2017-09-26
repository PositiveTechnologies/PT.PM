using PT.PM.Common;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using PT.PM.Matching.Patterns;

namespace PT.PM.Matching
{
    public class PatternDto
    {
        private HashSet<Language> languages = new HashSet<Language>(LanguageExt.AllPatternLanguages);

        public string Name { get; set; } = "";

        public string Key { get; set; } = "";

        public HashSet<Language> Languages
        {
            get
            {
                return languages;
            }
            set
            {
                if (value.Contains(Language.Aspx))
                {
                    throw new ArgumentException($"Unable to create pattern for Aspx");
                }
                languages = value;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public UstFormat DataFormat { get; set; } = UstFormat.Dsl;

        public string Value { get; set; } = "";

        public string CweId { get; set; } = "";

        public string Description { get; set; } = "";

        public string FilenameWildcard { get; set; } = "";

        public PatternDto()
        {
        }

        public PatternDto(PatternRootUst pattern, UstFormat dataFormat, string data)
        {
            Key = pattern.Key;
            Description = pattern.DebugInfo;
            Languages = pattern.Languages;
            FilenameWildcard = pattern.FilenameWildcard;

            DataFormat = dataFormat;
            Value = data;
        }

        public override string ToString()
        {
            var titles = LanguageExt.AllLanguages
                .Where(lang => Languages.Contains(lang))
                .Select(lang => LanguageExt.LanguageInfos[lang].Title);
            return $"{Name} ({(string.Join(", ", titles))})";
        }
    }
}
