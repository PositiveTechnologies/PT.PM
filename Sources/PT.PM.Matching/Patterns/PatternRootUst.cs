using System.Collections.Generic;
using PT.PM.Common.Nodes;
using Newtonsoft.Json;
using System;
using PT.PM.Common;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Converters;

namespace PT.PM.Matching.Patterns
{
    public class PatternRootUst : RootUst
    {
        private HashSet<Language> languages = new HashSet<Language>();
        private Regex pathWildcardRegex;

        public override UstKind Kind => UstKind.PatternRootUst;

        public string Key { get; set; } = "";

        public string FilenameWildcard { get; set; }

        public HashSet<Language> Languages
        {
            get
            {
                return languages;
            }
            set
            {
                if (languages.Contains(Language.Aspx))
                {
                    throw new ArgumentException($"Unable to create pattern for Aspx");
                }
                languages = value;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public UstNodeSerializationFormat DataFormat { get; set; } = UstNodeSerializationFormat.Json;

        [JsonIgnore]
        public string DebugInfo { get; set; } = "";

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

        [JsonIgnore]
        public List<PatternVarDef> Vars { get; set; } = new List<PatternVarDef>();

        public PatternRootUst(string key, string debugInfo, IEnumerable<Language> languages, string filenameWildcard)
            : this(null)
        {
            Key = key;
            DebugInfo = debugInfo;
            Languages = new HashSet<Language>(languages);
            FilenameWildcard = filenameWildcard;
        }

        public PatternRootUst()
            : this(null)
        {
        }

        public PatternRootUst(SourceCodeFile sourceCodeFile)
            : base(sourceCodeFile, Language.Universal)
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Nodes);
            return result.ToArray();
        }

        public override int CompareTo(Ust other)
        {
            if (other is PatternRootUst otherRoot)
            {
                return UstNodeHelper.CompareTo<Ust>(Nodes, otherRoot.Nodes);
            }

            return 1;
        }

        public override string ToString()
        {
            return (!string.IsNullOrEmpty(DebugInfo) ? DebugInfo : Key) ?? "";
        }
    }
}
