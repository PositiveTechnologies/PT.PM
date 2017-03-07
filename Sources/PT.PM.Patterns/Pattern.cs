using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Patterns.Nodes;
using Newtonsoft.Json;
using System;

namespace PT.PM.Patterns
{
    public class Pattern
    {
        private LanguageFlags languages = LanguageExt.AllPatternLanguages;

        public string Key { get; set; }

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

        public PatternNode Data { get; set; }

        public Pattern(PatternDto patternDto, PatternNode data)
        {
            Key = patternDto.Key;
            DebugInfo = patternDto.Description;
            Languages = patternDto.Languages;

            Data = data;
        }

        [JsonIgnore]
        public string DebugInfo { get; set; }

        public Pattern()
        {
        }

        public override string ToString()
        {
            return DebugInfo;
        }
    }
}
