using Newtonsoft.Json;
using PT.PM.Common;
using System;

namespace PT.PM.Patterns
{
    public abstract class PatternBase
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

        [JsonIgnore]
        public string DebugInfo { get; set; }

        public PatternBase(string key, string debugInfo, LanguageFlags languages)
        {
            Key = key;
            DebugInfo = debugInfo;
            Languages = languages;
        }

        public PatternBase()
        {
        }

        public override string ToString()
        {
            return DebugInfo;
        }
    }
}
