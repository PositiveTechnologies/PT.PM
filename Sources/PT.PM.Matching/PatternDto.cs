using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PT.PM.Common;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public UstSerializeFormat DataFormat { get; set; } = UstSerializeFormat.Dsl;

        public string Value { get; set; } = "";

        public string CweId { get; set; } = "";

        public string Description { get; set; } = "";

        public string FilenameWildcard { get; set; } = "";

        public PatternDto()
        {
        }

        public PatternDto(PatternRootUst pattern, UstSerializeFormat dataFormat, string data)
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
