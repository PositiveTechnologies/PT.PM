using Newtonsoft.Json;
using PT.PM.Common;
using System;
using System.Text.RegularExpressions;

namespace PT.PM.Patterns
{
    public abstract class PatternBase
    {
        private LanguageFlags languages = LanguageExt.AllPatternLanguages;
        private Regex pathWildcardRegex;

        public string Key { get; set; }

        public string FileNameWildcard { get; set; }

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

        [JsonIgnore]
        public Regex FileNameWildcardRegex
        {
            get
            {
                if (!string.IsNullOrEmpty(FileNameWildcard) && pathWildcardRegex == null)
                {
                    pathWildcardRegex = new Regex(FileNameWildcard, RegexOptions.Compiled);
                }
                return pathWildcardRegex;
            }
        }

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
