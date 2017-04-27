using PT.PM.Common;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Converters;
using System;

namespace PT.PM.Patterns
{
    public class PatternDto
    {
        private LanguageFlags languages = LanguageExt.AllPatternLanguages;

        public string Name { get; set; } = "";

        public string Key { get; set; } = "";

        [JsonConverter(typeof(StringEnumConverter))]
        public LanguageFlags Languages
        {
            get
            {
                return languages;
            }
            set
            {
                if (value.Is(LanguageFlags.Aspx))
                {
                    throw new ArgumentException($"Unable to create pattern for Aspx");
                }
                languages = value;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public UstNodeSerializationFormat DataFormat { get; set; } = UstNodeSerializationFormat.Dsl;

        public string Value { get; set; } = "";

        public string CweId { get; set; } = "";

        public string Description { get; set; } = "";

        public string FileNameWildcard { get; set; } = "";

        public PatternDto()
        {
        }

        public PatternDto(Pattern pattern, UstNodeSerializationFormat dataFormat, string data)
        {
            Key = pattern.Key;
            Description = pattern.DebugInfo;
            Languages = pattern.Languages;
            FileNameWildcard = pattern.FileNameWildcard;

            DataFormat = dataFormat;
            Value = data;
        }

        public override string ToString()
        {
            var titles = LanguageExt.Languages
                .Where(lang => Languages.Is(lang))
                .Select(lang => LanguageExt.LanguageInfos[lang].Title);
            return $"{Name} ({(string.Join(", ", titles))})";
        }
    }
}
