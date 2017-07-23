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

        public string FilenameWildcard { get; set; }

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
        public Regex FilenameWildcardRegex
        {
            get
            {
                if (!string.IsNullOrEmpty(FilenameWildcard) && pathWildcardRegex == null)
                {
                    pathWildcardRegex = new WildcardConverter().Convert(FilenameWildcard);
                }
                return pathWildcardRegex;
            }
        }

        public PatternBase(string key, string debugInfo, LanguageFlags languages, string filenameWildcard)
        {
            Key = key;
            DebugInfo = debugInfo;
            Languages = languages;
            FilenameWildcard = filenameWildcard;
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
